using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformObjects.MiscModels
{
    public class PasswordResetModel
    {
        public int TokenValue { get; set; }
        public int Id { get; set; }
        public string NewPassword { get; set; } = string.Empty;
    }
}
