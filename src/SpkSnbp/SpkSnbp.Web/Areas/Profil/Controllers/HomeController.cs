using Microsoft.AspNetCore.Mvc;

namespace SpkSnbp.Web.Areas.Profil.Controllers;

[Area(AreaNames.Profil)]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
