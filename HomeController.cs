using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Product.Areas.UserPanel.Controllers
{
    public class HomeController : Controller
    {
        [Area("UserPanel")]
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}
