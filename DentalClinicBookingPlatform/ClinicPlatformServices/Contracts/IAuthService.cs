using ClinicPlatformDTOs.AuthenticationModels;
using ClinicPlatformDTOs.UserModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformServices.Contracts
{
    public interface IAuthService : IDisposable
    {
        string GenerateAccessToken(UserInfoModel user, int duration = 10);

        string GenerateRefreshToken(UserInfoModel user, int duration = 60);

        UserInfoModel? ValidateAccessToken(string? token, out string message);

        UserInfoModel? ValidateAccessToken(string token, string roles, out string message);

        AuthenticationTokenModel GenerateTokens(UserInfoModel user, int accessDuration = 10, int refreshDuration = 60);

        public ClaimsPrincipal GetPrincipalsFromToken(string token);

        public IEnumerable<Claim> GetPrincipalsFromGoogleToken(string token);
    }
}
