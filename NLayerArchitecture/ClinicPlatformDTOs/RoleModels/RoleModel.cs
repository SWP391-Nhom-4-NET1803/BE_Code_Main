using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.RoleModels
{
    public class RoleModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        public enum Roles
        {
            Admin = 1,
            ClinicStaff = 2,
            Customer = 3
        }
    }
}
