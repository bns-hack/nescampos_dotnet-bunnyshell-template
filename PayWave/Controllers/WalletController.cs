using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayWave.Data;
using PayWave.Models.DTO;
using PayWave.Models.WalletModels;
using RestSharp;

namespace PayWave.Controllers
{
    [Authorize]
    public class WalletController : Controller
    {
        private IConfiguration _configuration;
        private ApplicationDbContext _db { get; set; }
        public WalletController(IConfiguration configuration, ApplicationDbContext db)
        {
            _configuration = configuration;
            _db = db;
        }
        public IActionResult Index()
        {
            IndexWalletViewModel model = new IndexWalletViewModel(_db, User.Identity.Name);
            return View(model);
        }

        public IActionResult Create()
        {
            CreateWalletViewModel model = new CreateWalletViewModel(_db);
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateWalletFormModel Form)
        {
            CreateWalletViewModel model = new CreateWalletViewModel(_db);
            model.Form = Form;
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (!string.IsNullOrWhiteSpace(Form.Alias))
            {
                string alias = Form.Alias.ToLower();
                bool existAlias = _db.Wallets.Any(x => x.Alias == alias);
                if (existAlias)
                {
                    ModelState.AddModelError("Form.Alias", "The alias is already occupied by another account");
                }
            }

            Guid idempotencyKey = Guid.NewGuid();
            var client = new RestClient(_configuration["CircleAPIBaseUrl"]);
            var request = new RestRequest("/wallets", Method.Post);
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authorization", "Bearer "+ Environment.GetEnvironmentVariable("CircleAPIKey"));
            request.AddParameter("application/json", "{\"idempotencyKey\":\""+ idempotencyKey + "\",\"description\":\"Wallet with alias "+Form.Alias+"\"}", ParameterType.RequestBody);
            RestResponse<WalletDTO> response = client.Execute<WalletDTO>(request);

            if(response.IsSuccessStatusCode)
            {
                Wallet newWallet = new Wallet
                {
                    Alias = Form.Alias.ToLower(),
                    CreatedAt = DateTime.UtcNow,
                    Name = Form.Name,
                    UserId = User.Identity.Name,
                    Account = response.Data.data.walletId ,
                    EntityId = response.Data.data.entityId
                };
                _db.Wallets.Add(newWallet);
                _db.SaveChanges();
                return RedirectToAction("View", new { id = newWallet.Id });
            }
            else
            {
                ModelState.AddModelError("Form.Alias", "Error creating a new wallet. Please, try again.");
                return View(model);
            }
        }

        public IActionResult View(long id)
        {
            ViewWalletViewModel model = new ViewWalletViewModel(_db, id);
            if (model.Wallet.UserId != User.Identity.Name)
            {
                return View("Unauthorized");
            }
            var client = new RestClient(_configuration["CircleAPIBaseUrl"]);
            var request = new RestRequest("/wallets/"+model.Wallet.Account, Method.Get);
            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", "Bearer " + Environment.GetEnvironmentVariable("CircleAPIKey"));
            RestResponse<WalletDTO> response = client.Execute<WalletDTO>(request);
            if (response.IsSuccessStatusCode)
            {
                model.Balances = response.Data.data.balances;
            }
            return View(model);
        }

        public IActionResult BlockchainAccounts(long id)
        {
            BlockchainAccountsViewModel model = new BlockchainAccountsViewModel(_db, id);
            if (model.Wallet.UserId != User.Identity.Name)
            {
                return View("Unauthorized");
            }
            var client = new RestClient(_configuration["CircleAPIBaseUrl"]);
            var request = new RestRequest("/wallets/" + model.Wallet.Account+"/addresses", Method.Get);
            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", "Bearer " + Environment.GetEnvironmentVariable("CircleAPIKey"));
            RestResponse<BlockchainAccountDTO> response = client.Execute<BlockchainAccountDTO>(request);
            if (response.IsSuccessStatusCode)
            {
                model.Accounts = response.Data.data;
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult BlockchainAccounts(BlockchainAccountsFormModel Form)
        {
            BlockchainAccountsViewModel model = new BlockchainAccountsViewModel(_db, Form.Id);
            if (model.Wallet.UserId != User.Identity.Name)
            {
                return View("Unauthorized");
            }
            model.Form = Form;
            if (!ModelState.IsValid)
            {
                var client2 = new RestClient(_configuration["CircleAPIBaseUrl"]);
                var request2 = new RestRequest("/wallets/" + model.Wallet.Account + "/addresses", Method.Get);
                request2.AddHeader("accept", "application/json");
                request2.AddHeader("authorization", "Bearer " + Environment.GetEnvironmentVariable("CircleAPIKey"));
                RestResponse<BlockchainAccountDTO> response2 = client2.Execute<BlockchainAccountDTO>(request2);
                if (response2.IsSuccessStatusCode)
                {
                    model.Accounts = response2.Data.data;
                }
                return View(model);
            }
            Guid idempotencyKey = Guid.NewGuid();

            var client = new RestClient(_configuration["CircleAPIBaseUrl"]);
            var request = new RestRequest("/wallets/" + model.Wallet.Account + "/addresses", Method.Post);
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authorization", "Bearer " + Environment.GetEnvironmentVariable("CircleAPIKey"));
            request.AddParameter("application/json", "{\"currency\":\""+Form.Currency+ "\",\"chain\":\"" + Form.Chain + "\",\"idempotencyKey\":\"" + idempotencyKey + "\"}", ParameterType.RequestBody);
            RestResponse response = client.Execute(request);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("BlockchainAccounts", new { id = Form.Id });
            }
            else
            {
                var client2 = new RestClient(_configuration["CircleAPIBaseUrl"]);
                var request2 = new RestRequest("/wallets/" + model.Wallet.Account + "/addresses", Method.Get);
                request2.AddHeader("accept", "application/json");
                request2.AddHeader("authorization", "Bearer " + Environment.GetEnvironmentVariable("CircleAPIKey"));
                RestResponse<BlockchainAccountDTO> response2 = client2.Execute<BlockchainAccountDTO>(request2);
                if (response2.IsSuccessStatusCode)
                {
                    model.Accounts = response2.Data.data;
                }
                ModelState.AddModelError("Form.Currency", "Error creating a new wallet. Please, try again or verify if an existing account can be used (in the same chain).");
                return View(model);
            }
        }


    }
}
