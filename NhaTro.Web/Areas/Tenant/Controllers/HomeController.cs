using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NhaTro.Web.Areas.Tenant.Controllers
{
    [Area("Tenant")]
    [Authorize(Roles = "Tenant")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}