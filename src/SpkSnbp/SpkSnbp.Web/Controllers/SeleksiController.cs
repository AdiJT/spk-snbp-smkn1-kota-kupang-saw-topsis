using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Domain.Shared;
using SpkSnbp.Web.Models.Seleksi;
using SpkSnbp.Web.Services.Toastr;
using SpkSnbp.Web.Services.TopsisSAW;

namespace SpkSnbp.Web.Controllers;

[Authorize]
public class SeleksiController : Controller
{
    private readonly ISiswaRepository _siswaRepository;
    private readonly ITopsisSAWService _topsisSAWService;
    private readonly ITahunAjaranRepository _tahunAjaranRepository;
    private readonly IToastrNotificationService _toastrNotificationService;

    public SeleksiController(
        ISiswaRepository siswaRepository,
        ITopsisSAWService topsisSAWService,
        ITahunAjaranRepository tahunAjaranRepository,
        IToastrNotificationService toastrNotificationService)
    {
        _siswaRepository = siswaRepository;
        _topsisSAWService = topsisSAWService;
        _tahunAjaranRepository = tahunAjaranRepository;
        _toastrNotificationService = toastrNotificationService;
    }

    public async Task<IActionResult> Index(Jurusan jurusan = Jurusan.TJKT, int? tahun = null)
    {
        var tahunAjaran = tahun is null ?
            await _tahunAjaranRepository.Get(CultureInfos.DateOnlyNow.Year) :
            await _tahunAjaranRepository.Get(tahun.Value);

        tahunAjaran ??= await _tahunAjaranRepository.GetByLatest();

        if (tahunAjaran is null)
            return View(new IndexVM { Jurusan = jurusan, DaftarSiswa = [] });

        return View(new IndexVM
        {
            Jurusan = jurusan,
            Tahun = tahunAjaran.Id,
            TahunAjaran = tahunAjaran,
            DaftarSiswa = [.. (await _siswaRepository.GetAll(jurusan, tahunAjaran.Id))
                .Where(x => x.NilaiTopsis != null)
                .OrderByDescending(x => x.NilaiTopsis)]
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
}
