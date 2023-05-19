using System.ComponentModel.DataAnnotations;

namespace PayWave.Models.PaymentModels
{
    public class SendPaymentFormModel
    {
        [Required]
        public long? OriginWalletId { get; set; }

        [Required]
        public long? RecipientId { get; set; }

        [Required]
        public decimal? Amount { get; set; }

        [Required]
        public string? OriginCurrency { get; set; }

        //[Required]
        public string? DestinationCurrency { get; set; }
    }
}
