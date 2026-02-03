using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpkSnbp.Domain.Auth;
using SpkSnbp.Domain.Contracts;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Web.Areas.Dashboard.Models.InformasiSekolahModels;
using SpkSnbp.Web.Services.Toastr;

namespace SpkSnbp.Web.Areas.Dashboard.Controllers;

[Authorize(Roles = UserRoles.Admin)]
[Area(AreaNames.Dashboard)]
public class InformasiSekolahController : Controller
{
    private readonly IInformasiSekolahRepository _informasiSekolahRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IToastrNotificationService _notificationService;

    public InformasiSekolahController(
        IInformasiSekolahRepository informasiSekolahRepository,
        IUnitOfWork unitOfWork,
        IToastrNotificationService notificationService)
    {
        _informasiSekolahRepository = informasiSekolahRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<IActionResult> Index() 
    {
        var informasiSekolah = await _informasiSekolahRepository.Get();

        return View(new EditVM
        {
            NPSN = informasiSekolah.NPSN,
            NamaSekolah = informasiSekolah.NamaSekolah,
            BentukPendidikan = informasiSekolah.BentukPendidikan,
            Akreditasi = informasiSekolah.Akreditasi,
            Nilai = informasiSekolah.Nilai,
            NoSKAkreditasi = informasiSekolah.NoSKAkreditasi,
            TanggalSKAkreditasi = informasiSekolah.TanggalSKAkreditasi,
            TMTMulaiSKAkreditasi = informasiSekolah.TMTMulaiSKAkreditasi,
            TMTSelesaiSKAkreditasi = informasiSekolah.TMTSelesaiSKAkreditasi,
            KepalaSekolah = informasiSekolah.KepalaSekolah,
            NoHP = informasiSekolah.NoHP,
            Jalan = informasiSekolah.Jalan,
            DesaKelurahan = informasiSekolah.DesaKelurahan,
            KecamatanDistrik = informasiSekolah.KecamatanDistrik,
            KabupatenKota = informasiSekolah.KabupatenKota,
            Provinsi = informasiSekolah.Provinsi,
            KodePos = informasiSekolah.KodePos,
        });
    }

    public async Task<IActionResult> Edit()
    {
        var informasiSekolah = await _informasiSekolahRepository.Get();

        return View(new EditVM
        {
            NPSN = informasiSekolah.NPSN,
            NamaSekolah = informasiSekolah.NamaSekolah,
            BentukPendidikan = informasiSekolah.BentukPendidikan,
            Akreditasi = informasiSekolah.Akreditasi,
            Nilai = informasiSekolah.Nilai,
            NoSKAkreditasi = informasiSekolah.NoSKAkreditasi,
            TanggalSKAkreditasi = informasiSekolah.TanggalSKAkreditasi,
            TMTMulaiSKAkreditasi = informasiSekolah.TMTMulaiSKAkreditasi,
            TMTSelesaiSKAkreditasi = informasiSekolah.TMTSelesaiSKAkreditasi,
            KepalaSekolah = informasiSekolah.KepalaSekolah,
            NoHP = informasiSekolah.NoHP,
            Jalan = informasiSekolah.Jalan,
            DesaKelurahan = informasiSekolah.DesaKelurahan,
            KecamatanDistrik = informasiSekolah.KecamatanDistrik,
            KabupatenKota = informasiSekolah.KabupatenKota,
            Provinsi = informasiSekolah.Provinsi,
            KodePos = informasiSekolah.KodePos,
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var informasiSekolah = await _informasiSekolahRepository.Get();

        informasiSekolah.NPSN = vm.NPSN;
        informasiSekolah.NamaSekolah = vm.NamaSekolah;
        informasiSekolah.BentukPendidikan = vm.BentukPendidikan;
        informasiSekolah.Akreditasi = vm.Akreditasi;
        informasiSekolah.Nilai = vm.Nilai;
        informasiSekolah.NoSKAkreditasi = vm.NoSKAkreditasi;
        informasiSekolah.TanggalSKAkreditasi = vm.TanggalSKAkreditasi;
        informasiSekolah.TMTMulaiSKAkreditasi = vm.TMTMulaiSKAkreditasi;
        informasiSekolah.TMTSelesaiSKAkreditasi = vm.TMTSelesaiSKAkreditasi;
        informasiSekolah.KepalaSekolah = vm.KepalaSekolah;
        informasiSekolah.NoHP = vm.NoHP;
        informasiSekolah.Jalan = vm.Jalan;
        informasiSekolah.DesaKelurahan = vm.DesaKelurahan;
        informasiSekolah.KecamatanDistrik = vm.KecamatanDistrik;
        informasiSekolah.KabupatenKota = vm.KabupatenKota;
        informasiSekolah.Provinsi = vm.Provinsi;
        informasiSekolah.KodePos = vm.KodePos;

        var result = await _unitOfWork.SaveChangesAsync();
        if (result.IsFailure)
        {
            _notificationService.AddError("Simpan Gagal!");
            return View(vm);
        }

        _notificationService.AddSuccess("Simpan Berhasil!");
        return RedirectToActionPermanent(nameof(Index));
    }
}
