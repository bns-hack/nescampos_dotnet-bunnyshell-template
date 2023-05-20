using PayWave.Data;

namespace PayWave.Models.PaymentModels
{
    public class NewGenerateTransactionLinkViewModel
    {
        public string Link { get; set; }

        public NewGenerateTransactionLinkViewModel(ApplicationDbContext db, string name, Guid id)
        {

        }
    }
}
