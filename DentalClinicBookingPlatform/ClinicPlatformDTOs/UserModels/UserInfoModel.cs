using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.UserModels
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
        public int? Role { get; set; }
        public bool? Status { get; set; }
        public DateTime? JoinedDate { get; set; }

        // Unused properties
        public string? ProfilePicture { get; set; }

        // Customer table
        public int? CustomerId { get; set; }
        public DateOnly? Birthdate { get; set; }
        public string? Sex { get; set; }
        public string? Insurance { get; set; }

        // ClinicStaff table
        public int? ClinicStaffId { get; set; }
        public int? ClinicId { get; set; }
        public bool IsOwner { get; set; } = false;

        // Role table
        public string? RoleName { get; set; } = null;
    }
}
