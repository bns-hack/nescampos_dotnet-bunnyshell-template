using PayWave.Data;

namespace PayWave.Models.AddressBookModels
{
    public class IndexAddressBookViewModel
    {
        public IEnumerable<Receiver> Receivers { get; set; }
        public IndexAddressBookViewModel(ApplicationDbContext _db, string username)
        {
            Receivers = _db.Receivers.Where(x => x.UserId == username);
        }
    }
}
