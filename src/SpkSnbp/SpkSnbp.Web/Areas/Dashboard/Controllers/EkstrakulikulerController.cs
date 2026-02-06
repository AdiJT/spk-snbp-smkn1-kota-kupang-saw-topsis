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
using SpkSnbp.Web.Areas.Dashboard.Models.Ekstrakulikuler;
using SpkSnbp.Web.Helpers;
using SpkSnbp.Web.Services.PDFGenerator;
using SpkSnbp.Web.Services.Toastr;

namespace SpkSnbp.Web.Areas.Dashboard.Controllers;

[Authorize(Roles = UserRoles.Admin)]
[Area(AreaNames.Dashboard)]
public class EkstrakulikulerController : Controller
{
    private readonly ISiswaRepository _siswaRepository;
    private readonly ITahunAjaranRepository _tahunAjaranRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IToastrNotificationService _notificationService;
    private readonly ISiswaKriteriaRepository _siswaKriteriaRepository;
    private readonly IFileService _fileService;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
    private readonly IRazorTemplateEngine _templateEngine;
    private readonly IPDFGeneratorService _pDFGeneratorService;
    private readonly IKelasRepository _kelasRepository;

    public EkstrakulikulerController(
        ISiswaRepository siswaRepository,
        ITahunAjaranRepository tahunAjaranRepository,
        IUnitOfWork unitOfWork,
        IToastrNotificationService notificationService,
        ISiswaKriteriaRepository siswaKriteriaRepository,
        IFileService fileService,
        ITempDataDictionaryFactory tempDataDictionaryFactory,
        IRazorTemplateEngine templateEngine,
        IPDFGeneratorService pDFGeneratorService,
        IKelasRepository kelasRepository)
    {
        _siswaRepository = siswaRepository;
        _tahunAjaranRepository = tahunAjaranRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _siswaKriteriaRepository = siswaKriteriaRepository;
        _fileService = fileService;
        _tempDataDictionaryFactory = tempDataDictionaryFactory;
        _templateEngine = templateEngine;
        _pDFGeneratorService = pDFGeneratorService;
        _kelasRepository = kelasRepository;
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
            IdKelas = kelas?.Id,
            Kelas = kelas,
            Jurusan = jurusan,
            DaftarEntry = (await _siswaRepository.GetAll(jurusan, tahun, kelas?.Id)).ToIndexEntryList()
        });
    }

    public async Task<IActionResult> Simpan(IndexVM vm)
    {
        foreach (var entry in vm.DaftarEntry)
        {
            var siswa = await _siswaRepository.Get(entry.IdSiswa);
            if (siswa is null) continue;

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

            if (entry.DaftarEkskul.Count == 0)
                siswaKriteria.Nilai = 0;
            else
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

        return RedirectToActionPermanent(nameof(Index), new { vm.Jurusan, vm.Tahun, vm.IdKelas });
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

        var daftarSiswa = await _siswaRepository.GetAll(vm.Jurusan, vm.Tahun, vm.IdKelas);

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

            if (ekstrakulikuler1 is null && ekstrakulikuler2 is null && ekstrakulikuler3 is null)
                siswaKriteria.Nilai = 0;
            else
            {
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
            }

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

    public async Task<IActionResult> PDF(int tahun, Jurusan jurusan, int? idKelas = null)
    {
        var kelas = idKelas is null ? null : await _kelasRepository.Get(idKelas.Value);

        var indexVM = new IndexVM
        {
            Tahun = tahun,
            Jurusan = jurusan,
            Kelas = kelas,
            IdKelas = idKelas,
            DaftarEntry = (await _siswaRepository.GetAll(jurusan, tahun, idKelas)).ToIndexEntryList()
        };

        var html = await _templateEngine.RenderAsync("Areas/Dashboard/Views/Ekstrakulikuler/PDF.cshtml", indexVM);

        var pdf = await _pDFGeneratorService.GeneratePDF(
            html,
            marginTop: 75,
            marginBottom: 75,
            marginLeft: 75,
            marginRight: 75);

        return File(
            pdf,
            "application/pdf",
            fileDownloadName: $"Ekstrakulikuler-{tahun}-{jurusan}{(kelas is null ? "" : $"-{kelas.Nama}")}.pdf"
        );
    }

    public async Task<IActionResult> Excel(int tahun, Jurusan jurusan, int? idKelas = null)
    {
        var kelas = idKelas is null ? null : await _kelasRepository.Get(idKelas.Value);

        var daftarEntry = (await _siswaRepository.GetAll(jurusan, tahun, idKelas)).ToIndexEntryList();

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
                },
                new Column
                {
                    Min = 8,
                    Max = 8,
                    Width = 24,
                    CustomWidth = true,
                },
                new Column
                {
                    Min = 9,
                    Max = 9,
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
                    CellValue = new CellValue($"(C{(int)KriteriaEnum.Ekstrakulikuler}) {KriteriaEnum.Ekstrakulikuler.Humanize()}"),
                    StyleIndex = 2,
                },
                new Cell
                {
                    CellReference = $"G{headerRow.RowIndex}",
                    CellValue = new CellValue("Predikat"),
                    StyleIndex = 2,
                },
                new Cell
                {
                    CellReference = $"H{headerRow.RowIndex}",
                    StyleIndex = 2,
                },
                new Cell
                {
                    CellReference = $"I{headerRow.RowIndex}",
                    StyleIndex = 2,
                }
            ]
        );
        sheetData.Append(headerRow);

        worksheetPart.Worksheet.InsertAfter(
            new MergeCells(
                [
                    new MergeCell { Reference = "G1:I1"}
                ]
            ),
            sheetData
        );

        for (int i = 0; i < daftarEntry.Count; i++)
        {
            var rowIndex = headerRow.RowIndex + (uint)i + 1u;
            var entry = daftarEntry[i];

            var row = new Row { RowIndex = rowIndex };
            row.Append([
                new Cell
                {
                    CellReference = $"A{row.RowIndex}",
                    CellValue = new CellValue(entry.Siswa.Nama),
                    StyleIndex = 2,
                },
                new Cell
                {
                    CellReference = $"B{row.RowIndex}",
                    CellValue = new CellValue(entry.Siswa.NISN),
                    StyleIndex = 1,
                },
                new Cell
                {
                    CellReference = $"C{row.RowIndex}",
                    CellValue = new CellValue(entry.Siswa.Jurusan.Humanize()),
                    StyleIndex = 1,
                },
                new Cell
                {
                    CellReference = $"D{row.RowIndex}",
                    CellValue = new CellValue(entry.Siswa.Kelas.Nama),
                    StyleIndex = 1,
                },
                new Cell
                {
                    CellReference = $"E{row.RowIndex}",
                    CellValue = new CellValue(entry.Siswa.TahunAjaran.Id),
                    StyleIndex = 1,
                },
                new Cell
                {
                    CellReference = $"F{row.RowIndex}",
                    CellValue = new(entry.Ekstrakulikuler.ToString() ?? "-"),
                    StyleIndex = 2,
                },
                new Cell
                {
                    CellReference = $"G{row.RowIndex}",
                    CellValue = new(entry.Siswa.Ekstrakulikuler1?.Humanize() ?? "-"),
                    StyleIndex = 2,
                },
                new Cell
                {
                    CellReference = $"H{row.RowIndex}",
                    CellValue = new(entry.Siswa.Ekstrakulikuler2?.Humanize() ?? "-"),
                    StyleIndex = 2,
                },
                new Cell
                {
                    CellReference = $"I{row.RowIndex}",
                    CellValue = new(entry.Siswa.Ekstrakulikuler3?.Humanize() ?? "-"),
                    StyleIndex = 2,
                }
            ]);

            sheetData.Append(row);
        }

        spreadSheet.Save();

        return File(
            memoryStream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileDownloadName: $"Ekstrakulikuler-{tahun}-{jurusan}{(kelas is null ? "" : $"-{kelas.Nama}")}.xlsx"
        );
    }
}
