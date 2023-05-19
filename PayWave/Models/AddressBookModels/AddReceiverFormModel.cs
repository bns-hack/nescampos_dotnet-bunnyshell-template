using System.ComponentModel.DataAnnotations;

namespace PayWave.Models.AddressBookModels
{
    public class AddReceiverFormModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Type { get; set; }

        public string? WalletId { get; set; }

        public string? BlockchainAddress { get; set; }
        public string? BlockchainAddressTag { get; set; }
        public string? Chain { get; set; }

    }
}
