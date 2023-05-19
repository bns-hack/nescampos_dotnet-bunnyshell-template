using Microsoft.AspNetCore.Mvc.Rendering;
using PayWave.Data;

namespace PayWave.Models.PaymentModels
{
    public class SendPersonalPaymentViewModel
    {
        public SendPersonalPaymentFormModel Form { get; set; }
        public IEnumerable<Wallet> Wallets { get; set; }
        public IEnumerable<SelectListItem> WalletSelectList { get; set; }
        public List<SelectListItem> Currencies { get; set; }

        public SendPersonalPaymentViewModel(ApplicationDbContext db, string? name)
        {
            Wallets = db.Wallets.Where(x => x.UserId == name).OrderBy(x => x.Name);
            WalletSelectList = Wallets
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
            Form = new SendPersonalPaymentFormModel {  };
            Currencies = new List<SelectListItem>();
            Currencies.Add(new SelectListItem { Text = "Bitcoin", Value = "BTC" });
            Currencies.Add(new SelectListItem { Text = "Ether", Value = "ETH" });
            Currencies.Add(new SelectListItem { Text = "Euro", Value = "EUR" });
            Currencies.Add(new SelectListItem { Text = "US Dollar", Value = "USD", Selected = true });
        }
    }
}
