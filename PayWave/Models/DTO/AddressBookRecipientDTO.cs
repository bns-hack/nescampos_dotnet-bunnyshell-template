namespace PayWave.Models.DTO
{
    public class AddressBookRecipientDTO
    {
        public AddressBookRecipientDataDTO data { get; set; }
    }

    public class AddressBookRecipientDataDTO
    {
        public string id { get; set; }
        public string status { get; set; }
    }
}
