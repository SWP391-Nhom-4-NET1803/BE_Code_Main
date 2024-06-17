using ClinicPlatformDTOs.UserModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.ClinicModels
{
    public class ClinicInformationModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? OpenHour { get; set; }
        public string? CloseHour { get; set; }
        public List<ClinicServiceModel> ClinicServices { get; set; } = [];
        public List<ClinicStaffInfoModel> ClinicStaff { get; set; } = [];
        public bool Status { get; set; } = false;
    }
}
