using Microsoft.AspNetCore.Mvc.Rendering;
using PayWave.Data;

namespace PayWave.Models.PaymentModels
{
    public class GeneratePaymentViewModel
    {
        public Transaction Transaction { get; set; }
        public GeneratePaymentFormModel Form { get; set; }
        public IEnumerable<SelectListItem> Wallets { get; set; }
        public Wallet DestinationWallet { get; set; }

        public GeneratePaymentViewModel(ApplicationDbContext db, Guid id, string name)
        {
            Transaction = db.Transactions.SingleOrDefault(x => x.Id == id);
            DestinationWallet = db.Wallets.SingleOrDefault(x => x.Id == Transaction.WalletId);

            Wallets = db.Wallets.Where(x => x.UserId == name).OrderBy(x => x.Name)
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });

            Form = new GeneratePaymentFormModel { TransactionId = id };


        }
    }
}
