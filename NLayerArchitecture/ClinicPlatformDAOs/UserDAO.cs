using ClinicPlatformBusinessObject;
using ClinicPlatformDTOs.UserModels;
using ClinicPlatformDAOs.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace ClinicPlatformDAOs
{
    public class UserDAO: IFilterQuery<User>
    {
        private readonly DentalClinicPlatformContext _context;
        private bool disposedValue;

        public UserDAO()
        {
            _context = new DentalClinicPlatformContext();
        }

        public UserDAO(DentalClinicPlatformContext context)
        {
            _context = context;
        }

        public User AddUser(User user)
        {
            _context.Add(user);
            this.SaveChanges();
            return user;
        }

        public User? GetUser(int id)
        {
            return _context.Users
                .Include(x => x.ClinicStaff)
                .Include(x => x.Customer)
                .Include(x => x.Role)
                .Where(x => x.UserId == id)
                .FirstOrDefault();
        }

        public IEnumerable<User> GetAllUsers()
        {
            return _context.Users.Include(x => x.ClinicStaff).Include(x => x.Customer).Include(x => x.Role).ToList();
        }

        public Customer? GetCustomerByCustomerId(int customerId)
        {
            return _context.Customers
                .Include(x => x.User)
                .Where(x => x.CustomerId == customerId)
                .FirstOrDefault();
        }

        public ClinicStaff? GetStaffByStaffId(int staffId)
        {
            return _context.ClinicStaffs
                .Include(x => x.User)
                .Where(x => x.StaffId == staffId)
                .FirstOrDefault();
        }

        public User UpdateUser(User user)
        {
            User? userInfo = GetUser(user.UserId);

            if (userInfo != null)
            {
                _context.Users.Update(user);
            }

            return user;
        }

        public void DeleteUser(int userId)
        {
            User? user = GetUser(userId);

            if (user != null)
            {
                _context.Users.Remove(user);
                this.SaveChanges();
            }
        }

        public void SaveChanges()
        {
            this._context.SaveChanges();
        }

        public virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this._context.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public IEnumerable<User> Filter(Expression<Func<User, bool>> filter, Func<IQueryable<User>, IOrderedQueryable<User>>? orderBy = null, string includeProperties = "", int? pageSize = null, int? pageIndex = null)
        {
            IQueryable<User> query = _context.Users;

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
