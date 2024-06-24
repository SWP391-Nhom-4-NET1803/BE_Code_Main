using ClinicPlatformDTOs.UserModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.ClinicModels
{
    public class ClinicInfoModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Email { get; set; } = null!;
        public TimeOnly OpenHour { get; set; }
        public TimeOnly CloseHour { get; set; }
        public int OwnerId { get; set; }
        public bool Working { get; set; } = false;
        public string Status { get; set; } = Unverified;

        public const string Unverified = "unverified";
        public const string Verified = "verified";
        public const string Removed = "removed";
    }
}
