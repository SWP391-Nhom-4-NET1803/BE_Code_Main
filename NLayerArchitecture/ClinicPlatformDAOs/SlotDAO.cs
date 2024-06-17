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
    public class SlotDAO : IDisposable
    {
        private readonly DentalClinicPlatformContext _context;
        private bool disposedValue;

        public SlotDAO()
        {
            _context = new DentalClinicPlatformContext();
        }

        public SlotDAO(DentalClinicPlatformContext context)
        {
            _context = context;
        }

        public Slot AddSlot(Slot Slot)
        {
            _context.Add(Slot);
            this.SaveChanges();

            return Slot;
        }

        public Slot? GetSlot(int SlotId)
        {
            return _context.Slots.Where(x => x.SlotId == SlotId).FirstOrDefault();
        }

        public IEnumerable<Slot> GetAllSlot()
        {
            return _context.Slots.ToList();
        }

        public Slot UpdateSlot(Slot Slot)
        {
            Slot? SlotInfo = GetSlot(Slot.SlotId);

            if (SlotInfo != null)
            {
                _context.Slots.Update(Slot);
                SaveChanges();
            }

            return Slot;
        }

        public void DeleteSlot(int SlotId)
        {
            Slot? Slot = GetSlot(SlotId);

            if (Slot != null)
            {
                _context.Slots.Remove(Slot);
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

        public IEnumerable<Slot> Filter(Expression<Func<Slot, bool>> filter, Func<IQueryable<Slot>, IOrderedQueryable<Slot>>? orderBy, string includeProperties = "", int? pageSize = null, int? pageIndex = null)
        {
            IQueryable<Slot> query = _context.Slots;

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
