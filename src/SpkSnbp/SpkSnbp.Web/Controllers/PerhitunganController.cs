using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Domain.Shared;
using SpkSnbp.Web.Models.Perhitungan;
using SpkSnbp.Web.Services.Toastr;
using SpkSnbp.Web.Services.TopsisSAW;

namespace SpkSnbp.Web.Controllers;

[Authorize]
public class PerhitunganController : Controller
{
    private readonly ISiswaRepository _siswaRepository;
    private readonly ITahunAjaranRepository _tahunAjaranRepository;
    private readonly ITopsisSAWService _topsisSAWService;
    private readonly IToastrNotificationService _notificationService;
    private readonly IHasilPerhitunganRepository _hasilPerhitunganRepository;

    public PerhitunganController(
        ISiswaRepository siswaRepository,
        ITahunAjaranRepository tahunAjaranRepository,
        ITopsisSAWService topsisSAWService,
        IToastrNotificationService notificationService,
        IHasilPerhitunganRepository hasilPerhitunganRepository)
    {
        _siswaRepository = siswaRepository;
        _tahunAjaranRepository = tahunAjaranRepository;
        _topsisSAWService = topsisSAWService;
        _notificationService = notificationService;
        _hasilPerhitunganRepository = hasilPerhitunganRepository;
    }

    public async Task<IActionResult> Index(Jurusan jurusan = Jurusan.TJKT, int? tahun = null)
    {
        var tahunAjaran = tahun is null ? 
            await _tahunAjaranRepository.Get(CultureInfos.DateOnlyNow.Year) : 
            await _tahunAjaranRepository.Get(tahun.Value);

        tahunAjaran ??= await _tahunAjaranRepository.GetLatest();

        if (tahunAjaran is null)
            return View(new IndexVM { Jurusan = jurusan, DaftarSiswa = []});

        return View(new IndexVM
        {
            Jurusan = jurusan,
            Tahun = tahunAjaran.Id,
            TahunAjaran = tahunAjaran,
            DaftarSiswa = (await _siswaRepository.GetAll(jurusan, tahunAjaran.Id)),
            HasilPerhitungan = await _hasilPerhitunganRepository.Get(tahunAjaran.Id, jurusan)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Hitung(Jurusan jurusan, int tahun)
    {
        var tahunAjaran = await _tahunAjaranRepository.Get(tahun);
        if (tahunAjaran is null)
        {
            _notificationService.AddError("Tahun tidak ditemukan");
            return RedirectToAction(nameof(Index), new { jurusan });
        }

        var result = await _topsisSAWService.Perhitungan(tahun, jurusan);
        if (result.IsSuccess)
            _notificationService.AddSuccess("Perhitungan Sukses");
        else
            _notificationService.AddError(result.Error.Message, "Perhitungan Gagal");

        return RedirectToActionPermanent(nameof(Index), new { jurusan, tahun });
    }
}
