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

        Clinic? GetClinicWithName(string name);

        Clinic? GetClinicOwnedBy(int user_id);

        public void CreateClinicSlot(ScheduledSlot slot);

        IEnumerable<Clinic> GetClinicStartWith(string prefix);

        IEnumerable<Clinic> GetClinicWorkInTimeRange(TimeOnly start, TimeOnly end);

        IEnumerable<Clinic> GetClinicWithService(List<Service> services);

        public IEnumerable<ScheduledSlot> GetAllClinicSlot(int clinicId);
    }
}
