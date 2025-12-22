using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpkSnbp.Domain.Auth;
using SpkSnbp.Domain.Contracts;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Web.Models.KriteriaModels;
using SpkSnbp.Web.Services.Toastr;

namespace SpkSnbp.Web.Controllers;

[Authorize(Roles = UserRoles.Admin)]
public class KriteriaController : Controller
{
    private readonly IKriteriaRepository _kriteriaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IToastrNotificationService _notificationService;

    public KriteriaController(
        IKriteriaRepository kriteriaRepository,
        IUnitOfWork unitOfWork,
        IToastrNotificationService notificationService)
    {
        _kriteriaRepository = kriteriaRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<IActionResult> Index() => View(await _kriteriaRepository.GetAll());

    [HttpPost]
    public async Task<IActionResult> Edit(EditVM vm)
    {
        var returnUrl = vm.ReturnUrl ?? Url.ActionLink(nameof(Index))!;

        if (!ModelState.IsValid)
        {
            _notificationService.AddError("Data tidak valid", "Edit");
            return RedirectPermanent(returnUrl);
        }

        var kriteria = await _kriteriaRepository.Get(vm.Id);
        if (kriteria is null)
        {
            _notificationService.AddError("Kriteria tidak ditemukan", "Edit");
            return RedirectPermanent(returnUrl);
        }

        kriteria.Bobot = vm.Bobot;
        kriteria.Jenis = vm.Jenis;

        var result = await _unitOfWork.SaveChangesAsync();
        if (result.IsSuccess)
            _notificationService.AddSuccess("Simpan Berhasil", "Edit");
        else
            _notificationService.AddError("Simpan Gagal", "Edit");

        return RedirectPermanent(returnUrl);
    }
}
