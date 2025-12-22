using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpkSnbp.Web.Authentication;
using SpkSnbp.Web.Models.Home;
using SpkSnbp.Web.Services.Toastr;

namespace SpkSnbp.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ISignInManager _signInManager;
    private readonly IToastrNotificationService _notificationService;

    public HomeController(
        ILogger<HomeController> logger,
        ISignInManager signInManager,
        IToastrNotificationService notificationService)
    {
        _logger = logger;
        _signInManager = signInManager;
        _notificationService = notificationService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null) => View(new LoginVM { ReturnUrl = returnUrl });

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await _signInManager.Login(vm.UserName, vm.Password, vm.RememberMe);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            return View(vm);
        }

        return vm.ReturnUrl is null ? RedirectToAction("Index", "Home") : Redirect(vm.ReturnUrl);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.Logout();

        return RedirectToAction(nameof(Login));
    }
}
