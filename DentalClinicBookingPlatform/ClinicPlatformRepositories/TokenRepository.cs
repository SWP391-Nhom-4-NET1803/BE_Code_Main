using ClinicPlatformDatabaseObject;
using ClinicPlatformObjects.TokenModels;
using ClinicPlatformRepositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformRepositories
{
    public class TokenRepository : ITokenRepository
    {

        DentalClinicPlatformContext context;
        private bool disposedValue;

        public TokenRepository(DentalClinicPlatformContext context)
        {
            this.context = context;
        }

        public TokenInfoModel? CreateToken(TokenInfoModel tokenInfo)
        {
            var token = ConvertToToken(tokenInfo);

            context.Tokens.Add(token);
            context.SaveChanges();

            return ConvertToTokenInfo(token);
        }

        public bool DeleteToken(Guid token)
        {
            var target = context.Tokens.Find(token);

            if (target != null)
            {
                context.Tokens.Remove(target);
                context.SaveChanges();

                return true;
            }

            return false;
        }

        public IEnumerable<TokenInfoModel> GetTokenCreatedOn(DateOnly date)
        {
            return context.Tokens.Where(x => DateOnly.FromDateTime(x.CreationTime) == date).Select(x => ConvertToTokenInfo(x)).ToList();
        }

        public IEnumerable<TokenInfoModel> GetTokens()
        {
            return context.Tokens.Select(x => ConvertToTokenInfo(x)).ToList();
        }

        public TokenInfoModel? GetTokenWithId(Guid id)
        {
            var token = context.Tokens.Find(id);

            return token != null ? ConvertToTokenInfo(token) : null;
        }

        public IEnumerable<TokenInfoModel> GetTokenWithReason(string reason)
        {
            return context.Tokens.Where(x => x.Reason == reason).Select(x => ConvertToTokenInfo(x)).ToList();
        }

        public IEnumerable<TokenInfoModel> GetUnusedToken()
        {
            return context.Tokens.Where(x => !x.Used).Select(x => ConvertToTokenInfo(x)).ToList();
        }

        public IEnumerable<TokenInfoModel> GetUserToken(int userId)
        {
            return context.Tokens.Where(x => x.User == userId).Select(x => ConvertToTokenInfo(x)).ToList();
        }

        public TokenInfoModel? UpdateToken(TokenInfoModel tokenInfo)
        {
            var token = context.Tokens.Find(tokenInfo.Id);

            if (token != null)
            {
                token.Expiration = tokenInfo.Expiration;
                token.TokenValue = tokenInfo.Value;
                token.Used = tokenInfo.Used;

                context.Tokens.Update(token);
                context.SaveChanges();

                return tokenInfo;
            }

            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    context.Dispose();
                }

                disposedValue = true;
            }
        }


        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private static TokenInfoModel ConvertToTokenInfo(Token token)
        {
            return new TokenInfoModel
            {
                Id = token.Id,
                Value = token.TokenValue,
                Creation = token.CreationTime,
                Expiration = token.Expiration,
                Reason = token.Reason,
                UserId = token.User,
                Used = token.Used,
            };
        }
    
        private static Token ConvertToToken(TokenInfoModel tokenInfo)
        {
            return new Token
            {
                Id = tokenInfo.Id,
                TokenValue = tokenInfo.Value,
                Reason = tokenInfo.Reason,
                Expiration = tokenInfo.Expiration,
                CreationTime = tokenInfo.Creation,
                User = tokenInfo.UserId,
                Used = tokenInfo.Used,
            };
        }
    
    }
}
