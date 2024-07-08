using ClinicPlatformObjects.TokenModels;
using ClinicPlatformRepositories.Contracts;
using ClinicPlatformServices.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformServices
{
    public class TokenService : ITokenService
    {
        private readonly ITokenRepository tokenRepository;
        private readonly IUserRepository userRepository;
        private bool disposedValue;

        public TokenService(ITokenRepository tokenRepository, IUserRepository userRepository)
        {
            this.tokenRepository = tokenRepository;
            this.userRepository = userRepository;
        }

        public TokenInfoModel? CreateNewToken(int userId, string reason, out string message)
        {

            if (userRepository.GetUser(userId) == null)
            {
                message = "Can not find user";
                return null;
            }
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (res.Length < 8)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }

            TokenInfoModel? token = new TokenInfoModel
            {
                Id = Guid.NewGuid(),
                Value = res.ToString(),
                Reason = reason,
                Creation = DateTime.UtcNow,
                Expiration = DateTime.UtcNow.AddMinutes(10),
                UserId = userId,
            };

            token = tokenRepository.CreateToken(token);

            message = token != null ? "token created" : "Token creation failed.";
            return token;
        }

        public TokenInfoModel? CreateUserPasswordResetToken(int userId, out string message)
        {
            return CreateNewToken(userId, "pasword_reset", out message);
        }

        public TokenInfoModel? MarkTokenAsUsed(Guid tokenId)
        {
            var token = tokenRepository.GetTokenWithId(tokenId);

            if (token != null)
            {
                token.Used = true;
                token = tokenRepository.UpdateToken(token);

                return token;
            }

            return null;
        }

        public TokenInfoModel? MatchTokenValue(string tokenValue, out string message)
        {
            var token = tokenRepository.GetTokens().Where(x => x.Value == tokenValue).FirstOrDefault();

            if (token != null)
            {
                if (token.Used)
                {
                    message = "This token has been used!";
                    return null;
                }

                if (token.Expiration <=  DateTime.UtcNow) 
                {
                    message = "This token is expired!";
                    return null;
                }

                message = "Token found!";
                return token;
            }

            message = "Can not find any token with given value";
            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    tokenRepository.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
