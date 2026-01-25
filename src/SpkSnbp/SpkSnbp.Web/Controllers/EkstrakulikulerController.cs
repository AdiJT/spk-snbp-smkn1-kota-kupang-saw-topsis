using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Office;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SpkSnbp.Domain.Auth;
using SpkSnbp.Domain.Contracts;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Domain.Shared;
using SpkSnbp.Infrastructure.Services.FileServices;
using SpkSnbp.Web.Helpers;
using SpkSnbp.Web.Models;
using SpkSnbp.Web.Models.Ekstrakulikuler;
using SpkSnbp.Web.Services.Toastr;

namespace SpkSnbp.Web.Controllers;

[Authorize(Roles = UserRoles.Admin)]
public class EkstrakulikulerController : Controller
{
    private readonly ISiswaRepository _siswaRepository;
    private readonly IKriteriaRepository _kriteriaRepository;
    private readonly ITahunAjaranRepository _tahunAjaranRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IToastrNotificationService _notificationService;
    private readonly ISiswaKriteriaRepository _siswaKriteriaRepository;
    private readonly IFileService _fileService;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;

    public EkstrakulikulerController(
        ISiswaRepository siswaRepository,
        IKriteriaRepository kriteriaRepository,
        ITahunAjaranRepository tahunAjaranRepository,
        IUnitOfWork unitOfWork,
        IToastrNotificationService notificationService,
        ISiswaKriteriaRepository siswaKriteriaRepository,
        IFileService fileService,
        ITempDataDictionaryFactory tempDataDictionaryFactory)
    {
        _siswaRepository = siswaRepository;
        _kriteriaRepository = kriteriaRepository;
        _tahunAjaranRepository = tahunAjaranRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _siswaKriteriaRepository = siswaKriteriaRepository;
        _fileService = fileService;
        _tempDataDictionaryFactory = tempDataDictionaryFactory;
    }

    public async Task<IActionResult> Index(Jurusan jurusan = Jurusan.TJKT, int? tahun = null, bool first = true)
    {
        var tempDataDict = _tempDataDictionaryFactory.GetTempData(HttpContext);

        if (first)
        {
            var jurusanTempData = tempDataDict.Peek(TempDataKeys.Jurusan);
            var tahunTempData = tempDataDict.Peek(TempDataKeys.Tahun);

            if (jurusanTempData is not null)
                jurusan = (Jurusan)jurusanTempData;

            if (tahunTempData is not null)
                tahun = (int)tahunTempData;
        }
        else
            tempDataDict[TempDataKeys.Jurusan] = jurusan;

        var tahunAjaran = tahun is null ?
            await _tahunAjaranRepository.Get(CultureInfos.DateOnlyNow.Year) :
            await _tahunAjaranRepository.Get(tahun.Value);

        tahunAjaran ??= await _tahunAjaranRepository.GetLatest();

        if (tahunAjaran is null)
            return View(new IndexVM { Jurusan = jurusan, DaftarEntry = (await _siswaRepository.GetAll(jurusan, tahun)).ToIndexEntryList() });

        if (!first) tempDataDict[TempDataKeys.Tahun] = tahunAjaran.Id;

        return View(new IndexVM
        {
            Tahun = tahunAjaran.Id,
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
            if (siswa is null || entry.DaftarEkskul.Count == 0) continue;

            var siswaKriteria = siswa.DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == (int)KriteriaEnum.Ekstrakulikuler);

            if (siswaKriteria is null)
            {
                siswaKriteria = new SiswaKriteria
                {
                    IdSiswa = siswa.Id,
                    IdKriteria = (int)KriteriaEnum.Ekstrakulikuler,
                    Nilai = default
                };

                _siswaKriteriaRepository.Add(siswaKriteria);
            }

            siswaKriteria.Nilai = entry.DaftarEkskul.Select(x => (double)(int)x).Average() * entry.DaftarEkskul.Count;

            siswa.Ekstrakulikuler1 = entry.Ekstrakulikuler1;
            siswa.Ekstrakulikuler2 = entry.Ekstrakulikuler2;
            siswa.Ekstrakulikuler3 = entry.Ekstrakulikuler3;
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
            if (cells.Count < 8) continue;

            var nama = HelperFunctions.GetCellValues(cells[1], sharedStrings);
            if (string.IsNullOrWhiteSpace(nama)) continue;

            var siswa = daftarSiswa.FirstOrDefault(x => x.Nama.ToLower() == nama.ToLower());
            if (siswa is null) continue;

            var ekstrakulikuler1String = HelperFunctions.GetCellValues(cells[5], sharedStrings);
            PredikatEkstrakulikuler? ekstrakulikuler1 = null;
            if (!string.IsNullOrWhiteSpace(ekstrakulikuler1String))
                ekstrakulikuler1 = ekstrakulikuler1String.Trim().DehumanizeTo<PredikatEkstrakulikuler>(OnNoMatch.ReturnsNull);

            var ekstrakulikuler2String = HelperFunctions.GetCellValues(cells[6], sharedStrings);
            PredikatEkstrakulikuler? ekstrakulikuler2 = null;
            if (!string.IsNullOrWhiteSpace(ekstrakulikuler2String))
                ekstrakulikuler2 = ekstrakulikuler2String.Trim().Transform(To.SentenceCase).DehumanizeTo<PredikatEkstrakulikuler>(OnNoMatch.ReturnsNull);

            var ekstrakulikuler3String = HelperFunctions.GetCellValues(cells[7], sharedStrings);
            PredikatEkstrakulikuler? ekstrakulikuler3 = null;
            if (!string.IsNullOrWhiteSpace(ekstrakulikuler3String))
                ekstrakulikuler3 = ekstrakulikuler3String.Trim().DehumanizeTo<PredikatEkstrakulikuler>(OnNoMatch.ReturnsNull);

            if (ekstrakulikuler1 is null && ekstrakulikuler2 is null && ekstrakulikuler3 is null)
                continue;

            var siswaKriteria = siswa.DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == (int)KriteriaEnum.Ekstrakulikuler);
            if (siswaKriteria is null)
            {
                siswaKriteria = new SiswaKriteria
                {
                    Siswa = siswa,
                    IdKriteria = (int)KriteriaEnum.Ekstrakulikuler,
                    Nilai = default
                };

                _siswaKriteriaRepository.Add(siswaKriteria);
            }

            var total = 0d;
            var jumlah = 0;

            if (ekstrakulikuler1 is not null)
            {
                total += (int)ekstrakulikuler1;
                jumlah++;
            }

            if (ekstrakulikuler2 is not null)
            {
                total += (int)ekstrakulikuler2;
                jumlah++;
            }

            if (ekstrakulikuler3 is not null)
            {
                total += (int)ekstrakulikuler3;
                jumlah++;
            }

            siswaKriteria.Nilai = total / jumlah * jumlah;

            siswa.Ekstrakulikuler1 = ekstrakulikuler1;
            siswa.Ekstrakulikuler2 = ekstrakulikuler2;
            siswa.Ekstrakulikuler3 = ekstrakulikuler3;
        }

        var result = await _unitOfWork.SaveChangesAsync();
        if (result.IsSuccess)
            _notificationService.AddSuccess("Import Berhasil", "Import");
        else
            _notificationService.AddError("Import Gagal", "Import");

        return RedirectPermanent(returnUrl);
    }
}
