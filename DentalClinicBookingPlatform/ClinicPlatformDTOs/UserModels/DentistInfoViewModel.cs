using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.UserModels
{
    public class DentistInfoViewModel
    {
        public int Id { get; set; }
        public int StaffId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Fullname { get; set; }
        public string? Role { get; set; }
        public bool isRemove {  get; set; }
        public bool isActive { get; set; }
        public DateTime? JoinedDate { get; set; }
        public int? ClinicId { get; set; }
        public string? ClinicName { get; set; }
        public bool? IsOwner { get; set; }
    }
}
