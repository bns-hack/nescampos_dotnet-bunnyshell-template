using Microsoft.AspNetCore.Mvc;

namespace PayWave.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
