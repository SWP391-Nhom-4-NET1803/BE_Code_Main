using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.UserModels
{
    public class UserInfoModel
    {
        // User table
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Salt { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Fullname { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsActive { get; set; } = false;
        public bool IsRemoved { get; set; } = false;
        public DateTime JoinedDate { get; set; }


        // Customer table
        public int? CustomerId { get; set; }
        public DateOnly? Birthdate { get; set; }
        public string Sex { get; set; } = null!;
        public string Insurance { get; set; } = null!;

        // Dentist table
        public int? DentistId { get; set; }
        public int? ClinicId { get; set; }
        public bool? IsOwner { get; set; }


        public const string Admin = "Admin";
        public const string Customer = "Customer";
        public const string Dentist = "Dentist";
    }
}
