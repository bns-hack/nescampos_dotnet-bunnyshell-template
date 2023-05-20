using System.ComponentModel.DataAnnotations;

namespace PayWave.Models.PaymentModels
{
    public class GeneratePaymentFormModel
    {
        [Required]
        public Guid TransactionId { get; set; }

        [Required(ErrorMessage = "Select your wallet for payment")]
        public long? WalletId { get; set; }
    }
}
