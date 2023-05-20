using PayWave.Data;
using PayWave.Models.DTO;
using RestSharp;

namespace PayWave.Models.PaymentModels
{
    public class DetailsCrossPaymentViewModel
    {
        public Wallet? OriginWallet { get; set; }
        public Receiver? DestinationWalletWallet { get; set; }
        public RestResponse<PayoutDTO> response { get; set; }

        public DetailsCrossPaymentViewModel(ApplicationDbContext db, RestResponse<PayoutDTO> response)
        {
            this.response = response;
            OriginWallet = db.Wallets.SingleOrDefault(x => x.Account == response.Data.data.sourceWalletId);
            DestinationWalletWallet = db.Receivers.SingleOrDefault(x => x.AddressBookRecipientId == response.Data.data.destination.id);
        }

    }
}
