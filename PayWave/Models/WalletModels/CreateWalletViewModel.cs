using PayWave.Data;

namespace PayWave.Models.WalletModels
{
    public class CreateWalletViewModel
    {
        public CreateWalletFormModel Form { get; set; }
        public CreateWalletViewModel(ApplicationDbContext db)
        {
        }
    }
}
