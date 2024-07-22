using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformObjects.UserModels
{
    public class UserInfoViewModel
    {
        public int UserId { get; set; }
        public int? CustomerId { get; set; }
        public int? DentistId { get; set; }
        public int? ClinicId {  get; set; }
        public string Role { get; set; } = null!;
        public string? Fullname { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; } = null!;
        public string? Insurance { get; set; } = null!;
        public DateOnly? Birthdate { get; set; }
        public string? Sex { get; set; } = null!;
        public DateTime JoinedDate { get; set; }
        public bool IsActive { get; set; } = false;
        public bool IsRemoved { get; set; } = false;
    }
}
