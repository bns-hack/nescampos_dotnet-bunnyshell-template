using System.ComponentModel.DataAnnotations;

namespace PayWave.Models.WalletModels
{
    public class BlockchainAccountsFormModel
    {
        [Required]
        public long Id { get; set; }

        [Required]
        public string? Chain { get; set; }

        [Required]
        public string? Currency { get; set; }
    }
}
