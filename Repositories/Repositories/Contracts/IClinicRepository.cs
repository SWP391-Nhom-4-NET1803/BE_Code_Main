using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repositories.Contracts
{
    public interface IClinicRepository
    {
        bool CheckClinicAvailability(string clinicName, out string message);

        Clinic? getClinicWithName(string name);

        Clinic? getClinicOwnedBy(int user_id);

        IEnumerable<Clinic> getClinicStartWith(string prefix);

        IEnumerable<Clinic> getClinicWorkInTimeRange(TimeOnly start, TimeOnly end);

        IEnumerable<Clinic> getClinicWithService(List<Service> services);
    }
}
