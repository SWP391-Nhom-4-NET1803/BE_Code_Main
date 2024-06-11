using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.HttpModels.ObjectModels
{
    public class UserInfoModel
    {
        // User table
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Fullname { get; set; }
        public string? Role { get; set; }
        public string? Status { get; set; }
        public DateTime? JoinedDate { get; set; }

        // Unused properties
        public string? ProfilePicture { get; set; }

        // Customer table
        public DateOnly? Birthdate { get; set; } = null;
        public string? Sex {  get; set; }
        public string? Insurance { get; set; }

        // ClinicStaff table
        public int? Clinic { get; set; }
        public bool? IsOwner { get; set; } 
    }
}
