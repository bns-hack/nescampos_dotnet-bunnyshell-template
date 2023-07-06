using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
                request.AddHeader("authorization", "Bearer " + Environment.GetEnvironmentVariable("CircleAPIKey"));
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
            request.AddHeader("authorization", "Bearer " + Environment.GetEnvironmentVariable("CircleAPIKey"));
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
            request.AddHeader("authorization", "Bearer " + Environment.GetEnvironmentVariable("CircleAPIKey"));
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
            request.AddHeader("authorization", "Bearer " + Environment.GetEnvironmentVariable("CircleAPIKey"));

            

            if(walletDestination.Type == "blockchain")
            {
                string tag = walletDestination.BlockchainAddressTag != null ? ",\"addressTag\":\"" + walletDestination.BlockchainAddressTag + "\"" : "";
                request.AddParameter("application/json", "{\"source\":{\"type\":\"wallet\",\"id\":\"" + walletOrigin.Account + "\"},\"amount\":{\"currency\":\"" + Form.OriginCurrency + "\",\"amount\":\"" + Form.Amount.ToString().Replace(",", ".") + "\"},\"destination\":{\"type\":\"blockchain\",\"chain\":\"" + walletDestination.Chain + "\",\"address\":\"" + walletDestination.BlockchainAddress + "\"},\"idempotencyKey\":\"" + idempotencyKey + "\"" + tag + "}", ParameterType.RequestBody);
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
                request.AddParameter("application/json", "{\"source\":{\"type\":\"wallet\",\"id\":\"" + walletOrigin.Account + "\"},\"amount\":{\"currency\":\"" + Form.OriginCurrency + "\",\"amount\":\"" + Form.Amount.ToString().Replace(",", ".") + "\"},\"destination\":{\"type\":\"wallet\",\"id\":\"" + recipient.Account + "\"},\"idempotencyKey\":\"" + idempotencyKey + "\"}", ParameterType.RequestBody);
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

        public IActionResult GenerateTransactionLink()
        {
            GenerateTransactionLinkViewModel model = new GenerateTransactionLinkViewModel(_db, User.Identity.Name);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateTransactionLink(CreateTransactionFormModel Form)
        {
            if (!ModelState.IsValid)
            {
                GenerateTransactionLinkViewModel model = new GenerateTransactionLinkViewModel(_db, User.Identity.Name);
                model.Form = Form;
                return View(model);
            }
            Wallet wallet = _db.Wallets.SingleOrDefault(x => x.Id == Form.WalletId);
            Transaction transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Amount = Form.Amount.Value,
                CreatedAt = DateTime.UtcNow,
                WalletId = Form.WalletId,
                Subtitle = Form.Subtitle,
                Title = Form.Title,
                //AllowedEmail = Form.AllowedEmail,
                //AllowedWalletId = Form.AllowedWalletId,
                ExpiredDate = Form.ExpiredDate,
                Currency = Form.Currency
            };
            _db.Transactions.Add(transaction);
            _db.SaveChanges();
            //string paymentLink = Url.Action("GeneratePayment", "Payment", new { id = transaction.Id }, HttpContext.Request.Scheme);
            return RedirectToAction("NewGenerateTransactionLink", new { id = transaction.Id });
        }

        public ActionResult NewGenerateTransactionLink(Guid id)
        {
            NewGenerateTransactionLinkViewModel model = new NewGenerateTransactionLinkViewModel(_db, User.Identity.Name, id);
            model.Link = Url.Action("GeneratePayment", "Payment", new { id = id }, HttpContext.Request.Scheme);
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult GeneratePayment(Guid id)
        {
            GeneratePaymentViewModel model = new GeneratePaymentViewModel(_db, id, User.Identity.Name);
            //if (!string.IsNullOrWhiteSpace(model.Transaction.AllowedEmail))
            //{
            //    if (User.Identity.IsAuthenticated && model.Transaction.AllowedEmail != User.Identity.Name)
            //    {
            //        return RedirectToAction("ErrorCreatingTransaction");
            //    }
            //}
            if (model.Transaction.ExpiredDate.HasValue)
            {
                if (model.Transaction.ExpiredDate.Value < DateTime.UtcNow)
                {
                    return RedirectToAction("ErrorCreatingTransaction");
                }
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult GeneratePayment(GeneratePaymentFormModel Form)
        {
            GeneratePaymentViewModel model = new GeneratePaymentViewModel(_db, Form.TransactionId, User.Identity.Name);
            model.Form = Form;
            if (!ModelState.IsValid)
            {

                return View(model);
            }
            Wallet walletOrigin = _db.Wallets.SingleOrDefault(x => x.Id == Form.WalletId.Value);
            Wallet walletDestination = _db.Wallets.SingleOrDefault(x => x.Id == model.Transaction.WalletId.Value);

            if (walletOrigin.UserId != User.Identity.Name)
            {
                return View("Unauthorized");
            }

            Guid idempotencyKey = Guid.NewGuid();
            var client = new RestClient(_configuration["CircleAPIBaseUrl"]);
            var request = new RestRequest("/transfers", Method.Post);
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authorization", "Bearer " + Environment.GetEnvironmentVariable("CircleAPIKey"));

            request.AddParameter("application/json", "{\"source\":{\"type\":\"wallet\",\"id\":\"" + walletOrigin.Account + "\"},\"amount\":{\"currency\":\"" + model.Transaction.Currency + "\",\"amount\":\"" + model.Transaction.Amount.ToString().Replace(",",".") + "\"},\"destination\":{\"type\":\"wallet\",\"id\":\"" + walletDestination.Account + "\"},\"idempotencyKey\":\"" + idempotencyKey + "\"}", ParameterType.RequestBody);
            RestResponse<TransferDTO> response = client.Execute<TransferDTO>(request);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Details", new { id = response.Data.data.id });
            }
            else
            {
                ModelState.AddModelError("Form.WalletId", "Error in send the payments. Please, try again later.");
                return View(model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult CreateTransaction(CreateTransactionFormModel Form)
        {
            if (!ModelState.IsValid)
            {
                return View("ErrorCreatingTransaction", Form.FailedURL);
            }
            Transaction transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Amount = Form.Amount.Value,
                CreatedAt = DateTime.UtcNow,
                WalletId = Form.WalletId,
                Subtitle = Form.Subtitle,
                Title = Form.Title,
                TransactionCode = Form.TransactionCode,
                FailedURL = Form.FailedURL,
                ReturnURL = Form.ReturnURL,
                SuccessURL = Form.SuccessURL,
                AllowedEmail = Form.AllowedEmail,
                AllowedWalletId = Form.AllowedWalletId,
                ExpiredDate = Form.ExpiredDate,
                Currency = Form.Currency
            };
            _db.Transactions.Add(transaction);
            _db.SaveChanges();
            return RedirectToAction("GeneratePayment", new { id = transaction.Id });
        }

        public IActionResult CreateTransactionDemo()
        {
            PaymentDemoViewModel model = new PaymentDemoViewModel(_db);
            model.Link = Url.Action("CreateTransaction", "Payment", null, HttpContext.Request.Scheme);
            return View(model);
        }

        public IActionResult SendCross()
        {
            SendPaymentViewModel model = new SendPaymentViewModel(_db, User.Identity.Name);
            model.Destinations = _db.Receivers.Where(x => x.UserId == User.Identity.Name && x.AddressBookRecipientId != "").OrderBy(x => x.Name).Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
            return View(model);
        }

        [HttpPost]
        public IActionResult SendCross(SendPaymentFormModel Form)
        {
            SendPaymentViewModel model = new SendPaymentViewModel(_db, User.Identity.Name);
            model.Form = Form;
            if(string.IsNullOrEmpty(Form.DestinationCurrency))
            {
                ModelState.AddModelError("Form.DestinationCurrency", "The destination currency is required.");
            }
            if (!ModelState.IsValid)
            {
                model.Destinations = _db.Receivers.Where(x => x.UserId == User.Identity.Name && x.AddressBookRecipientId != "").OrderBy(x => x.Name).Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
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
            var request = new RestRequest("/payouts", Method.Post);
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authorization", "Bearer " + Environment.GetEnvironmentVariable("CircleAPIKey"));

            request.AddParameter("application/json", "{\"source\":{\"type\":\"wallet\",\"id\":\"" + walletOrigin.Account + "\"},\"destination\":{\"type\":\"address_book\",\"id\":\""+ walletDestination.AddressBookRecipientId+ "\"},\"amount\":{\"currency\":\"" + Form.OriginCurrency + "\",\"amount\":\"" + Form.Amount + "\"},\"toAmount\":{\"currency\":\"" + Form.DestinationCurrency + "\"},\"idempotencyKey\":\"" + idempotencyKey + "\"}", ParameterType.RequestBody);
            RestResponse<PayoutDTO> response = client.Execute<PayoutDTO>(request);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("DetailCross", new { id = response.Data.data.id });
            }
            else
            {
                model.Destinations = _db.Receivers.Where(x => x.UserId == User.Identity.Name && x.AddressBookRecipientId != "").OrderBy(x => x.Name).Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
                ModelState.AddModelError("Form.OriginWalletId", "Error in send the payment, check the currencies. Please, try again later.");
                return View(model);
            }

        }

        public ActionResult DetailCross(string id)
        {
            Guid idempotencyKey = Guid.NewGuid();
            var client = new RestClient(_configuration["CircleAPIBaseUrl"]);
            var request = new RestRequest("/payouts/" + id, Method.Get);
            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", "Bearer " + Environment.GetEnvironmentVariable("CircleAPIKey"));
            RestResponse<PayoutDTO> response = client.Execute<PayoutDTO>(request);
            DetailsCrossPaymentViewModel model = new DetailsCrossPaymentViewModel(_db, response);
            return View(model);
        }


    }
}
