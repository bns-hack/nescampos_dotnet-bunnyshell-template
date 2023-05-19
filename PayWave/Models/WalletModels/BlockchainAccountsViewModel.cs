using Microsoft.AspNetCore.Mvc.Rendering;
using PayWave.Data;
using PayWave.Models.DTO;

namespace PayWave.Models.WalletModels
{
    public class BlockchainAccountsViewModel
    {
        public Wallet Wallet { get; set; }
        public BlockchainAccountDataDTO[]? Accounts { get; set; }
        public BlockchainAccountsFormModel Form { get; set; }
        public List<SelectListItem> Currencies { get; set; }
        public List<SelectListItem> Chains { get; set; }

        public BlockchainAccountsViewModel(ApplicationDbContext db, long id)
        {
            Wallet = db.Wallets.SingleOrDefault(x => x.Id == id);
            Form = new BlockchainAccountsFormModel { Id = id };
            Currencies = new List<SelectListItem>();
            Currencies.Add(new SelectListItem { Text = "Bitcoin", Value = "BTC" });
            Currencies.Add(new SelectListItem { Text = "Ether", Value = "ETH" });
            Currencies.Add(new SelectListItem { Text = "Euro", Value = "EUR" });
            Currencies.Add(new SelectListItem { Text = "US Dollar", Value="USD", Selected=true });

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

            //LastFivePayments = db.Payments.Where(x => x.Confirmed == true && x.WalletId == id).OrderByDescending(x => x.CreatedAt).Take(5);
        }
    }
}
