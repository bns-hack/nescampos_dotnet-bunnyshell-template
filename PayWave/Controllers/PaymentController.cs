using Microsoft.AspNetCore.Mvc;
using PayWave.Data;
using PayWave.Models.DTO;
using PayWave.Models.PaymentModels;
using RestSharp;

namespace PayWave.Controllers
{
    public class PaymentController : Controller
    {
        private IConfiguration _configuration;
        private ApplicationDbContext _db { get; set; }
        public PaymentController(IConfiguration configuration, ApplicationDbContext db)
        {
            _configuration = configuration;
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SendPersonal()
        {
            SendPersonalPaymentViewModel model = new SendPersonalPaymentViewModel(_db, User.Identity.Name);
            return View(model);
        }

        [HttpPost]
        public IActionResult SendPersonal(SendPersonalPaymentFormModel Form)
        {
            if (Form.DestinationWalletId.HasValue && Form.OriginWalletId.HasValue)
            {
                if (Form.DestinationWalletId == Form.OriginWalletId)
                {
                    ModelState.AddModelError("Form.DestinationWalletId", "The destination account cannot be the same as the source account.");
                }
            }
            if (!ModelState.IsValid)
            {
                SendPersonalPaymentViewModel model = new SendPersonalPaymentViewModel(_db, User.Identity.Name);
                model.Form = Form;
                return View(model);
            }
            Wallet walletOrigin = _db.Wallets.SingleOrDefault(x => x.Id == Form.OriginWalletId.Value);
            Wallet walletDestination = _db.Wallets.SingleOrDefault(x => x.Id == Form.DestinationWalletId.Value);

            if (walletOrigin.UserId != User.Identity.Name || walletDestination.UserId != User.Identity.Name)
            {
                return View("Unauthorized");
            }

            Guid idempotencyKey = Guid.NewGuid();
            var client = new RestClient(_configuration["CircleAPIBaseUrl"]);
            var request = new RestRequest("/transfers", Method.Post);
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authorization", "Bearer " + _configuration["CircleAPIKey"]);
            request.AddParameter("application/json", "{\"source\":{\"type\":\"wallet\",\"id\":\""+ walletOrigin.Account+ "\"},\"amount\":{\"currency\":\""+Form.Currency+"\",\"amount\":\""+Form.Amount.ToString()+"\"},\"destination\":{\"type\":\"wallet\",\"id\":\"" + walletDestination.Account + "\"},\"idempotencyKey\":\""+idempotencyKey+"\"}", ParameterType.RequestBody);
            RestResponse<TransferDTO> response = client.Execute<TransferDTO>(request);

            return RedirectToAction("Details", new { id = response.Data.data.id });
        }

        public ActionResult Details(string id)
        {
            Guid idempotencyKey = Guid.NewGuid();
            var client = new RestClient(_configuration["CircleAPIBaseUrl"]);
            var request = new RestRequest("/transfers/"+id, Method.Get);
            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", "Bearer " + _configuration["CircleAPIKey"]);
            RestResponse<TransferDTO> response = client.Execute<TransferDTO>(request);
            DetailsPaymentViewModel model = new DetailsPaymentViewModel(_db, response);
            ////if (model.Payment.Wallet.UserId != User.Identity.Name)
            ////{
            ////    return View("Unauthorized");
            ////}
            //return View(model);
            return View(model);
        }
    }
}
