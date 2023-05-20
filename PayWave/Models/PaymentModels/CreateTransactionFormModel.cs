using System.ComponentModel.DataAnnotations;

namespace PayWave.Models.PaymentModels
{
    public class CreateTransactionFormModel
    {
        [Required]
        public decimal? Amount { get; set; }

        [Required]
        public long? WalletId { get; set; }

        public string? TransactionCode { get; set; }

        public string? Title { get; set; }

        public string? Subtitle { get; set; }

        public string? ReturnURL { get; set; }
        public string? SuccessURL { get; set; }
        public string? FailedURL { get; set; }

        public DateTime? ExpiredDate { get; set; }
        public string? AllowedEmail { get; set; }
        public string? AllowedWalletId { get; set; }

        [Required]
        public string? Currency { get; set; }
    }
}
