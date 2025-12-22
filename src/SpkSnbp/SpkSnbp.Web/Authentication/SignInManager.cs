using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using SpkSnbp.Domain.Auth;
using SpkSnbp.Domain.Shared;
using System.Security.Claims;

namespace SpkSnbp.Web.Authentication;

public class SignInManager : ISignInManager
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<SignInManager> _logger;
    private readonly IHttpContextAccessor _contextAccessor;

    public SignInManager(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        ILogger<SignInManager> logger,
        IHttpContextAccessor contextAccessor)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _contextAccessor = contextAccessor;
    }

    public async Task<User?> GetUser()
    {
        var httpContext = _contextAccessor.HttpContext;
        if (httpContext is null) return null;

        var userName = httpContext.User.Identity?.Name;
        if (userName is null) return null;

        var appUser = await _userRepository.Get(userName);

        return appUser; 
    }

    public async Task<Result<string>> Login(string username, string password, bool rememberMe)
    {
        var httpContext = _contextAccessor.HttpContext;
        if (httpContext is null)
            return new Error("Login.Gagal", "Tidak ada HttpContext aktif");

        var appUser = await _userRepository.Get(username);
        if (appUser is null)
            return new Error("Login.AkunTidakDitemukan", $"Akun '{username}' tidak ditemukan");

        if (_passwordHasher.VerifyHashedPassword(appUser, appUser.PasswordHash, password) == PasswordVerificationResult.Failed)
            return new Error("Login.PasswordSalah", "Password yang dimasukan salah!");

        List<Claim> claims = [
            new Claim(ClaimTypes.Name, appUser.UserName),
            new Claim(ClaimTypes.Role, appUser.Role)
        ];

        var claimIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimPrincipal = new ClaimsPrincipal(claimIdentity);
        var authProperties = new AuthenticationProperties { IsPersistent = rememberMe, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(4) };

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimPrincipal, authProperties);

        _logger.LogInformation("{@userName} logged in at {@time}", username, DateTime.Now);

        return appUser.Role;
    }

    public async Task Logout()
    {
        var httpContext = _contextAccessor.HttpContext;

        if (httpContext is not null)
            await httpContext.SignOutAsync();
    }
}
