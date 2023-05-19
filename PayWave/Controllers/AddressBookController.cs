using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayWave.Data;
using PayWave.Models.AddressBookModels;

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
            if(!string.IsNullOrEmpty(Form.Type))
            {
                if(Form.Type == "blockchain")
                {
                    if(string.IsNullOrEmpty(Form.Chain) || string.IsNullOrEmpty(Form.BlockchainAddress))
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
                        if(!existWallet)
                        {
                            ModelState.AddModelError("Form.WalletId", "The wallet Id/Alias does not exist.");
                        }
                    }
                }
            }
            if (!ModelState.IsValid)
            {

                model.Form = Form;
                return View(model);
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
