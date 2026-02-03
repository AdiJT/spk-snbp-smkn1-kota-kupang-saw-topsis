using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Razor.Templating.Core;
using SpkSnbp.Domain.Auth;
using SpkSnbp.Domain.Contracts;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Web.Areas.Dashboard.Models.TahunAjaranModels;
using SpkSnbp.Web.Services.PDFGenerator;
using SpkSnbp.Web.Services.Toastr;
using System.Threading.Tasks;

namespace SpkSnbp.Web.Areas.Dashboard.Controllers;

[Authorize(Roles = UserRoles.Admin)]
[Area(AreaNames.Dashboard)]
public class TahunAjaranController : Controller
{
    private readonly ITahunAjaranRepository _tahunAjaranRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IToastrNotificationService _notificationService;
    private readonly IRazorTemplateEngine _templateEngine;
    private readonly IPDFGeneratorService _pDFGeneratorService;

    public TahunAjaranController(
        ITahunAjaranRepository tahunAjaranRepository,
        IUnitOfWork unitOfWork,
        IToastrNotificationService notificationService,
        IRazorTemplateEngine templateEngine,
        IPDFGeneratorService pDFGeneratorService)
    {
        _tahunAjaranRepository = tahunAjaranRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _templateEngine = templateEngine;
        _pDFGeneratorService = pDFGeneratorService;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _tahunAjaranRepository.GetAll());
    }

    [HttpPost]
    public async Task<IActionResult> Tambah(TambahVM vm)
    {
        var returnUrl = vm.ReturnUrl ?? Url.ActionLink(nameof(Index))!;

        if (!ModelState.IsValid)
        {
            _notificationService.AddError("Data tidak valid", "Tambah");
            return Redirect(returnUrl);
        }

        if (await _tahunAjaranRepository.IsExist(vm.Tahun))
        {
            _notificationService.AddError($"Tahun {vm.Tahun} sudah ada", "Tambah");
            return Redirect(returnUrl);
        }

        var tahunAjaran = new TahunAjaran { Id = vm.Tahun };

        _tahunAjaranRepository.Add(tahunAjaran);
        var result = await _unitOfWork.SaveChangesAsync();
        if (result.IsSuccess)
            _notificationService.AddSuccess("Simpan Berhasil", "Tambah");
        else
            _notificationService.AddError("Simpan Gagal", "Tambah");

        return Redirect(returnUrl);
    }

    [HttpPost]
    public async Task<IActionResult> Hapus(int tahun, string? returnUrl = null)
    {
        returnUrl ??= Url.Action(nameof(Index))!;

        var tahunAjaran = await _tahunAjaranRepository.Get(tahun);
        if (tahunAjaran is null)
        {
            _notificationService.AddError("Tahun Ajaran tidak ditemukan", "Hapus");
            return Redirect(returnUrl);
        }

        _tahunAjaranRepository.Delete(tahunAjaran);
        var result = await _unitOfWork.SaveChangesAsync();
        if (result.IsSuccess)
            _notificationService.AddSuccess("Simpan Berhasil", "Hapus");
        else
            _notificationService.AddError("Simpan Gagal", "Hapus");

        return Redirect(returnUrl);
    }

    public async Task<IActionResult> PDF()
    {
        var daftarTahunAjaran = await _tahunAjaranRepository.GetAll();

        var html = await _templateEngine.RenderAsync("Areas/Dashboard/Views/TahunAjaran/PDF.cshtml", daftarTahunAjaran);

        var pdf = await _pDFGeneratorService.GeneratePDF(
            html,
            marginTop: 75,
            marginBottom: 75,
            marginLeft: 75,
            marginRight: 75);

        return File(pdf, "application/pdf", fileDownloadName:"Tahun Ajaran.pdf");
    }

    public async Task<IActionResult> Excel()
    {
        var daftarTahunAjaran = await _tahunAjaranRepository.GetAll();

        using var memoryStream = new MemoryStream();
        using var spreadSheet = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook);

        var workBookPart = spreadSheet.AddWorkbookPart();
        workBookPart.Workbook = new Workbook();

        var stylePart = workBookPart.AddNewPart<WorkbookStylesPart>();
        stylePart.Stylesheet = new()
        {
            Fonts = new
            (
                new Font { FontSize = new() { Val = 11 }, FontName = new() { Val = "Calibri" } }
            ) { Count = 1 },

            Fills = new
            (
                new Fill { PatternFill = new() { PatternType = PatternValues.None } },
                new Fill { PatternFill = new() { PatternType = PatternValues.Gray125 } }
            ) { Count = 2 },

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
            ) { Count = 2 },

            CellStyleFormats = new
            (
                new CellFormat()
            ) { Count = 1 },

            CellFormats = new
            (
                new CellFormat(),
                new CellFormat(new Alignment { Horizontal = HorizontalAlignmentValues.Center} ) 
                { 
                    FormatId = 0, 
                    FontId = 0, 
                    BorderId = 1, 
                    FillId = 0, 
                    ApplyFill = true, 
                    ApplyBorder = true, 
                    ApplyFont = true 
                }
            ) { Count = 2 }
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
                CellValue = new CellValue("Tahun Ajaran"),
                StyleIndex = 1,
            },
            new Cell
            {
                CellReference = $"B{headerRow.RowIndex}",
                CellValue = new CellValue("Jumlah Siswa"),
                StyleIndex = 1,
            }
        );
        sheetData.Append(headerRow);

        for(int i = 0; i < daftarTahunAjaran.Count; i++)
        {
            var rowIndex = headerRow.RowIndex + (uint)i + 1u;

            var row = new Row { RowIndex = rowIndex };
            row.Append(
                new Cell
                {
                    CellReference = $"A{row.RowIndex}",
                    CellValue = new CellValue($"{daftarTahunAjaran[i].Id}"),
                    StyleIndex = 1,
                },
                new Cell
                {
                    CellReference = $"B{row.RowIndex}",
                    CellValue = new CellValue($"{daftarTahunAjaran[i].DaftarSiswa.Count}"),
                    StyleIndex = 1,
                }
            );
            sheetData.Append(row);
        }

        spreadSheet.Save();

        return File(
            memoryStream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileDownloadName: "Tahun Ajaran.xlsx");
    }
}
