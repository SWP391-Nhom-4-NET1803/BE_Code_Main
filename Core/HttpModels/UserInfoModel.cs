using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.HttpModels
{
    public class UserInfoModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; } = string.Empty;
        public string? Fullname { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; } = string.Empty;
        public string? Insurance {  get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        // Optional Data
        public DateTime? JoinedDate { get; set; }
        
        public int? Clinic {get; set; }
    }
}
