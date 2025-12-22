using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SpkSnbp.Domain.Auth;
using SpkSnbp.Domain.Contracts;
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
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(
        ILogger<HomeController> logger,
        ISignInManager signInManager,
        IToastrNotificationService notificationService,
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _signInManager = signInManager;
        _notificationService = notificationService;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
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

    [AllowAnonymous]
    public IActionResult Daftar() => View(new DaftarVM());

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Daftar(DaftarVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        if (await _userRepository.IsExist(vm.UserName))
        {
            ModelState.AddModelError(nameof(vm.UserName), "User name sudah digunakan");
            return View(vm);
        }

        if (vm.Role != UserRoles.KepalaSekolah && vm.Role != UserRoles.WaliKelas)
        {
            ModelState.AddModelError(nameof(vm.Role), "Tidak valid");
            return View(vm);
        }

        var user = new User
        {
            UserName = vm.UserName,
            Role = vm.Role,
            PasswordHash = _passwordHasher.HashPassword(null, vm.Password)
        };

        _userRepository.Add(user);
        var result = await _unitOfWork.SaveChangesAsync();
        if (result.IsFailure)
        {
            _notificationService.AddError("Daftar Gagal");
            return View(vm);
        }

        _notificationService.AddSuccess("Daftar Berhasil");
        return RedirectToActionPermanent(nameof(Login));
    }
}
