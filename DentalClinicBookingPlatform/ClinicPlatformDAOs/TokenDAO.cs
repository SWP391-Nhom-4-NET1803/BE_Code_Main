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
    internal class TokenDAO: IDisposable
    {
        private readonly DentalClinicPlatformContext _context;
        private bool disposedValue;

        public TokenDAO()
        {
            _context = new DentalClinicPlatformContext();
        }

        public TokenDAO(DentalClinicPlatformContext context)
        {
            _context = context;
        }

        public Token AddToken(Token token)
        {
            _context.Add(token);
            this.SaveChanges();

            return token;
        }

        public Token? GetToken(Guid tokenId)
        {
            return _context.Tokens.Where(x => x.Id == tokenId).FirstOrDefault();
        }

        public IEnumerable<Token> GetAllToken()
        {
            return _context.Tokens.ToList();
        }

        public Token UpdateToken(Token token)
        {
            Token? ServiceInfo = GetToken(token.Id);

            if (ServiceInfo != null)
            {
                _context.Tokens.Update(token);
                SaveChanges();

                return token;
            }

            return null;
        }

        public bool DeleteToken(Guid tokenId)
        {
            Token? Token = GetToken(tokenId);

            if (Token != null)
            {
                _context.Tokens.Remove(Token);
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

        public IEnumerable<Token> Filter(Expression<Func<Token, bool>> filter, Func<IQueryable<Token>, IOrderedQueryable<Token>>? orderBy, string includeProperties = "", int? pageSize = null, int? pageIndex = null)
        {
            IQueryable<Token> query = _context.Tokens;

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
