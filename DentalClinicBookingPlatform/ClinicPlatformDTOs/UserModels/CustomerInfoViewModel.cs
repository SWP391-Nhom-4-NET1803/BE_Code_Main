using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.UserModels
{
    public class CustomerInfoViewModel
    {
        public int CustomerId { get; set; }
        public string Username { get; set; } = null!;
        public string Fullname { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public DateOnly? Birthdate { get; set; } = null;
        public string Sex { get; set; } = null!;
        public string Insurance { get; set; } = null!;
        public DateTime? JoinedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
