using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.UserModels
{
    public class CustomerInfoViewModel
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public int? Role { get; set; }
        public bool? Status { get; set; }
        public DateOnly? Birthdate { get; set; } = null;
        public string? Sex { get; set; }
        public string? Insurance { get; set; }
        public DateTime? JoinedDate { get; set; }
    }
}
