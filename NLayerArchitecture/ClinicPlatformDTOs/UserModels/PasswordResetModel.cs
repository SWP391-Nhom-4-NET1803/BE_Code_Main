using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.UserModels
{
    public class PasswordResetModel
    {
        public string Email { get; set; } = string.Empty;
        public string? PasswordReset { get; set; } = string.Empty;
    }
}
