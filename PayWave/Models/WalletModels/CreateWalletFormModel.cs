using System.ComponentModel.DataAnnotations;

namespace PayWave.Models.WalletModels
{
    public class CreateWalletFormModel
    {

        [Required]
        public string? Name { get; set; }

        [Required]
        [RegularExpression("^[a-z0-9]*$", ErrorMessage = "Alias must be only words (lowercase) and numbers.")]
        public string? Alias { get; set; }
    }
}
