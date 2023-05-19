using Microsoft.AspNetCore.Mvc;

namespace PayWave.Controllers
{
    public class WalletController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
