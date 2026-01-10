using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpkSnbp.Domain.Auth;
using SpkSnbp.Domain.Contracts;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Infrastructure.Services.FileServices;
using SpkSnbp.Web.Helpers;
using SpkSnbp.Web.Models;
using SpkSnbp.Web.Models.SiswaModels;
using SpkSnbp.Web.Services.Toastr;
using System.Text.RegularExpressions;

namespace SpkSnbp.Web.Controllers;

[Authorize(Roles = UserRoles.Admin)]
public class SiswaController : Controller
{
    private readonly ISiswaRepository _siswaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IToastrNotificationService _notificationService;
    private readonly ITahunAjaranRepository _tahunAjaranRepository;
    private readonly IFileService _fileService;

    public SiswaController(
        ISiswaRepository siswaRepository,
        IUnitOfWork unitOfWork,
        IToastrNotificationService notificationService,
        ITahunAjaranRepository tahunAjaranRepository,
        IFileService fileService)
    {
        _siswaRepository = siswaRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _tahunAjaranRepository = tahunAjaranRepository;
        _fileService = fileService;
    }

    public async Task<IActionResult> Index(Jurusan? jurusan = null, int? tahun = null)
    {
        var tahunAjaran = tahun is null ? null : await _tahunAjaranRepository.Get(tahun.Value);

        if (tahunAjaran is null) 
            return View(new IndexVM
            {
                Jurusan = jurusan,
                DaftarSiswa = await _siswaRepository.GetAll(jurusan)
            });

        return View(new IndexVM
        {
            Jurusan = jurusan,
            Tahun = tahun,
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
}
