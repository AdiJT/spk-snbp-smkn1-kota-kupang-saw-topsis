using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Humanizer;
using Razor.Templating.Core;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Domain.Shared;
using SpkSnbp.Web.Areas.Dashboard.Models;
using SpkSnbp.Web.Areas.Dashboard.Models.Seleksi;
using SpkSnbp.Web.Services.PDFGenerator;
using SpkSnbp.Web.Services.Toastr;
using SpkSnbp.Web.Services.TopsisSAW;

namespace SpkSnbp.Web.Areas.Dashboard.Controllers;

[Authorize]
[Area(AreaNames.Dashboard)]
public class SeleksiController : Controller
{
    private readonly ISiswaRepository _siswaRepository;
    private readonly ITopsisSAWService _topsisSAWService;
    private readonly ITahunAjaranRepository _tahunAjaranRepository;
    private readonly IToastrNotificationService _toastrNotificationService;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
    private readonly IRazorTemplateEngine _templateEngine;
    private readonly IPDFGeneratorService _pDFGeneratorService;
    private readonly IKriteriaRepository _kriteriaRepository;

    public SeleksiController(
        ISiswaRepository siswaRepository,
        ITopsisSAWService topsisSAWService,
        ITahunAjaranRepository tahunAjaranRepository,
        IToastrNotificationService toastrNotificationService,
        ITempDataDictionaryFactory tempDataDictionaryFactory,
        IRazorTemplateEngine templateEngine,
        IPDFGeneratorService pDFGeneratorService,
        IKriteriaRepository kriteriaRepository)
    {
        _siswaRepository = siswaRepository;
        _topsisSAWService = topsisSAWService;
        _tahunAjaranRepository = tahunAjaranRepository;
        _toastrNotificationService = toastrNotificationService;
        _tempDataDictionaryFactory = tempDataDictionaryFactory;
        _templateEngine = templateEngine;
        _pDFGeneratorService = pDFGeneratorService;
        _kriteriaRepository = kriteriaRepository;
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
            return View(new IndexVM { Jurusan = jurusan, DaftarSiswa = [] });

        if (!first) tempDataDict[TempDataKeys.Tahun] = tahunAjaran.Id;

        return View(new IndexVM
        {
            Jurusan = jurusan,
            Tahun = tahunAjaran.Id,
            TahunAjaran = tahunAjaran,
            DaftarSiswa = [.. (await _siswaRepository.GetAll(jurusan, tahunAjaran.Id)).OrderByDescending(x => x.NilaiTopsis)]
        });
    }

    [HttpPost]
    public async Task<IActionResult> Index(Jurusan jurusan, int tahun, string? returnUrl = null)
    {
        returnUrl ??= Url.ActionLink(nameof(Index))!;

        var tahunAjaran = await _tahunAjaranRepository.Get(tahun);

        if (tahunAjaran is null)
        {
            _toastrNotificationService.AddError("Tahun tidak ditemukan", "Seleksi");
            return RedirectPermanent(returnUrl);
        }

        var result = await _topsisSAWService.SeleksiEligible(tahun, jurusan);
        if (result.IsSuccess)
            _toastrNotificationService.AddSuccess("Seleksi Berhasil");
        else
            _toastrNotificationService.AddError(result.Error.Message, "Seleksi");

        return RedirectPermanent(returnUrl);
    }

    public async Task<IActionResult> PDF(int tahun, Jurusan jurusan)
    {
        var indexVM = new IndexVM
        {
            Jurusan = jurusan,
            Tahun = tahun,
            DaftarSiswa = [.. (await _siswaRepository.GetAll(jurusan, tahun)).OrderByDescending(x => x.NilaiTopsis)]
        };

        var html = await _templateEngine.RenderAsync("Areas/Dashboard/Views/Seleksi/PDF.cshtml", indexVM);

        var pdf = await _pDFGeneratorService.GeneratePDF(
            html,
            marginTop: 75,
            marginBottom: 75,
            marginLeft: 75,
            marginRight: 75);

        return File(
            pdf,
            "application/pdf",
            fileDownloadName: $"Hasil Seleksi-{tahun}-{jurusan}.pdf"
        );
    }

    public async Task<IActionResult> Excel(int tahun, Jurusan jurusan)
    {
        var daftarSiswa = await _siswaRepository.GetAll();
        daftarSiswa = [.. daftarSiswa.OrderByDescending(x => x.NilaiTopsis)];
        var daftarKriteria = await _kriteriaRepository.GetAll();

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
                    Width = 15,
                    CustomWidth = true,
                },
                new Column
                {
                    Min = 2,
                    Max = 2,
                    Width = 15,
                    CustomWidth = true,
                },
                new Column
                {
                    Min = 3,
                    Max = 3,
                    Width = 24,
                    CustomWidth = true,
                },
                new Column
                {
                    Min = 4,
                    Max = 4,
                    Width = 40,
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
                },
                new Column
                {
                    Min = 10,
                    Max = 10,
                    Width = 24,
                    CustomWidth = true,
                },
                new Column
                {
                    Min = 11,
                    Max = 11,
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
                    CellValue = new CellValue("Rangking"),
                    StyleIndex = 2,
                },
                new Cell
                {
                    CellReference = $"B{headerRow.RowIndex}",
                    CellValue = new CellValue("Nilai Preferensi"),
                    StyleIndex = 2,
                },
                new Cell
                {
                    CellReference = $"C{headerRow.RowIndex}",
                    CellValue = new CellValue("NISN"),
                    StyleIndex = 1,
                },
                new Cell
                {
                    CellReference = $"D{headerRow.RowIndex}",
                    CellValue = new CellValue("Nama"),
                    StyleIndex = 2,
                },
                new Cell
                {
                    CellReference = $"E{headerRow.RowIndex}",
                    CellValue = new CellValue("Eligible"),
                    StyleIndex = 2,
                },
                new Cell
                {
                    CellReference = $"F{headerRow.RowIndex}",
                    CellValue = new CellValue("Kriteria"),
                    StyleIndex = 2,
                },
                ..Enumerable.Range(1, daftarKriteria.Count - 1).Select(x => new Cell {
                    CellReference = $"{(char)('F' + x)}{headerRow.RowIndex}",
                    StyleIndex = 2,
                })
            ]
        );
        sheetData.Append(headerRow);

        var headerRow2 = new Row() { RowIndex = 2 };
        headerRow2.Append(
            [
                new Cell
                {
                    CellReference = $"A{headerRow2.RowIndex}",
                    StyleIndex = 2,
                },
                new Cell
                {
                    CellReference = $"B{headerRow2.RowIndex}",
                    StyleIndex = 2,
                },
                new Cell
                {
                    CellReference = $"C{headerRow2.RowIndex}",
                    StyleIndex = 1,
                },
                new Cell
                {
                    CellReference = $"D{headerRow2.RowIndex}",
                    StyleIndex = 2,
                },
                new Cell
                {
                    CellReference = $"E{headerRow2.RowIndex}",
                    StyleIndex = 2,
                },
                ..daftarKriteria.OrderBy(x => x.Id).Select(x => new Cell {
                    CellReference = $"{(char)('E' + x.Id)}{headerRow2.RowIndex}",
                    CellValue = new CellValue($"(C{x.Id}) {x.Nama}"),
                    StyleIndex = 2,
                })
            ]
        );
        sheetData.Append(headerRow2);

        worksheetPart.Worksheet.InsertAfter(
            new MergeCells(
                [
                    ..Enumerable.Range(0, 5)
                        .Select(x => new MergeCell { Reference = $"{(char)('A' + x)}1:{(char)('A' + x)}2"}),
                    new MergeCell { Reference = $"F1:{(char)('F' + daftarKriteria.Count - 1)}1" }
                ]
            ),
            sheetData
        );

        for (int i = 0; i < daftarSiswa.Count; i++)
        {
            var rowIndex = headerRow2.RowIndex + (uint)i + 1u;
            var siswa = daftarSiswa[i];

            var row = new Row { RowIndex = rowIndex };
            row.Append(
            [
                new Cell
                {
                    CellReference = $"A{row.RowIndex}",
                    CellValue = new CellValue(siswa.NilaiTopsis is null ? "-" : $"{i + 1}"),
                    StyleIndex = 2,
                },
                new Cell
                {
                    CellReference = $"B{row.RowIndex}",
                    CellValue = new CellValue(siswa.NilaiTopsis?.ToString() ?? "-" ),
                    StyleIndex = 2,
                },
                new Cell
                {
                    CellReference = $"C{row.RowIndex}",
                    CellValue = new CellValue(siswa.NISN),
                    StyleIndex = 1,
                },
                new Cell
                {
                    CellReference = $"D{row.RowIndex}",
                    CellValue = new CellValue(siswa.Nama),
                    StyleIndex = 2,
                },
                new Cell
                {
                    CellReference = $"E{row.RowIndex}",
                    CellValue = new CellValue(siswa.Eligible?.Humanize() ?? "-"),
                    StyleIndex = 2,
                },
                ..daftarKriteria.OrderBy(x => x.Id).Select(x => new Cell {
                    CellReference = $"{(char)('E' + x.Id)}{row.RowIndex}",
                    CellValue = new($"{siswa.DaftarSiswaKriteria.FirstOrDefault(y => y.Kriteria == x)?.Nilai.ToString() ?? "-"}"),
                    StyleIndex = 2,
                })
            ]
        );
            sheetData.Append(row);
        }

        spreadSheet.Save();

        return File(
            memoryStream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileDownloadName: $"Hasil Seleksi-{tahun}-{jurusan}.xlsx");
    }
}
