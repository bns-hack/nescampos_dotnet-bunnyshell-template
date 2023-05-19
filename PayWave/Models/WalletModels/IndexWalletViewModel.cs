using PayWave.Data;

namespace PayWave.Models.WalletModels
{
    public class IndexWalletViewModel
    {
        public IEnumerable<Wallet> Wallets { get; set; }
        public bool WorkingWithRealMoney { get; set; }
        public IndexWalletViewModel(ApplicationDbContext db, string username)
        {
            Wallets = db.Wallets.Where(x => x.UserId == username).OrderBy(x => x.Name);
        }
    }
}
