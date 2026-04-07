using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Razor.Templating.Core;
using SpkSnbp.Domain.Auth;
using SpkSnbp.Domain.Contracts;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Domain.Shared;
using SpkSnbp.Infrastructure.Services.FileServices;
using SpkSnbp.Web.Areas.Dashboard.Models;
using SpkSnbp.Web.Areas.Dashboard.Models.NilaiKriteria;
using SpkSnbp.Web.Helpers;
using SpkSnbp.Web.Services.PDFGenerator;
using SpkSnbp.Web.Services.Toastr;

namespace SpkSnbp.Web.Areas.Dashboard.Controllers;

[Authorize(Roles = UserRoles.Admin)]
[Area(AreaNames.Dashboard)]
public class NilaiKriteriaController : Controller
{
    private const string FormatId = "e3713bc2-f299-4ee4-9c63-7ce4d6f82969";

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
    private readonly IInformasiSekolahRepository _informasiSekolahRepository;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public NilaiKriteriaController(
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
        IInformasiSekolahRepository informasiSekolahRepository,
        IWebHostEnvironment webHostEnvironment)
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
        _informasiSekolahRepository = informasiSekolahRepository;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index(
        int? id,
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
            var kriteriaTempData = tempDataDict.Peek(TempDataKeys.NilaiKriteria);

            if (jurusanTempData is not null)
                jurusan = (Jurusan)jurusanTempData;

            if (tahunTempData is not null)
                tahun = (int)tahunTempData;

            if (kelasTempData is not null)
                idKelas = (int)kelasTempData;

            if (kriteriaTempData is not null)
                id = (int)kriteriaTempData;
        }
        else
            tempDataDict[TempDataKeys.Jurusan] = jurusan;

        var tahunAjaran = tahun is null ?
                    await _tahunAjaranRepository.Get(CultureInfos.DateOnlyNow.Year) :
                    await _tahunAjaranRepository.Get(tahun.Value);

        tahunAjaran ??= await _tahunAjaranRepository.GetLatest();

        var kelas = idKelas is null ? null : await _kelasRepository.Get(idKelas.Value);

        var kriteria = id is null ? null : await _kriteriaRepository.Get(id.Value);

        if (!first && tahunAjaran is not null) tempDataDict[TempDataKeys.Tahun] = tahunAjaran.Id;
        if (!first) tempDataDict[TempDataKeys.Kelas] = kelas?.Id;
        if (!first) tempDataDict[TempDataKeys.NilaiKriteria] = kriteria?.Id;

        return View(new IndexVM
        {
            Tahun = tahunAjaran?.Id,
            TahunAjaran = tahunAjaran,
            IdKelas = kelas?.Id,
            Kelas = kelas,
            Jurusan = jurusan,
            Id = id,
            Kriteria = kriteria,
            DaftarEntry = (await _siswaRepository.GetAll(jurusan, tahun, idKelas)).ToIndexEntryList(kriteria?.Id)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Simpan(IndexVM vm)
    {
        if (vm.Id is null)
        {
            _notificationService.AddError("Kriteria harus dipilih!", "Simpan");
            return RedirectToActionPermanent(nameof(Index), new { vm.Id, vm.Jurusan, vm.Tahun, vm.IdKelas });
        }

        var kriteria = await _kriteriaRepository.Get(vm.Id.Value);
        if (kriteria is null)
        {
            _notificationService.AddError($"Kriteria dengan Id {vm.Id} tidak ditemukan!", "Simpan");
            return RedirectToActionPermanent(nameof(Index), new { vm.Id, vm.Jurusan, vm.Tahun, vm.IdKelas });
        }

        foreach (var entry in vm.DaftarEntry)
        {
            var siswa = await _siswaRepository.Get(entry.IdSiswa);
            if (siswa is null || entry.Nilai is null) continue;

            var siswaKriteria = siswa.DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == kriteria.Id);

            if (siswaKriteria is null)
            {
                siswaKriteria = new SiswaKriteria
                {
                    IdSiswa = siswa.Id,
                    IdKriteria = kriteria.Id,
                    Nilai = default
                };

                _siswaKriteriaRepository.Add(siswaKriteria);
            }

            siswaKriteria.Nilai = entry.Nilai.Value;
        }

        var result = await _unitOfWork.SaveChangesAsync();
        if (result.IsSuccess)
            _notificationService.AddSuccess("Simpan Berhasil");
        else
            _notificationService.AddError("Simpan Gagal");

        return RedirectToActionPermanent(nameof(Index), new { vm.Id, vm.Jurusan, vm.Tahun, vm.IdKelas });
    }

    public async Task<IActionResult> Reset(
        Jurusan jurusan,
        int id,
        int tahun,
        int? idKelas = null,
        string? returnUrl = null)
    {
        returnUrl ??= Url.Action(nameof(Index))!;

        var kriteria = await _kriteriaRepository.Get(id);
        if (kriteria is null)
        {
            _notificationService.AddError($"Kriteria dengan Id {id} tidak ditemukan");
            return RedirectPermanent(returnUrl);
        }

        var daftarSiswa = await _siswaRepository.GetAll(jurusan, tahun, idKelas);

        foreach (var siswa in daftarSiswa)
        {
            var siswaKriteria = siswa.DaftarSiswaKriteria.FirstOrDefault(x => x.Kriteria == kriteria);
            if (siswaKriteria is not null)
                _siswaKriteriaRepository.Delete(siswaKriteria);

            siswa.JumlahAbsen = null;
        }

        var result = await _unitOfWork.SaveChangesAsync();
        if (result.IsSuccess)
            _notificationService.AddSuccess("Reset Berhasil!");
        else
            _notificationService.AddError("Reset Gagal!");

        return RedirectPermanent(returnUrl);
    }

    public async Task<IActionResult> DownloadFormat(int id, int? tahun, Jurusan? jurusan, int? idKelas)
    {
        var kriteria = await _kriteriaRepository.Get(id);
        if (kriteria is null)
        {
            _notificationService.AddError($"Kriteria dengan Id {id} tidak ditemukan", "Download Format");
            return RedirectToAction(nameof(Index), new { id, tahun, jurusan, idKelas });
        }

        var kelas = idKelas is null ? null : await _kelasRepository.Get(idKelas.Value);

        var informasiSekolah = await _informasiSekolahRepository.Get();

        var fileBytes = System.IO.File.ReadAllBytes(
            Path.Combine(_webHostEnvironment.WebRootPath, "file/Template_Nilai_Kriteria.xlsx"));

        using var memoryStream = new MemoryStream();
        memoryStream.Write(fileBytes, 0, fileBytes.Length);

        using var spreadSheet = SpreadsheetDocument.Open(
            memoryStream,
            isEditable: true);

        var workBookPart = spreadSheet.WorkbookPart!;

        var sheet = workBookPart.Workbook.Sheets!.Elements<Sheet>().First();
        var workSheetPart = (WorksheetPart)workBookPart.GetPartById(sheet.Id!);
        var sheetData = workSheetPart.Worksheet.Elements<SheetData>().First();

        // Isi informasi sekolah
        var sekolahCell = sheetData.Descendants<Cell>().FirstOrDefault(x => x.CellReference == "D2");
        if (sekolahCell is null)
        {
            _notificationService.AddError("Format bermasalah!");
            return RedirectToAction(nameof(Index), new { tahun, jurusan, idKelas });
        }
        sekolahCell.CellValue = new CellValue(informasiSekolah.NamaSekolah);

        // Isi kriteria
        var kriteriaRow = sheetData.Descendants<Row>().First(x => x.RowIndex is not null && x.RowIndex == 3);

        var kriteriaNamaCell = kriteriaRow.Elements<Cell>().FirstOrDefault(x => x.CellReference == "D3");
        if (kriteriaNamaCell is not null)
            kriteriaNamaCell.CellValue = new CellValue(kriteria.Nama);
        else
            kriteriaRow.AppendChild(new Cell { CellReference = "D3", CellValue = new CellValue(kriteria.Nama) });

        var kriteriaIdCell = kriteriaRow.Elements<Cell>().FirstOrDefault(x => x.CellReference == "E3");
        if (kriteriaIdCell is not null)
            kriteriaIdCell.CellValue = new CellValue(kriteria.Id);
        else
            kriteriaRow.AppendChild(new Cell { CellReference = "D4", CellValue = new CellValue(kriteria.Id) });

        var daftarSiswa = await _siswaRepository.GetAll(jurusan, tahun, idKelas);

        if (daftarSiswa.Count > 0)
        {
            var firstRow = sheetData.Descendants<Row>().First(x => x.RowIndex is not null && x.RowIndex == 5);
            var firstCells = firstRow.Elements<Cell>().ToList();
            firstCells[1].CellValue = new CellValue(daftarSiswa[0].Nama);
            firstCells[2].DataType = CellValues.String;
            firstCells[2].CellValue = new CellValue(daftarSiswa[0].NISN);
            firstCells[3].CellValue = new CellValue(daftarSiswa[0].DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == kriteria.Id)?.Nilai ?? 0);

            var rowIndex = 6u;
            var index = 2;
            foreach (var siswa in daftarSiswa.Skip(1))
            {
                var row = new Row { RowIndex = rowIndex };

                row.Append(
                    new Cell
                    {
                        CellReference = $"A{rowIndex}",
                        CellValue = new CellValue(index),
                        StyleIndex = firstCells[0].StyleIndex,
                    },
                    new Cell
                    {
                        CellReference = $"B{rowIndex}",
                        CellValue = new CellValue(siswa.Nama),
                        StyleIndex = firstCells[1].StyleIndex,
                    },
                    new Cell
                    {
                        CellReference = $"C{rowIndex}",
                        DataType = CellValues.String,
                        CellValue = new CellValue(siswa.NISN),
                        StyleIndex = firstCells[2].StyleIndex,
                    },
                    new Cell
                    {
                        CellReference = $"D{rowIndex}",
                        CellValue = new CellValue(siswa.DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == kriteria.Id)?.Nilai ?? 0),
                        StyleIndex = firstCells[3].StyleIndex,
                    }
                );

                sheetData.Append(row);
                rowIndex++;
                index++;
            }
        }

        workSheetPart.Worksheet.Save();
        workBookPart.Workbook.Save();
        spreadSheet.Save();

        return File(
            memoryStream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileDownloadName: $"Template_Nilai_Kriteria_{kriteria.Nama}" +
            $"{(tahun is not null ? $"_{tahun}" : "")}" +
            $"{(jurusan is not null ? $"_{jurusan.Value.Humanize()}" : "")}" +
            $"{(kelas is not null ? $"_{kelas.Nama}" : "")}" +
            $".xlsx");
    }

    [HttpPost]
    public async Task<IActionResult> Import(ImportNilaiKriteriaVM vm)
    {
        var returnUrl = vm.ReturnUrl ?? Url.ActionLink(nameof(Index))!;

        if (!ModelState.IsValid)
        {
            _notificationService.AddError("Data tidak valid", "Import");
            return RedirectPermanent(returnUrl);
        }

        var kriteria = await _kriteriaRepository.Get(vm.Id);
        if (kriteria is null)
        {
            _notificationService.AddError($"Kriteria dengan Id {vm.Id}", "Import");
            return RedirectPermanent(returnUrl);
        }

        if (vm.FormFile is null)
        {
            _notificationService.AddError("File harus diupload", "Import");
            return RedirectPermanent(returnUrl);
        }

        var file = await _fileService.ProcessFormFile<ImportNilaiKriteriaVM>(
            vm.FormFile,
            [".xlsx"],
            0,
            long.MaxValue);

        if (file.IsFailure)
        {
            _notificationService.AddError(file.Error.Message, "Import");
            return RedirectPermanent(returnUrl);
        }

        using var memoryStream = new MemoryStream(file.Value);
        using var spreadSheet = SpreadsheetDocument.Open(memoryStream, false);

        var workBookPart = spreadSheet.WorkbookPart!;
        var sharedStrings = workBookPart
            .SharedStringTablePart?
            .SharedStringTable
            .Elements<SharedStringItem>()
            .Select(s => s.InnerText)
            .ToList() ?? [];

        var sheet = workBookPart.Workbook.Sheets!.Elements<Sheet>().First();
        var workSheetPart = (WorksheetPart)workBookPart.GetPartById(sheet.Id!);
        var sheetData = workSheetPart.Worksheet.Elements<SheetData>().First();

        int jumlahDiproses = 0;
        int jumlahDitemukan = 0;
        int jumlahTidakDitemukan = 0;

        //Check Format Id
        var formatIdCell = sheetData.Descendants<Cell>().FirstOrDefault(x => x.CellReference == "C1");
        if (formatIdCell is null || HelperFunctions.GetCellValues(formatIdCell, sharedStrings) != FormatId)
        {
            _notificationService.AddError("Format import salah!");
            return RedirectPermanent(returnUrl);
        }

        //Check Kriteria
        var kriteriaIdCell = sheetData.Descendants<Cell>().FirstOrDefault(x => x.CellReference == "E3");
        if (kriteriaIdCell is null)
        {
            _notificationService.AddError("Id Kriteria tidak ditemukan", "Format tidak valid");
            return RedirectPermanent(returnUrl);
        }

        var kriteriaIdStr = HelperFunctions.GetCellValues(kriteriaIdCell, sharedStrings);
        if (string.IsNullOrWhiteSpace(kriteriaIdStr) ||
            !int.TryParse(kriteriaIdStr, out var kriteriaId) ||
            kriteria.Id != kriteriaId)
        {
            _notificationService.AddError("Id kriteria tidak benar", "Format tidak valid");
            return RedirectPermanent(returnUrl);
        }

        foreach (var row in sheetData.Elements<Row>().Skip(4))
        {
            var cells = row.Elements<Cell>().ToList();

            var nisn = HelperFunctions.GetCellValues(cells[2], sharedStrings);
            if (string.IsNullOrWhiteSpace(nisn)) continue;

            var nilaiString = HelperFunctions.GetCellValues(cells[3], sharedStrings);
            if (string.IsNullOrWhiteSpace(nilaiString) || !double.TryParse(nilaiString, out var nilai))
                continue;

            var siswa = await _siswaRepository.Get(nisn);
            if (siswa is null)
            {
                jumlahTidakDitemukan++;
                continue;
            }
            jumlahDitemukan++;

            var siswaKriteria = siswa.DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == kriteria.Id);
            if (siswaKriteria is null)
            {
                siswaKriteria = new SiswaKriteria
                {
                    Siswa = siswa,
                    Kriteria = kriteria,
                    Nilai = default
                };

                _siswaKriteriaRepository.Add(siswaKriteria);
            }

            siswaKriteria.Nilai = nilai;
            jumlahDiproses++;
        }

        if (jumlahDitemukan == 0)
        {
            _notificationService.AddError(
                "NISN siswa tidak ditemukan, silakan tambah siswa di Data Siswa",
                "Import");

            return RedirectPermanent(returnUrl);
        }

        var result = await _unitOfWork.SaveChangesAsync();
        if (result.IsSuccess)
            if (jumlahDiproses == 0)
                _notificationService.AddWarning(
                    "Import berhasil, tetapi tidak ada perubahan data",
                    "Import");
            else
                _notificationService.AddSuccess(
                    $"Import berhasil. {jumlahDiproses} data diperbarui",
                    "Import");
        else _notificationService.AddError("Import Gagal", "Import");

        return RedirectPermanent(returnUrl);
    }

    public async Task<IActionResult> PDF(int id, int tahun, Jurusan jurusan, int? idKelas = null)
    {
        var kelas = idKelas is null ? null : await _kelasRepository.Get(idKelas.Value);

        var kriteria = await _kriteriaRepository.Get(id);
        if (kriteria is null)
        {
            _notificationService.AddError($"Kriteria dengan Id {id} tidak ditemukan", "PDF");
            return RedirectToActionPermanent(nameof(Index), new { tahun, jurusan, idKelas });
        }

        var indexVM = new IndexVM
        {
            Tahun = tahun,
            Jurusan = jurusan,
            Kelas = kelas,
            IdKelas = kelas?.Id,
            DaftarEntry = (await _siswaRepository.GetAll(jurusan, tahun, kelas?.Id)).ToIndexEntryList(kriteria.Id)
        };

        var html = await _templateEngine.RenderAsync("Areas/Dashboard/Views/NilaiKriteria/PDF.cshtml", indexVM);

        var pdf = await _pDFGeneratorService.GeneratePDF(
            html,
            marginTop: 75,
            marginBottom: 75,
            marginLeft: 75,
            marginRight: 75);

        return File(
            pdf,
            "application/pdf",
            fileDownloadName: $"NilaiKriteria-{kriteria.Nama}-{tahun}-{jurusan}{(kelas is null ? "" : $"-{kelas.Nama}")}.pdf"
        );
    }

    public async Task<IActionResult> Excel(int id, int tahun, Jurusan jurusan, int? idKelas = null)
    {
        var kriteria = await _kriteriaRepository.Get(id);
        if (kriteria is null)
        {
            _notificationService.AddError($"Kriteria dengan Id {id} tidak ditemukan");
            return RedirectToActionPermanent(nameof(Index), new { tahun, jurusan, idKelas });
        }

        var kelas = idKelas is null ? null : await _kelasRepository.Get(idKelas.Value);

        var daftarEntry = (await _siswaRepository.GetAll(jurusan, tahun, idKelas)).ToIndexEntryList(kriteria.Id);

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
                    CellValue = new CellValue(kriteria.Nama),
                    StyleIndex = 2,
                }
            ]
        );
        sheetData.Append(headerRow);

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
                    CellValue = new(entry.Nilai?.ToString() ?? "-"),
                    StyleIndex = 2,
                }
            ]);

            sheetData.Append(row);
        }

        spreadSheet.Save();

        return File(
            memoryStream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileDownloadName: $"NilaiKriteria{kriteria.Nama}-{tahun}-{jurusan}{(kelas is null ? "" : $"-{kelas.Nama}")}.xlsx"
        );
    }
}
