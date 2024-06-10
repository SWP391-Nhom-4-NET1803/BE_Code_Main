using Core.HttpModels;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repositories;
using Repositories.Models;
using Services.JwtManager;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Services.TokenManager
{
    /// <summary>
    ///  The Service for creating and validating json web token, refreshing new token.
    /// </summary>
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _config;
        private readonly UnitOfWork _unitOfWork;

        public JwtTokenService(IConfiguration configuration, DentalClinicPlatformContext Dbcontext)
        {
            _config = configuration;
            _unitOfWork = new UnitOfWork(Dbcontext);
        }


        /// <summary>
        ///     Generate Access JSON Token that last in a short amount of time.
        ///     This is generally used for authenticating users in the API endpoints.
        /// </summary>
        /// <param name="user"> The user object (for claims generation)</param>
        /// <param name="duration"> The token lifetime</param>
        /// <returns></returns>



        //Cái AccessToken với cái RefreshToken là từ cái JWT Token ra hả ông

        public string GenerateAccessToken(User user, int duration = 10)
        {

            // Getting token Key and Issuer in the configuration file for encrypting the token.
            string TokenKey = _config.GetValue<string>("JWT:Key")!;
            string Issuer = _config.GetValue<string>("JWT:Issuer")!;

            // Get User information to put in the token to create user Identity.

            //var userStatus = _unitOfWork.StatusRepository.GetById(user.Status)!;

            var userStatus = true;
            var userRole = _unitOfWork.RoleRepository.GetById(user.RoleId);

            if (userRole == null)
            {
                throw new Exception("Role not exist");
            }

            var claims = new ClaimsIdentity(new[]
            {
                new Claim("id", user.UserId.ToString()),
                new Claim("username", user.Username),
                new Claim("email", user.Email!),
                new Claim("role", userRole.RoleName),

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

        /// <summary>
        ///  Generate Refresh Token that last for a long amount of time.
        ///  Used to re-generating user token.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public string GenerateRefreshToken(User user, int duration = 30)
        {
            string expirationTime = DateTime.UtcNow.AddMinutes(duration).ToString("MM-dd-yyyy HH:mm:ss");

            using (var rng = RandomNumberGenerator.Create())
            {
                var randomNumber = new byte[2];
                rng.GetBytes(randomNumber);

                // We add random data for extra "protection"
                string PlainInfo = $"{randomNumber[0].ToString()}|{user.Username}|{expirationTime}|{randomNumber[1].ToString()}";
                byte[] bytesInfo = Encoding.UTF8.GetBytes(PlainInfo);

                return Convert.ToBase64String(bytesInfo);
            }
        }

        public AuthenticationToken GenerateTokens(User user, int accessDuration = 1, int refreshDuration = 2)
        {
            AuthenticationToken authToken = new AuthenticationToken()
            {
                AccessToken = GenerateAccessToken(user),
                RefreshToken = GenerateRefreshToken(user)
            };

            return authToken;
        }

        public User? ValidateAccessToken(string token, out string message)
        {
            if (token == null)
            {
                message = "No token was provided!";
                return null;
            }

            var TokenHandler = new JwtSecurityTokenHandler();

            string TokenKey = _config.GetValue<string>("JWT:Key")!;
            string Issuer = _config.GetValue<string>("JWT:Issuer")!;

            var validatior = new TokenValidationParameters()
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TokenKey)),
                ClockSkew = TimeSpan.Zero,
            };

            try
            {
                TokenHandler.ValidateToken(token, validatior, out var validatedToken);

                var Token = (JwtSecurityToken)validatedToken;

                int userId = int.Parse(Token.Claims.First(x => x.Type == "id").Value);

                message = "Valid token";
                return _unitOfWork.UserRepository.GetById(userId);
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

        public User? ValidateAccessToken(string token, string?[] roles, out string message)
        {
            if (token == null)
            {
                message = "No token provided";
                return null;
            }

            var TokenHandler = new JwtSecurityTokenHandler();

            string TokenKey = _config.GetValue<string>("JWT:Key")!;
            string Issuer = _config.GetValue<string>("JWT:Issuer")!;

            var validatior = new TokenValidationParameters()
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TokenKey)),
                ClockSkew = TimeSpan.Zero,
            };

            try
            {
                TokenHandler.ValidateToken(token, validatior, out var validatedToken);

                var Token = (JwtSecurityToken)validatedToken;

                int userId = int.Parse(Token.Claims.First(x => x.Type == "id").Value);
                string userRole = Token.Claims.First(x => x.Type == "role").Value;

                if (roles.Length > 0 && !roles.Contains(userRole))
                {
                    throw new Exception("Unauthorized!");
                }

                message = "Token validated!";
                return _unitOfWork.UserRepository.GetById(userId);
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

        public ClaimsPrincipal GetPrincipalsFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenKey = _config.GetValue<string>("JWT:Key")!;

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
                //ValidIssuer = _config.GetValue<string>("JWT:Issuer")!,
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };

            SecurityToken securityToken;

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);

            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        public IEnumerable<Claim> GetPrincipalsFromGoogleToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetValue<string>("GoogleO2Auth:Token")!)),
                //ValidIssuer = _config.GetValue<string>("JWT:Issuer")!,
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };

            SecurityToken securityToken = tokenHandler.ReadToken(token);

            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null)
                throw new SecurityTokenException("Invalid token");

            return jwtSecurityToken.Claims;
        }
    }
}
