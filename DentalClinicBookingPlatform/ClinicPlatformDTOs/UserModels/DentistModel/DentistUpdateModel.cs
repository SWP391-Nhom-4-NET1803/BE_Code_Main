using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformObjects.UserModels.DentistModel
{
    public class DentistUpdateModel
    {
        public string Username { get; set; } = null!;
        public string Fullname { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
    }
}
