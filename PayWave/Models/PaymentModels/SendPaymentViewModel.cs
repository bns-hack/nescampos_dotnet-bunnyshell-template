using Microsoft.AspNetCore.Mvc.Rendering;
using PayWave.Data;

namespace PayWave.Models.PaymentModels
{
    public class SendPaymentViewModel
    {
        public SendPaymentFormModel Form { get; set; }
        public IEnumerable<Wallet> Wallets { get; set; }
        public IEnumerable<SelectListItem> WalletSelectList { get; set; }
        public List<SelectListItem> Currencies { get; set; }
        public IEnumerable<SelectListItem> Destinations { get; set; }

        public SendPaymentViewModel(ApplicationDbContext db, string? name)
        {
            Wallets = db.Wallets.Where(x => x.UserId == name).OrderBy(x => x.Name);
            WalletSelectList = Wallets
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
            Destinations = db.Receivers.Where(x => x.UserId == name).OrderBy(x => x.Name).Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
            Form = new SendPaymentFormModel { };
            Currencies = new List<SelectListItem>();
            Currencies.Add(new SelectListItem { Text = "Bitcoin", Value = "BTC" });
            Currencies.Add(new SelectListItem { Text = "Ether", Value = "ETH" });
            Currencies.Add(new SelectListItem { Text = "Euro", Value = "EUR" });
            Currencies.Add(new SelectListItem { Text = "US Dollar", Value = "USD", Selected = true });
        }
    }
}
