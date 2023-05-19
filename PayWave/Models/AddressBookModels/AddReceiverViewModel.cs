using Microsoft.AspNetCore.Mvc.Rendering;
using PayWave.Data;

namespace PayWave.Models.AddressBookModels
{
    public class AddReceiverViewModel
    {
        public AddReceiverFormModel Form { get; set; }
        public List<SelectListItem> Types { get; set; }
        public List<SelectListItem> Chains { get; set; }

        public AddReceiverViewModel(ApplicationDbContext db)
        {
            Form = new AddReceiverFormModel { };
            Types = new List<SelectListItem>();
            Types.Add(new SelectListItem { Text = "Blockchain", Value = "blockchain" });
            Types.Add(new SelectListItem { Text = "Wallet", Value = "wallet" });

            Chains = new List<SelectListItem>();
            Chains.Add(new SelectListItem { Text = "Algorand", Value = "ALGO" });
            Chains.Add(new SelectListItem { Text = "Avalanche", Value = "AVAX" });
            Chains.Add(new SelectListItem { Text = "Bitcoin", Value = "BTC" });
            Chains.Add(new SelectListItem { Text = "Ethereum", Value = "ETH" });
            Chains.Add(new SelectListItem { Text = "Flow", Value = "FLOW" });
            Chains.Add(new SelectListItem { Text = "Hedera", Value = "HBAR" });
            Chains.Add(new SelectListItem { Text = "Polygon", Value = "MATIC" });
            Chains.Add(new SelectListItem { Text = "Solana", Value = "SOL" });
            Chains.Add(new SelectListItem { Text = "Stellar", Value = "XLM" });
            Chains.Add(new SelectListItem { Text = "Tron", Value = "TRX" });

        }
    }
}
