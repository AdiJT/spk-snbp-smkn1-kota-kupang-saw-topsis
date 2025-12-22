using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SpkSnbp.Web.Models;
using SpkSnbp.Web.Services.Toastr;

namespace SpkSnbp.Web.ViewComponents;

public class ToastrViewComponent : ViewComponent
{
    private readonly IToastrNotificationService _notificationService;

    public ToastrViewComponent(IToastrNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public IViewComponentResult Invoke(ToastrOptions globalOptions)
    {
        string globalOptionsJson = JsonConvert.SerializeObject(globalOptions, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
        });
        return View((globalOptionsJson, _notificationService.GetNotificationJson()));
    }
}
