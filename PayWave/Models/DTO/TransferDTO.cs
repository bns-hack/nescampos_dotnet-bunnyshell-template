namespace PayWave.Models.DTO
{
    public class TransferDTO
    {
        public TransferDataDTO data { get; set; }
    }

    public class TransferDataDTO
    {
        public string? id { get; set; }
        public TransferDataSourceDTO source { get; set; }
        public TransferDataDestinationDTO destination { get; set; }
        public TransferDataAmountDTO amount { get; set; }
        public string? transactionHash { get; set; }
        public string? status { get; set; }
        public string? errorCode { get; set; }
        public DateTime? createDate { get; set; }
    }

    public class TransferDataSourceDTO
    {
        public string? id { get; set; }
        public string? type { get; set; }
        public string? chain { get; set; }
    }

    public class TransferDataDestinationDTO
    {
        public string? id { get; set; }
        public string? address { get; set; }
        public string? addressTag { get; set; }
        public string? type { get; set; }
        public string? chain { get; set; }
    }

    public class TransferDataAmountDTO
    {
        public string? currency { get; set; }
        public string? amount { get; set; }
    }
}
