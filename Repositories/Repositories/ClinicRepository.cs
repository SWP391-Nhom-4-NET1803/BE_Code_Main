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
    public class ClinicRepository : GenericRepository<Clinic, int>, IClinicRepository
    {
        public ClinicRepository(DentalClinicPlatformContext context) : base(context) { }

        public bool CheckClinicAvailability(string clinicName, out string message)
        {
            List<Clinic> ExistanceList = dbSet.Where((clinic) => (clinic.Name == clinicName)).ToList();

            if (ExistanceList.Count > 0)
            {
                message = "Clinic with this name already existed";
                return false;
            }

            message = "Clinic is available for creation";
            return true;
        }

        public void CreateClinicSlot(ScheduledSlot slot)
        {
            context.ScheduledSlots.Add(slot);
        }

        public Clinic? GetClinicOwnedBy(int user_id)
        {
            return dbSet.Where(clinic => clinic.OwnerId == user_id).FirstOrDefault();
        }

        public Clinic? GetClinicWithName(string name)
        {
            return dbSet.Where(clinic => clinic.Name == name).FirstOrDefault();
        }

        public IEnumerable<Clinic> GetClinicStartWith(string prefix)
        {
            return this.GetAll(filter: x => x.Name.StartsWith(prefix), orderBy: query => query.OrderBy(clinic => clinic.Name));
        }

        // Still testing
        public IEnumerable<Clinic> GetClinicWithService(List<Service> services)
        {
            return this.GetAll(filter: x => services.All(requestedService => x.ClinicServices.All(x => x.Service == requestedService)), orderBy: query => query.OrderBy(student => student.Name));
        }

        public IEnumerable<Clinic> GetClinicWorkInTimeRange(TimeOnly start, TimeOnly end)
        {
            return this.GetAll(filter: x => x.OpenHour.CompareTo(start) >= 0 && x.CloseHour.CompareTo(end) <= 0, orderBy: query => query.OrderBy(clinic => clinic.OpenHour));
        }

        public IEnumerable<ScheduledSlot> GetAllClinicSlot(int clinicId)
        {
            return context.ScheduledSlots.Where(x => x.ClinicId == clinicId).Include(x => x.Slot);
        }
    }
}
