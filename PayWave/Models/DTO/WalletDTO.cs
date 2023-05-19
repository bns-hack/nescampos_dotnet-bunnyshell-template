namespace PayWave.Models.DTO
{
    public class WalletDTO
    {
        public WalletDataDTO data { get; set; }
    }

    public class WalletDataDTO
    {
        public string walletId { get; set; }
        public string entityId { get; set; }
        public WalletBalanceDTO[]? balances { get; set; }
    }

    public class WalletBalanceDTO
    {
        public string amount { get; set; }
        public string currency { get; set; }
    }
}
