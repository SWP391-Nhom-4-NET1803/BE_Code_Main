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
    public class ClinicSlotDAO: IDisposable
    {
        private readonly DentalClinicPlatformContext _context;
        private bool disposedValue;

        public ClinicSlotDAO()
        {
            _context = new DentalClinicPlatformContext();
        }

        public ClinicSlotDAO(DentalClinicPlatformContext context)
        {
            _context = context;
        }

        public ClinicSlot AddClinicSlot(ClinicSlot ClinicSlot)
        {
            _context.Add(ClinicSlot);
            this.SaveChanges();

            return ClinicSlot;
        }

        public ClinicSlot? GetClinicSlot(Guid ClinicSlotId)
        {
            return _context.ClinicSlots
                .Where(x => x.SlotId == ClinicSlotId)
                .Include(x => x.Time)
                .FirstOrDefault();
        }

        public IEnumerable<ClinicSlot> GetAllClinicSlot()
        {
            return _context.ClinicSlots.Include(x=> x.Time).ToList();
        }

        public ClinicSlot? UpdateClinicSlot(ClinicSlot clinicSlot)
        {
            ClinicSlot? ClinicSlotInfo = GetClinicSlot(clinicSlot.SlotId);

            if (ClinicSlotInfo != null)
            {
                _context.ClinicSlots.Update(clinicSlot);
                SaveChanges();

                return clinicSlot;
            }

            return null;
        }

        public bool DeleteClinicSlot(Guid ClinicSlotId)
        {
            ClinicSlot? ClinicSlot = GetClinicSlot(ClinicSlotId);

            if (ClinicSlot != null)
            {
                _context.ClinicSlots.Remove(ClinicSlot);
                this.SaveChanges();

                return true;
            }

            return false;
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

        public IEnumerable<ClinicSlot> Filter(Expression<Func<ClinicSlot, bool>> filter, Func<IQueryable<ClinicSlot>, IOrderedQueryable<ClinicSlot>>? orderBy, string includeProperties = "", int? pageSize = null, int? pageIndex = null)
        {
            IQueryable<ClinicSlot> query = _context.ClinicSlots;

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
