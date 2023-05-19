using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayWave.Data;
using PayWave.Models.AddressBookModels;
using PayWave.Models.DTO;
using RestSharp;

namespace PayWave.Controllers
{
    [Authorize]
    public class AddressBookController : Controller
    {
        private IConfiguration _configuration;
        private ApplicationDbContext _db { get; set; }
        public AddressBookController(IConfiguration configuration, ApplicationDbContext db)
        {
            _configuration = configuration;
            _db = db;
        }
        public IActionResult Index()
        {
            IndexAddressBookViewModel model = new IndexAddressBookViewModel(_db, User.Identity.Name);
            return View(model);
        }

        public IActionResult Add()
        {
            AddReceiverViewModel model = new AddReceiverViewModel(_db);
            return View(model);
        }

        [HttpPost]
        public IActionResult Add(AddReceiverFormModel Form)
        {
            AddReceiverViewModel model = new AddReceiverViewModel(_db);
            model.Form = Form;
            if (!string.IsNullOrEmpty(Form.Type))
            {
                if (Form.Type == "blockchain")
                {
                    if (string.IsNullOrEmpty(Form.Chain) || string.IsNullOrEmpty(Form.BlockchainAddress))
                    {
                        ModelState.AddModelError("Form.BlockchainAddress", "You need to select a chain and enter the blockchain address.");
                        ModelState.AddModelError("Form.Chain", "You need to select a chain and enter the blockchain address.");
                    }
                }
                if (Form.Type == "wallet")
                {
                    if (string.IsNullOrEmpty(Form.WalletId))
                    {
                        ModelState.AddModelError("Form.WalletId", "You need to enter the wallet id or alias.");
                    }
                    else
                    {
                        bool existWallet = _db.Wallets.Any(x => x.Alias == Form.WalletId || x.Account == Form.WalletId);
                        if (!existWallet)
                        {
                            ModelState.AddModelError("Form.WalletId", "The wallet Id/Alias does not exist.");
                        }
                    }
                }
            }
            if (!ModelState.IsValid)
            {

                
                return View(model);
            }
            if (Form.Type == "blockchain")
            {
                var client = new RestClient(_configuration["CircleAPIBaseUrl"]);
                var request = new RestRequest("/addressBook/recipients", Method.Post);
                request.AddHeader("accept", "application/json");
                request.AddHeader("content-type", "application/json");
                request.AddHeader("authorization", "Bearer " + _configuration["CircleAPIKey"]);
                string tag = Form.BlockchainAddressTag != null? ",\"addressTag\":\""+ Form.BlockchainAddressTag + "\"" : "";
                request.AddParameter("application/json", "{\"chain\":\""+Form.Chain+"\",\"metadata\":{\"nickname\":\""+Form.Name+"\"},\"idempotencyKey\":\""+Guid.NewGuid()+"\",\"address\":\""+Form.BlockchainAddress+ "\""+ tag + "}", ParameterType.RequestBody);
                RestResponse<AddressBookRecipientDTO> response = client.Execute<AddressBookRecipientDTO>(request);
                if(response.IsSuccessStatusCode)
                {
                    Receiver receiver2 = new Receiver
                    {
                        WalletId = Form.WalletId,
                        BlockchainAddress = Form.BlockchainAddress,
                        BlockchainAddressTag = Form.BlockchainAddressTag,
                        Chain = Form.Chain,
                        CreatedAt = DateTime.Now,
                        Name = Form.Name,
                        Type = Form.Type,
                        UserId = User.Identity.Name,
                        AddressBookRecipientId = response.Data.data.id
                    };
                    _db.Receivers.Add(receiver2);
                    _db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("Form.BlockchainAddress", "Error creating this recipient");
                    return View(model);
                }
            }
            Receiver receiver = new Receiver
            {
                WalletId = Form.WalletId,
                BlockchainAddress = Form.BlockchainAddress,
                BlockchainAddressTag = Form.BlockchainAddressTag,
                Chain = Form.Chain,
                CreatedAt = DateTime.Now,
                Name = Form.Name,
                Type = Form.Type,
                UserId = User.Identity.Name
            };
            _db.Receivers.Add(receiver);
            _db.SaveChanges();
            return RedirectToAction("Index");

        }
    }
}
