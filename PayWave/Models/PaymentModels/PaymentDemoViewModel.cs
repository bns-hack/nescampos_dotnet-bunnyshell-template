using Microsoft.AspNetCore.Mvc.Rendering;
using PayWave.Data;

namespace PayWave.Models.PaymentModels
{
    public class PaymentDemoViewModel
    {
        public CreateTransactionFormModel Form { get; set; }
        public List<SelectListItem> Currencies { get; set; }
        public string? Link { get; internal set; }

        public PaymentDemoViewModel(ApplicationDbContext db)
        {
            Currencies = new List<SelectListItem>();
            Currencies.Add(new SelectListItem { Text = "BTC", Value = "BTC" });
            Currencies.Add(new SelectListItem { Text = "ETH", Value = "ETH" });
            Currencies.Add(new SelectListItem { Text = "EUR", Value = "EUR" });
            Currencies.Add(new SelectListItem { Text = "USD", Value = "USD", Selected = true });
        }
    }
}
