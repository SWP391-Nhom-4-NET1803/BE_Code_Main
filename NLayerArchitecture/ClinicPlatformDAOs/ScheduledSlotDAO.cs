using ClinicPlatformBusinessObject;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDAOs
{
    public class ScheduledSlotDAO: IDisposable
    {
        private readonly DentalClinicPlatformContext _context;
        private bool disposedValue;

        public ScheduledSlotDAO()
        {
            _context = new DentalClinicPlatformContext();
        }

        public ScheduledSlotDAO(DentalClinicPlatformContext context)
        {
            _context = context;
        }

        public ScheduledSlot AddScheduledSlot(ScheduledSlot ScheduledSlot)
        {
            _context.Add(ScheduledSlot);
            this.SaveChanges();

            return ScheduledSlot;
        }

        public ScheduledSlot? GetScheduledSlot(Guid ScheduledSlotId)
        {
            return _context.ScheduledSlots.Where(x => x.ScheduleSlotId == ScheduledSlotId).FirstOrDefault();
        }

        public IEnumerable<ScheduledSlot> GetAllScheduledSlot()
        {
            return _context.ScheduledSlots.ToList();
        }

        public ScheduledSlot UpdateScheduledSlot(ScheduledSlot ScheduledSlot)
        {
            ScheduledSlot? ScheduledSlotInfo = GetScheduledSlot(ScheduledSlot.ScheduleSlotId);

            if (ScheduledSlotInfo != null)
            {
                _context.ScheduledSlots.Update(ScheduledSlot);
                SaveChanges();
            }

            return ScheduledSlot;
        }

        public void DeleteScheduledSlot(Guid ScheduledSlotId)
        {
            ScheduledSlot? ScheduledSlot = GetScheduledSlot(ScheduledSlotId);

            if (ScheduledSlot != null)
            {
                _context.ScheduledSlots.Remove(ScheduledSlot);
                this.SaveChanges();
            }
        }

        public void SaveChanges()
        {
            this._context.SaveChanges();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public IEnumerable<ScheduledSlot> Filter(Expression<Func<ScheduledSlot, bool>> filter, Func<IQueryable<ScheduledSlot>, IOrderedQueryable<ScheduledSlot>>? orderBy, string includeProperties = "", int? pageSize = null, int? pageIndex = null)
        {
            IQueryable<ScheduledSlot> query = _context.ScheduledSlots;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            // Implementing pagination
            if (pageIndex.HasValue && pageSize.HasValue)
            {
                // Ensure the pageIndex and pageSize are valid
                int validPageIndex = pageIndex.Value > 0 ? pageIndex.Value - 1 : 0;
                int validPageSize = pageSize.Value > 0 ? pageSize.Value : 10; // Assuming a default pageSize of 10 if an invalid value is passed

                query = query.Skip(validPageIndex * validPageSize).Take(validPageSize);
            }

            return query.ToList();
        }
    }
}
