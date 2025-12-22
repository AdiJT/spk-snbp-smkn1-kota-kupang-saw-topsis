using SpkSnbp.Domain.Auth;
using SpkSnbp.Domain.Shared;

namespace SpkSnbp.Web.Authentication;

public interface ISignInManager
{
    Task<Result<string>> Login(string username, string password, bool rememberMe);
    Task<bool> IsInRole(string roleName);
    Task Logout();
    Task<User?> GetUser();
}
