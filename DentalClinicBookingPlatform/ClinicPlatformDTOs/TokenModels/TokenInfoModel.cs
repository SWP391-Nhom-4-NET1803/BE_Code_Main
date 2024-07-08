using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformObjects.TokenModels
{
    public class TokenInfoModel
    {
        public Guid Id { get; set; }
        public string Value { get; set; } = null!;
        public DateTime Creation { get; set; }
        public DateTime Expiration { get; set; }
        public string Reason { get; set; } = null!;
        public bool Used { get; set; }
        public int UserId { get; set; }
    }
}
