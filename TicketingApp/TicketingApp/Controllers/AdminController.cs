using Microsoft.AspNetCore.Mvc;

namespace TicketingApp.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
