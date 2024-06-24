using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.UserModels
{
    public class UserRegistrationModel
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Email { get; set; } = null!;

        // Data for the clinic staff
        public bool ClinicOwner { get; set; } = false;
        public int? Clinic { get; set; } = null;
    }
}
