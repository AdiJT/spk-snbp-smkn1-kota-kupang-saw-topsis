using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpkSnbp.Domain.Auth;
using SpkSnbp.Domain.Contracts;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Web.Models.TahunAjaranModels;
using SpkSnbp.Web.Services.Toastr;
using System.Threading.Tasks;

namespace SpkSnbp.Web.Controllers;

[Authorize(Roles = UserRoles.Admin)]
public class TahunAjaranController : Controller
{
    private readonly ITahunAjaranRepository _tahunAjaranRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IToastrNotificationService _notificationService;

    public TahunAjaranController(
        ITahunAjaranRepository tahunAjaranRepository,
        IUnitOfWork unitOfWork,
        IToastrNotificationService notificationService)
    {
        _tahunAjaranRepository = tahunAjaranRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
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
}
