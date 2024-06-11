﻿using PlatformRepository.Repositories;
using Repositories.Models;
using Repositories.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class ClinicStaffRepository : GenericRepository<ClinicStaff, int>, IClinicStaffRepository
    {
        public ClinicStaffRepository(DentalClinicPlatformContext context) : base(context) { }
    }
}
