using Microsoft.AspNetCore.Mvc;

namespace Product.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Order()
        {
            return View();
        }
    }
}
