using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Razor.Templating.Core;
using SpkSnbp.Domain.Auth;
using SpkSnbp.Domain.Contracts;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Domain.Shared;
using SpkSnbp.Infrastructure.Services.FileServices;
using SpkSnbp.Web.Areas.Dashboard.Models;
using SpkSnbp.Web.Areas.Dashboard.Models.MataPelajaran;
using SpkSnbp.Web.Helpers;
using SpkSnbp.Web.Services.PDFGenerator;
using SpkSnbp.Web.Services.Toastr;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SpkSnbp.Web.Areas.Dashboard.Controllers;

[Authorize(Roles = UserRoles.Admin)]
[Area(AreaNames.Dashboard)]
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
    private readonly IRazorTemplateEngine _templateEngine;
    private readonly IPDFGeneratorService _pDFGeneratorService;
    private readonly IKelasRepository _kelasRepository;
    private readonly IHasilPerhitunganRepository _hasilPerhitunganRepository;

    public MataPelajaranController(
        ISiswaRepository siswaRepository,
        IKriteriaRepository kriteriaRepository,
        ITahunAjaranRepository tahunAjaranRepository,
        IUnitOfWork unitOfWork,
        IToastrNotificationService notificationService,
        ISiswaKriteriaRepository siswaKriteriaRepository,
        IFileService fileService,
        ITempDataDictionaryFactory tempDataDictionaryFactory,
        IRazorTemplateEngine templateEngine,
        IPDFGeneratorService pDFGeneratorService,
        IKelasRepository kelasRepository,
        IHasilPerhitunganRepository hasilPerhitunganRepository)
    {
        _siswaRepository = siswaRepository;
        _kriteriaRepository = kriteriaRepository;
        _tahunAjaranRepository = tahunAjaranRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _siswaKriteriaRepository = siswaKriteriaRepository;
        _fileService = fileService;
        _tempDataDictionaryFactory = tempDataDictionaryFactory;
        _templateEngine = templateEngine;
        _pDFGeneratorService = pDFGeneratorService;
        _kelasRepository = kelasRepository;
        _hasilPerhitunganRepository = hasilPerhitunganRepository;
    }

    public async Task<IActionResult> Index(
        Jurusan jurusan = Jurusan.TJKT,
        int? tahun = null,
        int? idKelas = null,
        bool first = true)
    {
        var tempDataDict = _tempDataDictionaryFactory.GetTempData(HttpContext);

        if (first)
        {
            var jurusanTempData = tempDataDict.Peek(TempDataKeys.Jurusan);
            var tahunTempData = tempDataDict.Peek(TempDataKeys.Tahun);
            var kelasTempData = tempDataDict.Peek(TempDataKeys.Kelas);

            if (jurusanTempData is not null)
                jurusan = (Jurusan)jurusanTempData;

            if (tahunTempData is not null)
                tahun = (int)tahunTempData;

            if (kelasTempData is not null)
                idKelas = (int)kelasTempData;
        }
        else
            tempDataDict[TempDataKeys.Jurusan] = jurusan;

        var tahunAjaran = tahun is null ?
            await _tahunAjaranRepository.Get(CultureInfos.DateOnlyNow.Year) :
            await _tahunAjaranRepository.Get(tahun.Value);

        tahunAjaran ??= await _tahunAjaranRepository.GetLatest();

        var kelas = idKelas is null ? null : await _kelasRepository.Get(idKelas.Value);

        if (!first && tahunAjaran is not null) tempDataDict[TempDataKeys.Tahun] = tahunAjaran.Id;
        if (!first) tempDataDict[TempDataKeys.Kelas] = kelas?.Id;

        return View(new IndexVM
        {
            Tahun = tahunAjaran?.Id,
            TahunAjaran = tahunAjaran,
            Kelas = kelas,
            IdKelas = kelas?.Id,
            Jurusan = jurusan,
            DaftarEntry = (await _siswaRepository.GetAll(jurusan, tahun, idKelas)).ToIndexEntryList()
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
        {
            _notificationService.AddSuccess("Simpan Berhasil");
            await _hasilPerhitunganRepository.DeleteAll();
        }
        else
            _notificationService.AddError("Simpan Gagal");

        return RedirectToActionPermanent(nameof(Index), new { vm.Jurusan, vm.Tahun, vm.IdKelas });
    }

    public async Task<IActionResult> Reset(
        Jurusan jurusan,
        int tahun,
        int? idKelas = null,
        string? returnUrl = null)
    {
        returnUrl ??= Url.Action(nameof(Index))!;

        var daftarSiswa = await _siswaRepository.GetAll(jurusan, tahun, idKelas);

        foreach (var siswa in daftarSiswa)
        {
            var siswaKriteriaMPKejuruan = siswa.DaftarSiswaKriteria.FirstOrDefault(x => x.Kriteria.Id == (int)KriteriaEnum.MPKejuruan);
            if (siswaKriteriaMPKejuruan is not null)
                _siswaKriteriaRepository.Delete(siswaKriteriaMPKejuruan);

            var siswaKriteriaMPUmum = siswa.DaftarSiswaKriteria.FirstOrDefault(x => x.Kriteria.Id == (int)KriteriaEnum.MPUmum);
            if (siswaKriteriaMPUmum is not null)
                _siswaKriteriaRepository.Delete(siswaKriteriaMPUmum);
        }

        var result = await _unitOfWork.SaveChangesAsync();
        if (result.IsSuccess)
        {
            _notificationService.AddSuccess("Reset Berhasil!");
            await _hasilPerhitunganRepository.DeleteAll();
        }
        else
            _notificationService.AddSuccess("Reset Gagal!");

        return RedirectPermanent(returnUrl);
    }

    [HttpPost]
    public async Task<IActionResult> Import(ImportVM vm)
    {
        var returnUrl = vm.ReturnUrl ?? Url.ActionLink(nameof(Index))!;

        if (!ModelState.IsValid || vm.IdKelas is null)
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

        var kelas = await _kelasRepository.Get(vm.IdKelas.Value);
        if (kelas is null)
        {
            _notificationService.AddError("Kelas tidak ditemukan", "Import");
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
        using var spreadSheet = SpreadsheetDocument.Open(memoryStream, false);

        var workBookPart = spreadSheet.WorkbookPart!;
        var sharedStrings = workBookPart
            .SharedStringTablePart?
            .SharedStringTable
            .Elements<SharedStringItem>()
            .Select(s => s.InnerText).ToList() ?? [];

        var sheet = workBookPart.Workbook.Sheets!.Elements<Sheet>().First();
        var workSheetPart = (WorksheetPart)workBookPart.GetPartById(sheet.Id!);
        var sheetData = workSheetPart.Worksheet.Elements<SheetData>().First();

        var daftarSiswa = await _siswaRepository.GetAll(vm.Jurusan, vm.Tahun, vm.IdKelas);

        int jumlahNilaiDiupdate = 0;
        int jumlahSiswaDitemukan = 0;
        int jumlahSiswaTidakDitemukan = 0;

        // ================= VALIDASI KOLOM C = NISN =================
        var headerRows = sheetData.Elements<Row>()
            .Where(r => r.RowIndex >= 4 && r.RowIndex <= 7);

        bool nisnValid = false;

        foreach (var row in headerRows)
        {
            var cellC = row.Elements<Cell>()
                .FirstOrDefault(x => x.CellReference!.Value!.StartsWith("C"));

            if (cellC is null) continue;

            var value = Normalize(HelperFunctions.GetCellValues(cellC, sharedStrings));

            if (value.Contains("nisn"))
            {
                nisnValid = true;
                break;
            }
        }

        if (!nisnValid)
        {
            _notificationService.AddError("Format salah: Kolom C harus berisi NISN", "Import");
            return RedirectPermanent(returnUrl);
        }

        // ================= AMBIL KOLOM MAPEL =================
        var mapelUmumCol = FindColumnByHeader(sheetData, sharedStrings, "mapel umum", 4, 7);
        var mapelKejuruanCol = FindColumnByHeader(sheetData, sharedStrings, "mapel kejuruan", 4, 7);

        if (mapelUmumCol is null || mapelKejuruanCol is null)
        {
            _notificationService.AddError("Kolom Mapel tidak ditemukan", "Import");
            return RedirectPermanent(returnUrl);
        }

        // ================= LOOP DATA =================
        foreach (var row in sheetData.Elements<Row>().Where(r => r.RowIndex > 7))
        {
            var nisnCell = row.Elements<Cell>()
                .FirstOrDefault(x => x.CellReference!.Value!.StartsWith("C"));

            if (nisnCell is null) continue;

            var nisn = HelperFunctions.GetCellValues(nisnCell, sharedStrings);
            if (string.IsNullOrWhiteSpace(nisn)) continue;

            if (!nisn.All(char.IsDigit)) continue;

            var siswa = daftarSiswa.FirstOrDefault(x => x.NISN == nisn);

            if (siswa is null)
            {
                jumlahSiswaTidakDitemukan++;
                continue;
            }

            jumlahSiswaDitemukan++;

            // ===== MAPEL UMUM =====
            var mapelUmumCell = row.Elements<Cell>()
                .FirstOrDefault(x => x.CellReference!.Value!.StartsWith(mapelUmumCol));

            if (mapelUmumCell != null)
            {
                var value = HelperFunctions.GetCellValues(mapelUmumCell, sharedStrings);

                if (double.TryParse(value, CultureInfo.InvariantCulture, out var nilai))
                {
                    var data = siswa.DaftarSiswaKriteria
                        .FirstOrDefault(x => x.IdKriteria == (int)KriteriaEnum.MPUmum);

                    if (data is null)
                    {
                        _siswaKriteriaRepository.Add(new SiswaKriteria
                        {
                            Siswa = siswa,
                            IdKriteria = (int)KriteriaEnum.MPUmum,
                            Nilai = nilai
                        });

                        jumlahNilaiDiupdate++;
                    }
                    else if (data.Nilai != nilai)
                    {
                        data.Nilai = nilai;
                        jumlahNilaiDiupdate++;
                    }
                }
            }

            // ===== MAPEL KEJURUAN =====
            var mapelKejuruanCell = row.Elements<Cell>()
                .FirstOrDefault(x => x.CellReference!.Value!.StartsWith(mapelKejuruanCol));

            if (mapelKejuruanCell != null)
            {
                var value = HelperFunctions.GetCellValues(mapelKejuruanCell, sharedStrings);

                if (double.TryParse(value, CultureInfo.InvariantCulture, out var nilai))
                {
                    var data = siswa.DaftarSiswaKriteria
                        .FirstOrDefault(x => x.IdKriteria == (int)KriteriaEnum.MPKejuruan);

                    if (data is null)
                    {
                        _siswaKriteriaRepository.Add(new SiswaKriteria
                        {
                            Siswa = siswa,
                            IdKriteria = (int)KriteriaEnum.MPKejuruan,
                            Nilai = nilai
                        });

                        jumlahNilaiDiupdate++;
                    }
                    else if (data.Nilai != nilai)
                    {
                        data.Nilai = nilai;
                        jumlahNilaiDiupdate++;
                    }
                }
            }
        }

        // ================= VALIDASI FINAL =================
        if (jumlahSiswaDitemukan == 0)
        {
            _notificationService.AddError(
                "NISN siswa tidak ditemukan, silakan tambah siswa di Data Siswa",
                "Import");

            return RedirectPermanent(returnUrl);
        }

        var result = await _unitOfWork.SaveChangesAsync();

        // ================= TOASTR =================
        if (result.IsSuccess)
        {
            if (jumlahNilaiDiupdate == 0)
            {
                _notificationService.AddWarning(
                    "Import berhasil, tetapi tidak ada perubahan nilai",
                    "Import");
            }
            else
            {
                _notificationService.AddSuccess(
                    $"Import berhasil. {jumlahNilaiDiupdate} data diperbarui",
                    "Import");

                await _hasilPerhitunganRepository.DeleteAll();
            }
        }
        else
        {
            _notificationService.AddError("Import Gagal", "Import");
        }

        return RedirectPermanent(returnUrl);
    }

    // ============= HELPER ============
    public static string Normalize(string text)
    {
        return Regex.Replace((text ?? "").ToLower(), @"[^a-z0-9]", "");
    }

    private string? FindColumnByHeader(
        SheetData sheetData,
        List<string> sharedStrings,
        string keyword,
        int startRow,
        int endRow)
    {
        var normalizedKeyword = Normalize(keyword);

        foreach (var row in sheetData.Elements<Row>()
                                    .Where(r => r.RowIndex >= startRow && r.RowIndex <= endRow))
        {
            foreach (var cell in row.Elements<Cell>())
            {
                var value = Normalize(HelperFunctions.GetCellValues(cell, sharedStrings));

                if (value.Contains(normalizedKeyword))
                {
                    return Regex.Match(cell.CellReference!.Value!, @"[A-Z]+").Value;
                }
            }
        }

        return null;
    }

    public async Task<IActionResult> PDF(int tahun, Jurusan jurusan, int? idKelas = null)
    {
        var kelas = idKelas is null ? null : await _kelasRepository.Get(idKelas.Value);
        var daftarSiswa = await _siswaRepository.GetAll(jurusan, tahun, idKelas);

        var html = await _templateEngine.RenderAsync("Areas/Dashboard/Views/MataPelajaran/PDF.cshtml", daftarSiswa);

        var pdf = await _pDFGeneratorService.GeneratePDF(
            html,
            marginTop: 75,
            marginBottom: 75,
            marginLeft: 75,
            marginRight: 75);

        return File(
            pdf,
            "application/pdf",
            fileDownloadName: $"Mata Pelajaran-{tahun}-{jurusan}{(kelas is null ? "" : $"-{kelas.Nama}")}.pdf"
        );
    }

    public async Task<IActionResult> Excel(int tahun, Jurusan jurusan, int? idKelas = null)
    {
        var kelas = idKelas is null ? null : await _kelasRepository.Get(idKelas.Value);
        var daftarSiswa = await _siswaRepository.GetAll(jurusan, tahun, idKelas);

        using var memoryStream = new MemoryStream();
        using var spreadSheet = SpreadsheetDocument.Create(memoryStream, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);

        var workBookPart = spreadSheet.AddWorkbookPart();
        workBookPart.Workbook = new Workbook();

        var stylePart = workBookPart.AddNewPart<WorkbookStylesPart>();
        stylePart.Stylesheet = new()
        {
            Fonts = new
            (
                new Font { FontSize = new() { Val = 11 }, FontName = new() { Val = "Calibri" } }
            )
            { Count = 1 },

            Fills = new
            (
                new Fill { PatternFill = new() { PatternType = PatternValues.None } },
                new Fill { PatternFill = new() { PatternType = PatternValues.Gray125 } }
            )
            { Count = 2 },

            Borders = new
            (
                new Border(),
                new Border
                {
                    BottomBorder = new() { Style = BorderStyleValues.Thin, Color = new() { Auto = true } },
                    TopBorder = new() { Style = BorderStyleValues.Thin, Color = new() { Auto = true } },
                    LeftBorder = new() { Style = BorderStyleValues.Thin, Color = new() { Auto = true } },
                    RightBorder = new() { Style = BorderStyleValues.Thin, Color = new() { Auto = true } },
                }
            )
            { Count = 2 },

            CellStyleFormats = new
            (
                new CellFormat()
            )
            { Count = 1 },

            CellFormats = new
            (
                new CellFormat(),
                new CellFormat(new Alignment { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center })
                {
                    FormatId = 0,
                    FontId = 0,
                    BorderId = 1,
                    FillId = 0,
                    ApplyFill = true,
                    ApplyBorder = true,
                    ApplyFont = true
                },
                new CellFormat(new Alignment { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center, WrapText = true })
                {
                    FormatId = 0,
                    FontId = 0,
                    BorderId = 1,
                    FillId = 0,
                    ApplyFill = true,
                    ApplyBorder = true,
                    ApplyFont = true
                }
            )
            { Count = 3 }
        };

        var worksheetPart = workBookPart.AddNewPart<WorksheetPart>();
        var sheetData = new SheetData();
        worksheetPart.Worksheet = new Worksheet(
            new Columns(
                new Column
                {
                    Min = 1,
                    Max = 1,
                    Width = 40,
                    CustomWidth = true,
                },
                new Column
                {
                    Min = 2,
                    Max = 2,
                    Width = 24,
                    CustomWidth = true,
                },
                new Column
                {
                    Min = 3,
                    Max = 3,
                    Width = 15,
                    CustomWidth = true,
                },
                new Column
                {
                    Min = 4,
                    Max = 4,
                    Width = 15,
                    CustomWidth = true,
                },
                new Column
                {
                    Min = 5,
                    Max = 5,
                    Width = 15,
                    CustomWidth = true,
                },
                new Column
                {
                    Min = 6,
                    Max = 6,
                    Width = 24,
                    CustomWidth = true,
                },
                new Column
                {
                    Min = 7,
                    Max = 7,
                    Width = 24,
                    CustomWidth = true,
                }
            ),
            sheetData);

        var sheets = workBookPart.Workbook.AppendChild(new Sheets());
        var relationshipId = workBookPart.GetIdOfPart(worksheetPart);

        var sheetId = 1u;
        var sheetName = "Sheet" + sheetId;
        var sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = sheetName };
        sheets.Append(sheet);

        var headerRow = new Row() { RowIndex = 1 };
        headerRow.Append(
            [
                new Cell
                {
                    CellReference = $"A{headerRow.RowIndex}",
                    CellValue = new CellValue("Nama"),
                    StyleIndex = 2,
                },
                new Cell
                {
                    CellReference = $"B{headerRow.RowIndex}",
                    CellValue = new CellValue("NISN"),
                    StyleIndex = 1,
                },
                new Cell
                {
                    CellReference = $"C{headerRow.RowIndex}",
                    CellValue = new CellValue("Jurusan"),
                    StyleIndex = 1,
                },
                new Cell
                {
                    CellReference = $"D{headerRow.RowIndex}",
                    CellValue = new CellValue("Kelas"),
                    StyleIndex = 1,
                },
                new Cell
                {
                    CellReference = $"E{headerRow.RowIndex}",
                    CellValue = new CellValue("Tahun Ajaran"),
                    StyleIndex = 1,
                },
                new Cell
                {
                    CellReference = $"F{headerRow.RowIndex}",
                    CellValue = new CellValue($"(C{(int)KriteriaEnum.MPKejuruan}) {KriteriaEnum.MPKejuruan.Humanize()}"),
                    StyleIndex = 2,
                },
                new Cell
                {
                    CellReference = $"G{headerRow.RowIndex}",
                    CellValue = new CellValue($"(C{(int)KriteriaEnum.MPUmum}) {KriteriaEnum.MPUmum.Humanize()}"),
                    StyleIndex = 2,
                }
            ]
        );
        sheetData.Append(headerRow);

        for (int i = 0; i < daftarSiswa.Count; i++)
        {
            var rowIndex = headerRow.RowIndex + (uint)i + 1u;
            var siswa = daftarSiswa[i];

            var row = new Row { RowIndex = rowIndex };
            row.Append([
                new Cell
                {
                    CellReference = $"A{row.RowIndex}",
                    CellValue = new CellValue(siswa.Nama),
                    StyleIndex = 2,
                },
                new Cell
                {
                    CellReference = $"B{row.RowIndex}",
                    CellValue = new CellValue(siswa.NISN),
                    StyleIndex = 1,
                },
                new Cell
                {
                    CellReference = $"C{row.RowIndex}",
                    CellValue = new CellValue(siswa.Jurusan.Humanize()),
                    StyleIndex = 1,
                },
                new Cell
                {
                    CellReference = $"D{row.RowIndex}",
                    CellValue = new CellValue(siswa.Kelas.Nama),
                    StyleIndex = 1,
                },
                new Cell
                {
                    CellReference = $"E{row.RowIndex}",
                    CellValue = new CellValue(siswa.TahunAjaran.Id),
                    StyleIndex = 1,
                },
                new Cell
                {
                    CellReference = $"F{row.RowIndex}",
                    CellValue = new(siswa.DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == (int)KriteriaEnum.MPKejuruan)?.Nilai.ToString()
                        ?? "-"),
                    StyleIndex = 2,
                },
                new Cell
                {
                    CellReference = $"G{row.RowIndex}",
                    CellValue = new(siswa.DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == (int)KriteriaEnum.MPUmum)?.Nilai.ToString()
                        ?? "-"),
                    StyleIndex = 2,
                }
            ]);

            sheetData.Append(row);
        }

        spreadSheet.Save();

        return File(
            memoryStream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileDownloadName: $"Mata Pelajaran-{tahun}-{jurusan}{(kelas is null ? "" : $"-{kelas.Nama}")}.xlsx"
        );
    }
}
