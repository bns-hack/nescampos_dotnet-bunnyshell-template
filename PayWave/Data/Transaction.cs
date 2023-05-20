using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayWave.Data
{
    public class Transaction
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        public string? TransactionCode { get; set; }

        public string? Title { get; set; }

        public string? Subtitle { get; set; }

        public string? ReturnURL { get; set; }
        public string? SuccessURL { get; set; }
        public string? FailedURL { get; set; }


        [Required]
        public decimal? Amount { get; set; }


        [Required]
        public long? WalletId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? ExpiredDate { get; set; }
        public string? AllowedEmail { get; set; }
        public string? AllowedWalletId { get; set; }
        public string? Currency { get; set; }
    }
}
