using DocumentFormat.OpenXml.Bibliography;
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
using SpkSnbp.Infrastructure.Services.FileServices;
using SpkSnbp.Web.Areas.Dashboard.Models;
using SpkSnbp.Web.Areas.Dashboard.Models.SiswaModels;
using SpkSnbp.Web.Helpers;
using SpkSnbp.Web.Services.PDFGenerator;
using SpkSnbp.Web.Services.Toastr;
using System.Text.RegularExpressions;

namespace SpkSnbp.Web.Areas.Dashboard.Controllers;

[Authorize(Roles = UserRoles.Admin)]
[Area(AreaNames.Dashboard)]
public class SiswaController : Controller
{
    private readonly ISiswaRepository _siswaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IToastrNotificationService _notificationService;
    private readonly ITahunAjaranRepository _tahunAjaranRepository;
    private readonly IFileService _fileService;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
    private readonly IRazorTemplateEngine _templateEngine;
    private readonly IPDFGeneratorService _pDFGeneratorService;
    private readonly IKriteriaRepository _kriteriaRepository;

    public SiswaController(
        ISiswaRepository siswaRepository,
        IUnitOfWork unitOfWork,
        IToastrNotificationService notificationService,
        ITahunAjaranRepository tahunAjaranRepository,
        IFileService fileService,
        ITempDataDictionaryFactory tempDataDictionaryFactory,
        IRazorTemplateEngine templateEngine,
        IPDFGeneratorService pDFGeneratorService,
        IKriteriaRepository kriteriaRepository)
    {
        _siswaRepository = siswaRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _tahunAjaranRepository = tahunAjaranRepository;
        _fileService = fileService;
        _tempDataDictionaryFactory = tempDataDictionaryFactory;
        _templateEngine = templateEngine;
        _pDFGeneratorService = pDFGeneratorService;
        _kriteriaRepository = kriteriaRepository;
    }

    public async Task<IActionResult> Index(Jurusan? jurusan = null, int? tahun = null, bool first = true)
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
        else if (jurusan is not null)
            tempDataDict[TempDataKeys.Jurusan] = jurusan;

        var tahunAjaran = tahun is null ? null : await _tahunAjaranRepository.Get(tahun.Value);

        if (tahunAjaran is null)
            return View(new IndexVM
            {
                Jurusan = jurusan,
                DaftarSiswa = await _siswaRepository.GetAll(jurusan)
            });

        if (!first) tempDataDict[TempDataKeys.Tahun] = tahunAjaran.Id;

        return View(new IndexVM
        {
            Jurusan = jurusan,
            Tahun = tahunAjaran.Id,
            TahunAjaran = tahunAjaran,
            DaftarSiswa = await _siswaRepository.GetAll(jurusan, tahun)
        });
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

        if (await _siswaRepository.IsExist(vm.NISN))
        {
            _notificationService.AddError($"NISN '{vm.NISN}' sudah digunakan", "Tambah");
            return Redirect(returnUrl);
        }

        var tahunAjaran = await _tahunAjaranRepository.Get(vm.IdTahunAjaran);
        if (tahunAjaran is null)
        {
            _notificationService.AddError("Tahun ajaran tidak ditemukan", "Tambah");
            return Redirect(returnUrl);
        }

        var siswa = new Siswa
        {
            Nama = vm.Nama,
            NISN = vm.NISN,
            Jurusan = vm.Jurusan,
            TahunAjaran = tahunAjaran
        };

        _siswaRepository.Add(siswa);
        var result = await _unitOfWork.SaveChangesAsync();
        if (result.IsSuccess)
            _notificationService.AddSuccess("Simpan Berhasil", "Tambah");
        else
            _notificationService.AddError("Simpan Gagal", "Tambah");

        return Redirect(returnUrl);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditVM vm)
    {
        var returnUrl = vm.ReturnUrl ?? Url.ActionLink(nameof(Index))!;

        if (!ModelState.IsValid)
        {
            _notificationService.AddError("Data tidak valid", "Edit");
            return Redirect(returnUrl);
        }

        var siswa = await _siswaRepository.Get(vm.Id);
        if (siswa is null)
        {
            _notificationService.AddError("Siswa tidak ditemukan", "Edit");
            return Redirect(returnUrl);
        }

        if (await _siswaRepository.IsExist(vm.NISN, vm.Id))
        {
            _notificationService.AddError($"NISN '{vm.NISN}' sudah digunakan", "Edit");
            return Redirect(returnUrl);
        }

        var tahunAjaran = await _tahunAjaranRepository.Get(vm.IdTahunAjaran);
        if (tahunAjaran is null)
        {
            _notificationService.AddError("Tahun ajaran tidak ditemukan", "Edit");
            return Redirect(returnUrl);
        }

        siswa.TahunAjaran = tahunAjaran;
        siswa.NISN = vm.NISN;
        siswa.Nama = vm.Nama;
        siswa.Jurusan = vm.Jurusan;

        var result = await _unitOfWork.SaveChangesAsync();
        if (result.IsSuccess)
            _notificationService.AddSuccess("Simpan Berhasil", "Edit");
        else
            _notificationService.AddError("Simpan Gagal", "Edit");

        return Redirect(returnUrl);
    }

    [HttpPost]
    public async Task<IActionResult> Hapus(int id, string? returnUrl = null)
    {
        returnUrl ??= Url.ActionLink(nameof(Index))!;

        var siswa = await _siswaRepository.Get(id);
        if (siswa is null)
        {
            _notificationService.AddError("Siswa tidak ditemukan", "Hapus");
            return RedirectPermanent(returnUrl);
        }

        _siswaRepository.Delete(siswa);
        var result = await _unitOfWork.SaveChangesAsync();
        if (result.IsSuccess)
            _notificationService.AddSuccess("Simpan Berhasil", "Hapus");
        else
            _notificationService.AddError("Simpan Gagal", "Hapus");

        return RedirectPermanent(returnUrl);
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

        var nisnCellRef = sheetData
            .Descendants<Cell>()
            .FirstOrDefault(x => HelperFunctions.GetCellValues(x, sharedStrings).Trim().ToLower() == "nisn")?
            .CellReference?.Value;

        if (nisnCellRef is null)
        {
            _notificationService.AddError("Tidak ada kolom NISN", "Import");
            return RedirectPermanent(returnUrl);
        }

        var match = Regex.Match(nisnCellRef, @"(?<kolom>[A-Z]*)[1-2]*");
        if (!match.Success || !match.Groups.TryGetValue("kolom", out var kolomGroup))
        {
            _notificationService.AddError("Tidak ada kolom NISN", "Import");
            return RedirectPermanent(returnUrl);
        }

        nisnCellRef = kolomGroup.Value;

        var namaCellRef = sheetData
            .Descendants<Cell>()
            .FirstOrDefault(x => HelperFunctions.GetCellValues(x, sharedStrings).ToLower().Contains("nama"))?
            .CellReference?.Value;

        if (namaCellRef is null)
        {
            _notificationService.AddError("Tidak ada kolom Nama", "Import");
            return RedirectPermanent(returnUrl);
        }

        match = Regex.Match(namaCellRef, @"(?<kolom>[A-Z]*)[1-2]*");
        if (!match.Success || !match.Groups.TryGetValue("kolom", out kolomGroup))
        {
            _notificationService.AddError("Tidak ada kolom Nama", "Import");
            return RedirectPermanent(returnUrl);
        }

        namaCellRef = kolomGroup.Value;

        foreach (var row in sheetData.Elements<Row>().Skip(7))
        {
            var nisnCell = row.Elements<Cell>().FirstOrDefault(x => x.CellReference!.Value!.StartsWith(nisnCellRef));
            if (nisnCell is null) continue;

            var nisn = HelperFunctions.GetCellValues(nisnCell, sharedStrings);
            if (string.IsNullOrWhiteSpace(nisn)) continue;

            var namaCell = row.Elements<Cell>().FirstOrDefault(x => x.CellReference!.Value!.StartsWith(namaCellRef));
            if (namaCell is null) continue;

            var nama = HelperFunctions.GetCellValues(namaCell, sharedStrings);
            if (string.IsNullOrWhiteSpace(nama) || await _siswaRepository.IsExist(nisn)) continue;

            var siswa = new Siswa
            {
                NISN = nisn,
                Nama = nama,
                Jurusan = vm.Jurusan,
                TahunAjaran = tahunAjaran
            };

            _siswaRepository.Add(siswa);
            daftarSiswa.Add(siswa);
        }

        var result = await _unitOfWork.SaveChangesAsync();
        if (result.IsSuccess)
            _notificationService.AddSuccess("Import Berhasil", "Import");
        else
            _notificationService.AddError("Import Gagal", "Import");

        return RedirectPermanent(returnUrl);
    }

    public async Task<IActionResult> PDF(int? tahun, Jurusan? jurusan)
    {
        var daftarSiswa = await _siswaRepository.GetAll(jurusan, tahun);

        var html = await _templateEngine.RenderAsync("Areas/Dashboard/Views/Siswa/PDF.cshtml", daftarSiswa);

        var pdf = await _pDFGeneratorService.GeneratePDF(
            html,
            marginTop: 75,
            marginBottom: 75,
            marginLeft: 75,
            marginRight: 75);

        return File(
            pdf,
            "application/pdf",
            fileDownloadName: $"Siswa{(tahun is null ? "" : $"-{tahun}")}" +
            $"{(jurusan is null ? "" : $"-{jurusan.Value.Humanize()}")}.pdf"
        );
    }

    public async Task<IActionResult> Excel(int? tahun, Jurusan? jurusan)
    {
        var daftarSiswa = await _siswaRepository.GetAll(jurusan, tahun);
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
                    Width = 24,
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
                    CellValue = new CellValue("Tahun Ajaran"),
                    StyleIndex = 1,
                },
                new Cell
                {
                    CellReference = $"E{headerRow.RowIndex}",
                    CellValue = new CellValue("Kriteria"),
                    StyleIndex = 2,
                },
                ..Enumerable.Range(1, daftarKriteria.Count - 1).Select(x => new Cell {
                    CellReference = $"{(char)('E' + x)}{headerRow.RowIndex}",
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
                    StyleIndex = 1,
                },
                new Cell
                {
                    CellReference = $"C{headerRow2.RowIndex}",
                    StyleIndex = 1,
                },
                new Cell
                {
                    CellReference = $"D{headerRow2.RowIndex}",
                    StyleIndex = 1,
                },
                ..daftarKriteria.OrderBy(x => x.Id).Select(x => new Cell {
                    CellReference = $"{(char)('D' + x.Id)}{headerRow2.RowIndex}",
                    CellValue = new CellValue($"(C{x.Id}) {x.Nama}"),
                    StyleIndex = 2,
                })
            ]
        );
        sheetData.Append(headerRow2);

        worksheetPart.Worksheet.InsertAfter(
            new MergeCells(
                [
                    ..Enumerable.Range(0, 4)
                        .Select(x => new MergeCell { Reference = $"{(char)('A' + x)}1:{(char)('A' + x)}2"}),
                    new MergeCell { Reference = $"E1:{(char)('E' + daftarKriteria.Count - 1)}1" }
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
                    CellValue = new CellValue(siswa.TahunAjaran.Id),
                    StyleIndex = 1,
                },
                ..daftarKriteria.OrderBy(x => x.Id).Select(x => new Cell {
                    CellReference = $"{(char)('D' + x.Id)}{row.RowIndex}",
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
            fileDownloadName: $"Siswa{(tahun is null ? "" : $"-{tahun}")}" +
            $"{(jurusan is null ? "" : $"-{jurusan.Value.Humanize()}")}.xlsx");
    }
}
