using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Razor.Templating.Core;
using SpkSnbp.Domain.Auth;
using SpkSnbp.Domain.Contracts;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Web.Areas.Dashboard.Models.KriteriaModels;
using SpkSnbp.Web.Services.PDFGenerator;
using SpkSnbp.Web.Services.Toastr;

namespace SpkSnbp.Web.Areas.Dashboard.Controllers;

[Authorize(Roles = UserRoles.Admin)]
[Area(AreaNames.Dashboard)]
public class KriteriaController : Controller
{
    private readonly IKriteriaRepository _kriteriaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IToastrNotificationService _notificationService;
    private readonly IRazorTemplateEngine _templateEngine;
    private readonly IPDFGeneratorService _pDFGeneratorService;

    public KriteriaController(
        IKriteriaRepository kriteriaRepository,
        IUnitOfWork unitOfWork,
        IToastrNotificationService notificationService,
        IRazorTemplateEngine templateEngine,
        IPDFGeneratorService pDFGeneratorService)
    {
        _kriteriaRepository = kriteriaRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _templateEngine = templateEngine;
        _pDFGeneratorService = pDFGeneratorService;
    }

    public async Task<IActionResult> Index() => View(await _kriteriaRepository.GetAll());

    [HttpPost]
    public async Task<IActionResult> Edit(EditVM vm)
    {
        var returnUrl = vm.ReturnUrl ?? Url.ActionLink(nameof(Index))!;

        if (!ModelState.IsValid)
        {
            _notificationService.AddError("Data tidak valid", "Edit");
            return RedirectPermanent(returnUrl);
        }

        var kriteria = await _kriteriaRepository.Get(vm.Id);
        if (kriteria is null)
        {
            _notificationService.AddError("Kriteria tidak ditemukan", "Edit");
            return RedirectPermanent(returnUrl);
        }

        kriteria.Bobot = vm.Bobot;
        kriteria.Jenis = vm.Jenis;

        var result = await _unitOfWork.SaveChangesAsync();
        if (result.IsSuccess)
            _notificationService.AddSuccess("Simpan Berhasil", "Edit");
        else
            _notificationService.AddError("Simpan Gagal", "Edit");

        return RedirectPermanent(returnUrl);
    }

    public async Task<IActionResult> PDF()
    {
        var daftarKriteria = await _kriteriaRepository.GetAll();

        var html = await _templateEngine.RenderAsync("Areas/Dashboard/Views/Kriteria/PDF.cshtml", daftarKriteria);

        var pdf = await _pDFGeneratorService.GeneratePDF(
            html,
            marginTop: 75,
            marginBottom: 75,
            marginLeft: 75,
            marginRight: 75);

        return File(pdf, "application/pdf", fileDownloadName: "Kriteria.pdf");
    }

    public async Task<IActionResult> Excel()
    {
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
                new CellFormat(new Alignment { Horizontal = HorizontalAlignmentValues.Center })
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
            { Count = 2 }
        };

        var worksheetPart = workBookPart.AddNewPart<WorksheetPart>();
        var sheetData = new SheetData();
        worksheetPart.Worksheet = new Worksheet(
            new Columns(
                new Column
                {
                    Min = 1,
                    Max = 1,
                    Width = 6,
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
            new Cell
            {
                CellReference = $"A{headerRow.RowIndex}",
                CellValue = new CellValue("Kode"),
                StyleIndex = 1,
            },
            new Cell
            {
                CellReference = $"B{headerRow.RowIndex}",
                CellValue = new CellValue("Nama"),
                StyleIndex = 1,
            },
            new Cell
            {
                CellReference = $"C{headerRow.RowIndex}",
                CellValue = new CellValue("Bobot"),
                StyleIndex = 1,
            },
            new Cell
            {
                CellReference = $"D{headerRow.RowIndex}",
                CellValue = new CellValue("Jenis"),
                StyleIndex = 1,
            }
        );
        sheetData.Append(headerRow);

        for (int i = 0; i < daftarKriteria.Count; i++)
        {
            var rowIndex = headerRow.RowIndex + (uint)i + 1u;

            var row = new Row { RowIndex = rowIndex };
            row.Append(
                new Cell
                {
                    CellReference = $"A{row.RowIndex}",
                    CellValue = new CellValue($"C{daftarKriteria[i].Id}"),
                    StyleIndex = 1,
                },
                new Cell
                {
                    CellReference = $"B{row.RowIndex}",
                    CellValue = new CellValue($"{daftarKriteria[i].Nama}"),
                    StyleIndex = 1,
                },
                new Cell
                {
                    CellReference = $"C{row.RowIndex}",
                    CellValue = new CellValue($"{daftarKriteria[i].Bobot}"),
                    StyleIndex = 1,
                },
                new Cell
                {
                    CellReference = $"D{row.RowIndex}",
                    CellValue = new CellValue($"{daftarKriteria[i].Jenis.Humanize()}"),
                    StyleIndex = 1,
                }
            );
            sheetData.Append(row);
        }

        spreadSheet.Save();

        return File(
            memoryStream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileDownloadName: "Kriteria.xlsx");
    }
}
