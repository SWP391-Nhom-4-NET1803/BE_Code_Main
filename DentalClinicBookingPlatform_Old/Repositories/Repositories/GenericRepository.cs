using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Repositories.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PlatformRepository.Repositories
{
    public class GenericRepository<T, X>: IGenericRepository<T, X> where T : class
    {
        protected DentalClinicPlatformContext context;
        protected DbSet<T> dbSet;

        public GenericRepository(DentalClinicPlatformContext context)
        {
            this.context = context;
            this.dbSet = context.Set<T>();
        }

        // Function to get item using filter.
        public virtual IEnumerable<T> GetAll(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy, string includeProperties = "", int? pageSize = null, int? pageIndex = null)
        {
            IQueryable<T> query = dbSet;

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

        // Function to get all items from database.
        public virtual IEnumerable<T> GetAll()
        {
            return dbSet.ToList();
        }

        // Function to get a record by id.
        public virtual T? GetById(X id)
        {
            return dbSet.Find(id);
        }

        // Function to add new record.
        public virtual void Add(T entity)
        {
            dbSet.Add(entity);
        }

        // Function to update an existing record.
        public virtual void Update(T entity)
        {
            dbSet.Attach(entity);
            // For keeping track of entity changes.
            context.Entry(entity).State = EntityState.Modified;
        }

        // Function to delete a record.
        public virtual void Delete(T entity)
        {
            if (context.Entry(entity).State == EntityState.Detached)
            {
                dbSet.Attach(entity);
            }

            dbSet.Remove(entity);
        }

        // Function to count the number of record.
        public virtual int Count(Expression<Func<T, bool>>? filter)
        {
            IQueryable<T> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }
            return query.Count();
        }

        public virtual void Save()
        {
            context.SaveChanges();
        }
    }
}
