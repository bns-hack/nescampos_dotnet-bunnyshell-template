using System.ComponentModel.DataAnnotations;

namespace PayWave.Models.PaymentModels
{
    public class SendPersonalPaymentFormModel
    {
        [Required]
        public long? OriginWalletId { get; set; }

        [Required]
        public long? DestinationWalletId { get; set; }

        [Required]
        public decimal? Amount { get; set; }

        [Required]
        public string? Currency { get; set; }
    }
}
