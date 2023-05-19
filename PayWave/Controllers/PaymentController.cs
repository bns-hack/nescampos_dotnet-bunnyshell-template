using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayWave.Data;
using PayWave.Models.DTO;
using PayWave.Models.PaymentModels;
using RestSharp;

namespace PayWave.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private IConfiguration _configuration;
        private ApplicationDbContext _db { get; set; }
        public PaymentController(IConfiguration configuration, ApplicationDbContext db)
        {
            _configuration = configuration;
            _db = db;
        }

        public IActionResult Index(long? walletId, DateTime? from, DateTime? to)
        {
            IndexPaymentViewModel model = new IndexPaymentViewModel(_db, User.Identity.Name);
            string url = "/transfers?";
            if (walletId.HasValue)
            {
                Wallet wallet = _db.Wallets.SingleOrDefault(x => x.Id == walletId.Value);
                url += "walletId="+wallet.Account;
                if(from.HasValue)
                {
                    string formattedDateTime = from.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ").Replace(":", "%3A").Replace(".", "%2E");
                    url += "&from="+ formattedDateTime;
                }
                if (to.HasValue)
                {
                    string formattedDateTime = to.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ").Replace(":", "%3A").Replace(".", "%2E");
                    url += "&to=" + formattedDateTime;
                }
                var client = new RestClient(_configuration["CircleAPIBaseUrl"]);
                var request = new RestRequest(url, Method.Get);
                request.AddHeader("accept", "application/json");
                request.AddHeader("content-type", "application/json");
                request.AddHeader("authorization", "Bearer " + _configuration["CircleAPIKey"]);
                RestResponse<TransferListDTO> response = client.Execute<TransferListDTO>(request);
                model.TransferList = response.Data.data;
            }
            return View(model);
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
            return View(model);
        }

        public IActionResult SendSelector()
        {
            return View();
        }

        public IActionResult Send()
        {
            SendPaymentViewModel model = new SendPaymentViewModel(_db, User.Identity.Name);
            return View(model);
        }

        [HttpPost]
        public IActionResult Send(SendPaymentFormModel Form)
        {
            SendPaymentViewModel model = new SendPaymentViewModel(_db, User.Identity.Name);
            model.Form = Form;
            if (!ModelState.IsValid)
            {
                
                return View(model);
            }
            Wallet walletOrigin = _db.Wallets.SingleOrDefault(x => x.Id == Form.OriginWalletId.Value);
            Receiver walletDestination = _db.Receivers.SingleOrDefault(x => x.Id == Form.RecipientId.Value);

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

            

            if(walletDestination.Type == "blockchain")
            {
                string tag = walletDestination.BlockchainAddressTag != null ? ",\"addressTag\":\"" + walletDestination.BlockchainAddressTag + "\"" : "";
                request.AddParameter("application/json", "{\"source\":{\"type\":\"wallet\",\"id\":\"" + walletOrigin.Account + "\"},\"amount\":{\"currency\":\"" + Form.OriginCurrency + "\",\"amount\":\"" + Form.Amount + "\"},\"destination\":{\"type\":\"blockchain\",\"chain\":\"" + walletDestination.Chain + "\",\"address\":\"" + walletDestination.BlockchainAddress + "\"},\"idempotencyKey\":\"" + idempotencyKey + "\"" + tag + "}", ParameterType.RequestBody);
                RestResponse<TransferDTO> response = client.Execute<TransferDTO>(request);
                if(response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Details", new { id = response.Data.data.id });
                }
                else
                {
                    ModelState.AddModelError("Form.OriginWalletId", "Error in send the payments. Please, try again later.");
                    return View(model);
                }
                
            }
            else
            {
                Wallet recipient = _db.Wallets.SingleOrDefault(x => x.Account == walletDestination.WalletId || x.Alias == walletDestination.WalletId);
                request.AddParameter("application/json", "{\"source\":{\"type\":\"wallet\",\"id\":\"" + walletOrigin.Account + "\"},\"amount\":{\"currency\":\"" + Form.OriginCurrency + "\",\"amount\":\"" + Form.Amount.ToString() + "\"},\"destination\":{\"type\":\"wallet\",\"id\":\"" + recipient.Account + "\"},\"idempotencyKey\":\"" + idempotencyKey + "\"}", ParameterType.RequestBody);
                RestResponse<TransferDTO> response = client.Execute<TransferDTO>(request);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Details", new { id = response.Data.data.id });
                }
                else
                {
                    ModelState.AddModelError("Form.OriginWalletId", "Error in send the payments. Please, try again later.");
                    return View(model);
                }
            }
        }
    }
}
