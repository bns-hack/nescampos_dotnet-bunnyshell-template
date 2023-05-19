using PayWave.Data;
using PayWave.Models.DTO;
using RestSharp;

namespace PayWave.Models.PaymentModels
{
    public class DetailsPaymentViewModel
    {
        public Wallet? OriginWallet { get; set; }
        public Wallet? DestinationWalletWallet { get; set; }
        public RestResponse<TransferDTO> response { get; set; }

        public DetailsPaymentViewModel(ApplicationDbContext db, RestResponse<TransferDTO> response)
        {
            this.response = response;
            if(response.Data.data.source.type == "wallet")
            {
                OriginWallet = db.Wallets.SingleOrDefault(x => x.Account == response.Data.data.source.id);
            }
            if (response.Data.data.destination.type == "wallet")
            {
                DestinationWalletWallet = db.Wallets.SingleOrDefault(x => x.Account == response.Data.data.destination.id);
            }
        }

        
    }
}
