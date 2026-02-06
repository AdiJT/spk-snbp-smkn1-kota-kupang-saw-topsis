using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Razor.Templating.Core;
using SpkSnbp.Domain.Auth;
using SpkSnbp.Domain.Contracts;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Web.Areas.Dashboard.Models.KelasModels;
using SpkSnbp.Web.Services.PDFGenerator;
using SpkSnbp.Web.Services.Toastr;
using System.Threading.Tasks;

namespace SpkSnbp.Web.Areas.Dashboard.Controllers;

[Area(AreaNames.Dashboard)]
[Authorize(Roles = UserRoles.Admin)]
public class KelasController : Controller
{
    private readonly IKelasRepository _kelasRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IToastrNotificationService _notificationService;
    private readonly IRazorTemplateEngine _templateEngine;
    private readonly IPDFGeneratorService _pDFGeneratorService;

    public KelasController(
        IKelasRepository kelasRepository,
        IUnitOfWork unitOfWork,
        IToastrNotificationService notificationService,
        IRazorTemplateEngine templateEngine,
        IPDFGeneratorService pDFGeneratorService)
    {
        _kelasRepository = kelasRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _templateEngine = templateEngine;
        _pDFGeneratorService = pDFGeneratorService;
    }

    public async Task<IActionResult> Index() => View(await _kelasRepository.GetAll());

    [HttpPost]
    public async Task<IActionResult> Tambah(TambahVM vm)
    {
        if (!ModelState.IsValid)
        {
            _notificationService.AddError("Data tidak valid", "Tambah Kelas");
            return RedirectToAction(nameof(Index));
        }

        if (await _kelasRepository.IsExist(vm.Nama))
        {
            _notificationService.AddError($"Kelas dengan nama {vm.Nama} sudah ada!", "Tambah Kelas");
            return RedirectToAction(nameof(Index));
        }

        var kelas = new Kelas { Nama = vm.Nama };

        _kelasRepository.Add(kelas);
        var result = await _unitOfWork.SaveChangesAsync();
        if (result.IsSuccess) _notificationService.AddSuccess("Simpan Berhasil", "Tambah Kelas");
        else _notificationService.AddError("Simpan Gagal", "Tambah Kelas");

        return RedirectToActionPermanent(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditVM vm)
    {
        if (!ModelState.IsValid)
        {
            _notificationService.AddError("Data tidak valid", "Edit Kelas");
            return RedirectToAction(nameof(Index));
        }

        if (await _kelasRepository.IsExist(vm.Nama, vm.Id))
        {
            _notificationService.AddError($"Kelas dengan nama {vm.Nama} sudah ada!", "Edit Kelas");
            return RedirectToAction(nameof(Index));
        }

        var kelas = await _kelasRepository.Get(vm.Id);
        if (kelas is null)
        {
            _notificationService.AddError("Kelas tidak ditemukan!", "Edit Kelas");
            return RedirectToAction(nameof(Index));
        }

        kelas.Nama = vm.Nama;

        var result = await _unitOfWork.SaveChangesAsync();
        if (result.IsSuccess) _notificationService.AddSuccess("Simpan Berhasil", "Edit Kelas");
        else _notificationService.AddError("Simpan Gagal", "Edit Kelas");

        return RedirectToActionPermanent(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Hapus(int id)
    {
        var kelas = await _kelasRepository.Get(id);
        if (kelas is null)
        {
            _notificationService.AddError("Kelas tidak ditemukan", "Hapus Kelas");
            return RedirectToAction(nameof(Index));
        }

        _kelasRepository.Delete(kelas);
        var result = await _unitOfWork.SaveChangesAsync();
        if (result.IsSuccess) _notificationService.AddSuccess("Simpan Berhasil", "Hapus Kelas");
        else _notificationService.AddError("Simpan Gagal", "Hapus Kelas");

        return RedirectToActionPermanent(nameof(Index));
    }


    public async Task<IActionResult> PDF()
    {
        var daftarKelas = await _kelasRepository.GetAll();

        var html = await _templateEngine.RenderAsync("Areas/Dashboard/Views/Kelas/PDF.cshtml", daftarKelas);

        var pdf = await _pDFGeneratorService.GeneratePDF(
            html,
            marginTop: 75,
            marginBottom: 75,
            marginLeft: 75,
            marginRight: 75);

        return File(pdf, "application/pdf", fileDownloadName: "Kelas.pdf");
    }

    public async Task<IActionResult> Excel()
    {
        var daftarKelas = await _kelasRepository.GetAll();

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
            [
                new Cell
                {
                    CellReference = $"A{headerRow.RowIndex}",
                    CellValue = new CellValue("Kelas"),
                    StyleIndex = 1,
                },
                ..Enum.GetValues<Jurusan>().Select((x, i) => new Cell 
                {
                    CellReference = $"{(char)('B' + i)}{headerRow.RowIndex}",
                    CellValue = new CellValue(x.Humanize()),
                    StyleIndex = 1
                })
            ]
        );
        sheetData.Append(headerRow);

        for (int i = 0; i < daftarKelas.Count; i++)
        {
            var rowIndex = headerRow.RowIndex + (uint)i + 1u;

            var row = new Row { RowIndex = rowIndex };
            row.Append(
                [
                    new Cell
                    {
                        CellReference = $"A{row.RowIndex}",
                        CellValue = new CellValue(daftarKelas[i].Nama),
                        StyleIndex = 1,
                    },
                    ..Enum.GetValues<Jurusan>().Select((x, j) => new Cell
                    {
                        CellReference = $"{(char)('B' + j)}{row.RowIndex}",
                        CellValue = new CellValue(daftarKelas[i].DaftarSiswa.Count(y => y.Jurusan == x)),
                        StyleIndex = 1
                    })
                ]
            );
            sheetData.Append(row);
        }

        spreadSheet.Save();

        return File(
            memoryStream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileDownloadName: "Kelas.xlsx");
    }
}
