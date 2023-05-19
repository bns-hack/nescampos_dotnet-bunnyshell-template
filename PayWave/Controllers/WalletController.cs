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
            request.AddHeader("authorization", "Bearer "+_configuration["CircleAPIKey"]);
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
            return View(model);
        }
    }
}
