using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
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
using SpkSnbp.Web.Models.MataPelajaran;
using SpkSnbp.Web.Services.Toastr;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SpkSnbp.Web.Controllers;

[Authorize(Roles = UserRoles.Admin)]
public class MataPelajaranController : Controller
{
    private readonly ISiswaRepository _siswaRepository;
    private readonly IKriteriaRepository _kriteriaRepository;
    private readonly ITahunAjaranRepository _tahunAjaranRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IToastrNotificationService _notificationService;
    private readonly ISiswaKriteriaRepository _siswaKriteriaRepository;
    private readonly IFileService _fileService;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;

    public MataPelajaranController(
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

    [HttpPost]
    public async Task<IActionResult> Simpan(IndexVM vm)
    {
        foreach (var entry in vm.DaftarEntry)
        {
            var siswa = await _siswaRepository.Get(entry.IdSiswa);
            if (siswa is null) continue;

            var siswaKriteriaMPKejuruan = siswa.DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == (int)KriteriaEnum.MPKejuruan);

            if (siswaKriteriaMPKejuruan is null)
            {
                siswaKriteriaMPKejuruan = new SiswaKriteria
                {
                    IdSiswa = siswa.Id,
                    IdKriteria = (int)KriteriaEnum.MPKejuruan,
                    Nilai = default
                };

                _siswaKriteriaRepository.Add(siswaKriteriaMPKejuruan);
            }

            siswaKriteriaMPKejuruan.Nilai = entry.MataPelajaranKejuruan;

            var siswaKriteriaMPUmum = siswa.DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == (int)KriteriaEnum.MPUmum);
            if (siswaKriteriaMPUmum is null)
            {
                siswaKriteriaMPUmum = new SiswaKriteria
                {
                    IdSiswa = siswa.Id,
                    IdKriteria = (int)KriteriaEnum.MPUmum,
                    Nilai = default
                };

                _siswaKriteriaRepository.Add(siswaKriteriaMPUmum);
            }

            siswaKriteriaMPUmum.Nilai = entry.MataPelajaranUmum;
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

        var mapelUmumCellRef = sheetData
            .Descendants<Cell>()
            .FirstOrDefault(x => HelperFunctions.GetCellValues(x, sharedStrings).Trim().ToLower() == "mapel umum")?
            .CellReference?.Value;

        if (mapelUmumCellRef is null)
        {
            _notificationService.AddError("Tidak ada kolom Mapel Umum", "Import");
            return RedirectPermanent(returnUrl);
        }

        var match = Regex.Match(mapelUmumCellRef, @"(?<kolom>[A-Z]*)[1-2]*");
        if (!match.Success || !match.Groups.TryGetValue("kolom", out var kolomGroup))
        {
            _notificationService.AddError("Tidak ada kolom Mapel Umum", "Import");
            return RedirectPermanent(returnUrl);
        }

        mapelUmumCellRef = kolomGroup.Value;

        var mapelKejuruanCellRef = sheetData
            .Descendants<Cell>()
            .FirstOrDefault(x => HelperFunctions.GetCellValues(x, sharedStrings).ToLower() == "mapel kejuruan")?
            .CellReference?.Value;

        if (mapelKejuruanCellRef is null)
        {
            _notificationService.AddError("Tidak ada kolom Mapel kejuruan", "Import");
            return RedirectPermanent(returnUrl);
        }

        match = Regex.Match(mapelKejuruanCellRef, @"(?<kolom>[A-Z]*)[1-2]*");
        if (!match.Success || !match.Groups.TryGetValue("kolom", out kolomGroup))
        {
            _notificationService.AddError("Tidak ada kolom Mapel Umum", "Import");
            return RedirectPermanent(returnUrl);
        }

        mapelKejuruanCellRef = kolomGroup.Value;

        foreach (var row in sheetData.Elements<Row>().Skip(7))
        {
            var nisnCell = row.Elements<Cell>().FirstOrDefault(x => x.CellReference!.Value!.StartsWith("C"));
            if (nisnCell is null) continue;

            var nisn = HelperFunctions.GetCellValues(nisnCell, sharedStrings);
            if (string.IsNullOrWhiteSpace(nisn)) continue;

            var siswa = daftarSiswa.FirstOrDefault(x => x.NISN == nisn);
            if (siswa is null)
            {
                var namaCell = row.Elements<Cell>().FirstOrDefault(x => x.CellReference!.Value!.StartsWith("B"));
                if (namaCell is null) continue;

                var nama = HelperFunctions.GetCellValues(namaCell, sharedStrings);
                if (string.IsNullOrWhiteSpace(nama) || await _siswaRepository.IsExist(nisn)) continue;

                siswa = new Siswa
                {
                    NISN = nisn,
                    Nama = nama,
                    Jurusan = vm.Jurusan,
                    TahunAjaran = tahunAjaran
                };

                _siswaRepository.Add(siswa);
                daftarSiswa.Add(siswa);
            }

            var mapelUmumCell = row.Elements<Cell>().FirstOrDefault(x => x.CellReference!.Value!.StartsWith(mapelUmumCellRef));
            if (mapelUmumCell is null) continue;

            var mapelUmumString = HelperFunctions.GetCellValues(mapelUmumCell, sharedStrings);
            if (string.IsNullOrWhiteSpace(mapelUmumString) ||
                !double.TryParse(mapelUmumString, CultureInfo.InvariantCulture, out var mapelUmum) ||
                mapelUmum < 0 ||
                mapelUmum > 100)
                continue;

            var mapelKejuruanCell = row.Elements<Cell>().FirstOrDefault(x => x.CellReference!.Value!.StartsWith(mapelKejuruanCellRef));
            if (mapelKejuruanCell is null) continue;

            var mapelKejuruanString = HelperFunctions.GetCellValues(mapelKejuruanCell, sharedStrings);
            if (string.IsNullOrWhiteSpace(mapelKejuruanString) ||
                !double.TryParse(mapelKejuruanString, CultureInfo.InvariantCulture, out var mapelKejuruan) ||
                mapelKejuruan < 0 ||
                mapelKejuruan > 100)
                continue;

            var siswaKriteriaMPUmum = siswa.DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == (int)KriteriaEnum.MPUmum);
            if (siswaKriteriaMPUmum is null)
            {
                siswaKriteriaMPUmum = new SiswaKriteria
                {
                    Siswa = siswa,
                    IdKriteria = (int)KriteriaEnum.MPUmum,
                    Nilai = default
                };

                _siswaKriteriaRepository.Add(siswaKriteriaMPUmum);
            }

            siswaKriteriaMPUmum.Nilai = mapelUmum;

            var siswaKriteriaMPKejuruan = siswa.DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == (int)KriteriaEnum.MPKejuruan);
            if (siswaKriteriaMPKejuruan is null)
            {
                siswaKriteriaMPKejuruan = new SiswaKriteria
                {
                    Siswa = siswa,
                    IdKriteria = (int)KriteriaEnum.MPKejuruan,
                    Nilai = default
                };

                _siswaKriteriaRepository.Add(siswaKriteriaMPKejuruan);
            }

            siswaKriteriaMPKejuruan.Nilai = mapelKejuruan;
        }

        var result = await _unitOfWork.SaveChangesAsync();
        if (result.IsSuccess)
            _notificationService.AddSuccess("Import Berhasil", "Import");
        else
            _notificationService.AddError("Import Gagal", "Import");

        return RedirectPermanent(returnUrl);
    }
}
