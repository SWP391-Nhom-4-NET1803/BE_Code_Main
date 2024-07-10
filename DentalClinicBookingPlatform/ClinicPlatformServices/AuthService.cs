using ClinicPlatformDTOs.AuthenticationModels;
using ClinicPlatformDTOs.UserModels;
using ClinicPlatformRepositories.Contracts;
using ClinicPlatformServices.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace ClinicPlatformServices
{
    public class AuthService : IAuthService
    {
        private bool disposedValue;

        private readonly IConfiguration configuration;
        private readonly IUserRepository userRepository;
        private string TokenKey;
        private string GoogleO2Auth;
        private string Issuer;

        public AuthService(IConfiguration config, IUserRepository userRepository)
        {
            this.configuration = config;
            this.userRepository = userRepository;
            TokenKey = configuration.GetSection("JWT:Key").Value!;
            Issuer = configuration.GetSection("JWT:Issuer").Value!;
            GoogleO2Auth = configuration.GetSection("GoogleO2Auth:Token").Value!;
        }

        public string GenerateAccessToken(UserInfoModel user, int duration = 10)
        {
            // Get User information to put in the token to create user Identity.
            var claims = new ClaimsIdentity(new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Username!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Role, user.Role.ToString()!),
                new Claim("IsOwner", user.IsOwner ? "1" : "0"),

                //new Claim("status",userStatus.StatusName),
            });

            var TokenHandler = new JwtSecurityTokenHandler();

            // Token Descriptor is used in order to make user Identity.
            SecurityTokenDescriptor TokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = claims,
                Issuer = Issuer,
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(0.1 + duration),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TokenKey)), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = TokenHandler.CreateToken(TokenDescriptor);

            return TokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken(UserInfoModel user, int duration = 60)
        {
            string expirationTime = DateTime.UtcNow.AddMinutes(duration).ToString("MM-dd-yyyy HH:mm:ss");

            using (var rng = RandomNumberGenerator.Create())
            {
                var randomNumber = new byte[2];
                rng.GetBytes(randomNumber);

                // We add random data for extra "protection"
                string PlainInfo = $"{randomNumber[0]}|{user.Username}|{expirationTime}|{randomNumber[1]}";
                byte[] bytesInfo = Encoding.UTF8.GetBytes(PlainInfo);

                return Convert.ToBase64String(bytesInfo);
            }
        }

        public AuthenticationTokenModel GenerateTokens(UserInfoModel user, int accessDuration = 10, int refreshDuration = 60)
        {
            AuthenticationTokenModel authToken = new AuthenticationTokenModel()
            {
                AccessToken = GenerateAccessToken(user),
                RefreshToken = GenerateRefreshToken(user)
            };

            return authToken;
        }

        public IEnumerable<Claim> GetPrincipalsFromGoogleToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GoogleO2Auth)),
                //ValidIssuer = _config.GetValue<string>("JWT:Issuer")!,
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };

            SecurityToken securityToken = tokenHandler.ReadToken(token);

            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null)
                throw new SecurityTokenException("Invalid token");

            return jwtSecurityToken.Claims;
        }

        public ClaimsPrincipal GetPrincipalsFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TokenKey)),
                ValidIssuer = Issuer,
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };

            SecurityToken securityToken;

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);

            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        public UserInfoModel? ValidateAccessToken(string? token, out string message)
        {
            if (token == null)
            {
                message = "No token was provided!";
                return null;
            }

            var TokenHandler = new JwtSecurityTokenHandler();

            var validatior = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TokenKey)),
                ClockSkew = TimeSpan.Zero,
            };

            try
            {
                TokenHandler.ValidateToken(token, validatior, out var validatedToken);

                var Token = (JwtSecurityToken)validatedToken;

                int userId = int.Parse(Token.Claims.First(x => x.Type == "id").Value);

                message = "Valid token";

                return userRepository.GetUser(userId);
            }
            catch (SecurityTokenExpiredException)
            {
                message = "User access token is expired.";
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                message = "The signature of this token is invalid.";
            }
            catch (SecurityTokenInvalidIssuerException)
            {
                message = "Unknown Issuer!";
            }

            return null;
        }

        public UserInfoModel? ValidateAccessToken(string token, string roles, out string message)
        { 
            if (token == null)
            {
                message = "No token provided";
                return null;
            }

            var TokenHandler = new JwtSecurityTokenHandler();

            var validatior = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TokenKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            };

            try
            {
                TokenHandler.ValidateToken(token, validatior, out var validatedToken);

                var Token = (JwtSecurityToken)validatedToken;

                int userId = int.Parse(Token.Claims.First(x => x.Type == "id").Value);
                string userRole = Token.Claims.First(x => x.Type == "role").Value;


                var roleList = roles.Split(",");

                if (roles.Length > 0 && !roleList.Contains(userRole))
                {
                    throw new Exception("Unauthorized!");
                }

                message = "Token validated!";
                return userRepository.GetUser(userId);
            }
            catch (SecurityTokenExpiredException)
            {
                message = "User access token is expired.";
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                message = "The signature of this token is invalid.";
            }
            catch (SecurityTokenInvalidIssuerException)
            {
                message = "The token is issued by an unknown source!";
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    userRepository.Dispose();
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
