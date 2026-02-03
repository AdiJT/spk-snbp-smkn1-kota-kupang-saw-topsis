using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SpkSnbp.Domain.Auth;
using SpkSnbp.Domain.Contracts;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Web.Areas.Dashboard.Models.Home;
using SpkSnbp.Web.Authentication;
using SpkSnbp.Web.Services.Toastr;

namespace SpkSnbp.Web.Areas.Dashboard.Controllers;

[Authorize]
[Area(AreaNames.Dashboard)]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ISignInManager _signInManager;
    private readonly IToastrNotificationService _notificationService;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISiswaRepository _siswaRepository;
    private readonly ITahunAjaranRepository _tahunAjaranRepository;
    private readonly IKriteriaRepository _kriteriaRepository;

    public HomeController(
        ILogger<HomeController> logger,
        ISignInManager signInManager,
        IToastrNotificationService notificationService,
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        IUnitOfWork unitOfWork,
        ISiswaRepository siswaRepository,
        ITahunAjaranRepository tahunAjaranRepository,
        IKriteriaRepository kriteriaRepository)
    {
        _logger = logger;
        _signInManager = signInManager;
        _notificationService = notificationService;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _siswaRepository = siswaRepository;
        _tahunAjaranRepository = tahunAjaranRepository;
        _kriteriaRepository = kriteriaRepository;
    }

    public async Task<IActionResult> Index()
    {
        return View(new IndexVM 
        { 
            DaftarSiswa = await _siswaRepository.GetAll(),
            DaftarTahunAjaran = await _tahunAjaranRepository.GetAll(),
            DaftarKriteria = await _kriteriaRepository.GetAll(),
        });
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
