using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformObjects.UserModels.DentistModel
{
    public class DentistInfoViewModel
    {
        public int DentistId { get; set; }
        public string Fullname { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime? JoinedDate { get; set; }
        public int? ClinicId { get; set; }
        public bool IsOwner { get; set; }
    }
}
