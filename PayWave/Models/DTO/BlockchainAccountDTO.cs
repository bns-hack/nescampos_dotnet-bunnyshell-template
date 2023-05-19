namespace PayWave.Models.DTO
{
    public class BlockchainAccountDTO
    {
        public BlockchainAccountDataDTO[]? data { get; set; }
    }

    public class BlockchainAccountDataDTO
    {
        public string? address { get; set; }
        public string? addressTag { get; set; }
        public string? currency { get; set; }
        public string? chain { get; set; }
    }
}
