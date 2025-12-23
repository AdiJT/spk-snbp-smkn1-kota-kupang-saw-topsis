using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpkSnbp.Domain.Auth;
using SpkSnbp.Domain.Contracts;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Web.Models.SiswaModels;
using SpkSnbp.Web.Services.Toastr;

namespace SpkSnbp.Web.Controllers;

[Authorize(Roles = UserRoles.Admin)]
public class SiswaController : Controller
{
    private readonly ISiswaRepository _siswaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IToastrNotificationService _notificationService;
    private readonly ITahunAjaranRepository _tahunAjaranRepository;

    public SiswaController(
        ISiswaRepository siswaRepository,
        IUnitOfWork unitOfWork,
        IToastrNotificationService notificationService,
        ITahunAjaranRepository tahunAjaranRepository)
    {
        _siswaRepository = siswaRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _tahunAjaranRepository = tahunAjaranRepository;
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
}
