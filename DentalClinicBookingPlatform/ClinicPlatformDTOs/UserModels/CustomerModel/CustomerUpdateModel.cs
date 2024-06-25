using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformObjects.UserModels.CustomerModel
{
    public class CustomerUpdateModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Fullname { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Sex { get; set; } = null!;
        public string Insurance { get; set; } = null!;
        public DateOnly? Birthdate { get; set; }
    }
}
