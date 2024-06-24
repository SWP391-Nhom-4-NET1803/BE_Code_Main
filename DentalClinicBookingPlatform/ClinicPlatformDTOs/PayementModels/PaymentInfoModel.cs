using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformObjects.PayementModels
{
    public class PaymentInfoModel
    {
        public int Id { get; set; }
        public string TransactId { get; set; } = null!;
        public int Amount { get; set; }
        public string Title { get; set; } = null!;
        public DateOnly Expiration { get; set; }
        public DateTime CreatedTime { get; set; }
        public bool Status { get; set; }
        public int type { get; set; } = 1;
        public string provider { get; set; } = null!;
        public Guid appointmentId { get; set; }
        public int UserId { get; set; }

    }
}
