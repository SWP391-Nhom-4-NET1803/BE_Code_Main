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
        public decimal Amount { get; set; }
        public string Info { get; set; } = null!;
        public DateTime Expiration { get; set; }
        public DateTime CreatedTime { get; set; }
        public string Status { get; set; } = null!;
        public string Provider { get; set; } = null!;
        public Guid AppointmentId { get; set; }
    }
}
