using Microsoft.AspNetCore.Mvc.Rendering;
using PayWave.Data;
using PayWave.Models.DTO;

namespace PayWave.Models.PaymentModels
{
    public class IndexPaymentViewModel
    {
        public IEnumerable<SelectListItem> Wallets { get; set; }
        public TransferDataDTO[] TransferList { get; set; }

        public IndexPaymentViewModel(ApplicationDbContext db, string username)
        {
            Wallets = db.Wallets.Where(x => x.UserId == username).OrderBy(x => x.Name).Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
                
        }

        
    }
}
