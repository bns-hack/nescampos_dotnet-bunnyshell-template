using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayWave.Data
{
    public class Receiver
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Type { get; set; }

        public string? WalletId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
        public string? BlockchainAddress { get; set; }
        public string? BlockchainAddressTag { get; set; }
        public string? Chain { get; set; }

        [Required]
        public string UserId { get; set; }

        public string AddressBookRecipientId { get; set; }

    }
}
