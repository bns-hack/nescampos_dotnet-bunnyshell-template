namespace PayWave.Models.DTO
{
    public class PayoutDTO
    {
        public PayoutDataDTO data { get; set; }
    }

    public class PayoutDataDTO
    {
        public string id { get; set; }
        public string sourceWalletId { get; set; }
        public PayoutDataDestinationDTO destination { get; set; }
        public PayoutDataAmountDTO amount { get; set; }
        public PayoutDataAmountDTO toAmount { get; set; }
        public PayoutDataAmountDTO fees { get; set; }
        public PayoutDataAmountDTO networkFees { get; set; }
        public string? status { get; set; }
        public string? errorCode { get; set; }

    }

    public class PayoutDataDestinationDTO
    {
        public string id { get; set; }
        public string type { get; set; }
    }

    public class PayoutDataAmountDTO
    {
        public string amount { get; set; }
        public string currency { get; set; }
    }
}
