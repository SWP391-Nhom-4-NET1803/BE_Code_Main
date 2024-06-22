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
    public class UserDAO : IFilterQuery<User>, IDisposable
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
            return _context.Users.Where(x => x.Id == id)
                .FirstOrDefault();
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users.ToList();
        }

        public User? UpdateUser(User user)
        {
            User? userInfo = GetUser(user.Id);

            if (userInfo != null)
            {
                _context.Users.Update(user);
                SaveChanges();

                return user;
            }

            return null;
        }

        public bool DeleteUser(int userId)
        {
            User? user = GetUser(userId);

            if (user != null)
            {
                _context.Users.Remove(user);
                this.SaveChanges();

                return true;
            }

            return false;
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

            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (filter != null)
            {
                query = query.Where(filter);
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
