using Microsoft.EntityFrameworkCore;
using PlatformRepository.Repositories;
using Repositories.Models;
using Repositories.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class RoleRepository: GenericRepository<Role, int>, IRoleRepository
    {

        public RoleRepository(DentalClinicPlatformContext context) : base(context) { }
        public Role? GetRoleByName(string roleName)
        {
            return dbSet.First(x => x.RoleName == roleName);
        }
    }
}
