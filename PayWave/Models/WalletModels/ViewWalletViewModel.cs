using PayWave.Data;

namespace PayWave.Models.WalletModels
{
    public class ViewWalletViewModel
    {
        public Wallet Wallet { get; set; }
        //public IEnumerable<Payment> LastFivePayments { get; set; }

        public ViewWalletViewModel(ApplicationDbContext db, long id)
        {
            Wallet = db.Wallets.SingleOrDefault(x => x.Id == id);
            //LastFivePayments = db.Payments.Where(x => x.Confirmed == true && x.WalletId == id).OrderByDescending(x => x.CreatedAt).Take(5);
        }
    }
}
