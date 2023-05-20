using Microsoft.AspNetCore.Mvc.Rendering;
using PayWave.Data;

namespace PayWave.Models.PaymentModels
{
    public class GenerateTransactionLinkViewModel
    {
        public CreateTransactionFormModel Form { get; set; }
        public IEnumerable<SelectListItem> Wallets { get; set; }
        public List<SelectListItem> Currencies { get; set; }
        public bool WorkingWithRealMoney { get; set; }

        public GenerateTransactionLinkViewModel(ApplicationDbContext db, string name)
        {
            Wallets = db.Wallets.Where(x => x.UserId == name).OrderBy(x => x.Name)
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
            Currencies = new List<SelectListItem>();
            Currencies.Add(new SelectListItem { Text = "Bitcoin", Value = "BTC" });
            Currencies.Add(new SelectListItem { Text = "Ether", Value = "ETH" });
            Currencies.Add(new SelectListItem { Text = "Euro", Value = "EUR" });
            Currencies.Add(new SelectListItem { Text = "US Dollar", Value = "USD", Selected = true });
        }
    }
}
