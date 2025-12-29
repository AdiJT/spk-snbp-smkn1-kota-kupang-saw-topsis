using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpkSnbp.Domain.Auth;
using SpkSnbp.Domain.Contracts;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Infrastructure.Services.FileServices;
using SpkSnbp.Web.Helpers;
using SpkSnbp.Web.Models;
using SpkSnbp.Web.Models.SertifikatLSPModels;
using SpkSnbp.Web.Services.Toastr;
using System.Text.RegularExpressions;

namespace SpkSnbp.Web.Controllers;

[Authorize(Roles = UserRoles.Admin)]
public class SertifikatLSPController : Controller
{
    private readonly ISiswaRepository _siswaRepository;
    private readonly IKriteriaRepository _kriteriaRepository;
    private readonly ITahunAjaranRepository _tahunAjaranRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IToastrNotificationService _notificationService;
    private readonly ISiswaKriteriaRepository _siswaKriteriaRepository;
    private readonly IFileService _fileService;

    public SertifikatLSPController(
        ISiswaRepository siswaRepository,
        IKriteriaRepository kriteriaRepository,
        ITahunAjaranRepository tahunAjaranRepository,
        IUnitOfWork unitOfWork,
        IToastrNotificationService notificationService,
        ISiswaKriteriaRepository siswaKriteriaRepository,
        IFileService fileService)
    {
        _siswaRepository = siswaRepository;
        _kriteriaRepository = kriteriaRepository;
        _tahunAjaranRepository = tahunAjaranRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _siswaKriteriaRepository = siswaKriteriaRepository;
        _fileService = fileService;
    }

    public async Task<IActionResult> Index(Jurusan? jurusan = null, int? tahun = null)
    {
        var tahunAjaran = tahun is null ? null : await _tahunAjaranRepository.Get(tahun.Value);
        if (tahunAjaran is null)
            return View(new IndexVM { Jurusan = jurusan, DaftarEntry = (await _siswaRepository.GetAll(jurusan)).ToIndexEntryList() });

        return View(new IndexVM
        {
            Tahun = tahun,
            TahunAjaran = tahunAjaran,
            Jurusan = jurusan,
            DaftarEntry = (await _siswaRepository.GetAll(jurusan, tahun)).ToIndexEntryList()
        });
    }

    public async Task<IActionResult> Simpan(IndexVM vm)
    {
        foreach (var entry in vm.DaftarEntry)
        {
            var siswa = await _siswaRepository.Get(entry.IdSiswa);
            if (siswa is null || entry.SertifikatLSP is null) continue;

            var siswaKriteria = siswa.DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == (int)KriteriaEnum.SertLSP);

            if (siswaKriteria is null)
            {
                siswaKriteria = new SiswaKriteria
                {
                    IdSiswa = siswa.Id,
                    IdKriteria = (int)KriteriaEnum.SertLSP,
                    Nilai = default
                };

                _siswaKriteriaRepository.Add(siswaKriteria);
            }

            siswaKriteria.Nilai = (int)entry.SertifikatLSP;
        }

        var result = await _unitOfWork.SaveChangesAsync();
        if (result.IsSuccess)
            _notificationService.AddSuccess("Simpan Berhasil");
        else
            _notificationService.AddError("Simpan Gagal");

        return RedirectToActionPermanent(nameof(Index), new { vm.Jurusan, vm.Tahun });
    }

    [HttpPost]
    public async Task<IActionResult> Import(ImportVM vm)
    {
        var returnUrl = vm.ReturnUrl ?? Url.ActionLink(nameof(Index))!;

        if (!ModelState.IsValid)
        {
            _notificationService.AddError("Data tidak valid", "Import");
            return RedirectPermanent(returnUrl);
        }

        var tahunAjaran = await _tahunAjaranRepository.Get(vm.Tahun);
        if (tahunAjaran is null)
        {
            _notificationService.AddError("Tahun tidak ditemukan", "Import");
            return RedirectPermanent(returnUrl);
        }

        if (vm.FormFile is null)
        {
            _notificationService.AddError("File harus diupload", "Import");
            return RedirectPermanent(returnUrl);
        }

        var file = await _fileService.ProcessFormFile<ImportVM>(
            vm.FormFile,
            [".xlsx"],
            0,
            long.MaxValue);

        if (file.IsFailure)
        {
            _notificationService.AddError(file.Error.Message, "Import");
            return View(vm);
        }

        using var memoryStream = new MemoryStream(file.Value);
        using var spreadSheet = SpreadsheetDocument.Open(memoryStream, isEditable: false);

        var workBookPart = spreadSheet.WorkbookPart!;
        var sharedStrings = workBookPart
            .SharedStringTablePart?
            .SharedStringTable
            .Elements<SharedStringItem>()
            .Select(s => s.InnerText).ToList() ?? [];

        var sheet = workBookPart.Workbook.Sheets!.Elements<Sheet>().First()!;
        var workSheetPart = (WorksheetPart)workBookPart.GetPartById(sheet.Id!);
        var sheetData = workSheetPart.Worksheet.Elements<SheetData>().First();

        var daftarSiswa = await _siswaRepository.GetAll(vm.Jurusan, vm.Tahun);

        foreach (var row in sheetData.Elements<Row>())
        {
            var cells = row.Elements<Cell>().ToList();
            if (cells.Count < 4) continue;

            var nama = HelperFunctions.GetCellValues(cells[1], sharedStrings);
            if (string.IsNullOrWhiteSpace(nama)) continue;

            var sertifikatLSP = HelperFunctions.GetCellValues(cells[3], sharedStrings);
            if (string.IsNullOrWhiteSpace(sertifikatLSP)) continue;
            sertifikatLSP = sertifikatLSP.ToLower();
            if (sertifikatLSP != "bk" && sertifikatLSP != "k") continue;

            var siswa = daftarSiswa.FirstOrDefault(x => x.Nama.ToLower() == nama.ToLower());
            if (siswa is null) continue;

            var siswaKriteria = siswa.DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == (int)KriteriaEnum.SertLSP);
            if (siswaKriteria is null)
            {
                siswaKriteria = new SiswaKriteria
                {
                    Siswa = siswa,
                    IdKriteria = (int)KriteriaEnum.SertLSP,
                    Nilai = default
                };

                _siswaKriteriaRepository.Add(siswaKriteria);
            }

            siswaKriteria.Nilai = sertifikatLSP switch
            {
                "bk" => 1,
                "k" => 5,
                _ => throw new NotImplementedException()
            };
        }

        var result = await _unitOfWork.SaveChangesAsync();
        if (result.IsSuccess)
            _notificationService.AddSuccess("Import Berhasil", "Import");
        else
            _notificationService.AddError("Import Gagal", "Import");

        return RedirectPermanent(returnUrl);
    }
}
