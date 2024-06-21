using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.MiscModels
{
    public class EmailServiceModel
    {
        public string account { get; set; } = string.Empty;

        public string password { get; set; } = string.Empty;

        public string target { get; set; } = string.Empty;

        public string subject { get; set; } = string.Empty;

        public string body { get; set; } = string.Empty;
    }
}
