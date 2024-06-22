using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.UserModels
{
    public class PasswordResetModel
    {
        public int TokenValue { get; set; }
        public string NewPassword { get; set; } = string.Empty;
    }
}
