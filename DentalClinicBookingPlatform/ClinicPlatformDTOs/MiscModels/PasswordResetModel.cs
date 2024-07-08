using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformObjects.MiscModels
{
    public class PasswordResetModel
    {
        public string TokenValue { get; set; } = null!;
        public int? Id { get; set; }
        public string NewPassword { get; set; } = string.Empty;
    }
}
