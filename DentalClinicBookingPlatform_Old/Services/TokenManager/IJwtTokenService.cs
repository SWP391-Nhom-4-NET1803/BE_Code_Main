using Repositories.Models;
using Core;
using System.Security.Claims;
using Core.HttpModels.ObjectModels.AuthenticationModels;
using Core.HttpModels.ObjectModels.RoleModels;

namespace Services.JwtManager
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(User user, int duration = 10);

        string GenerateRefreshToken(User user, int duration = 60);

        User? ValidateAccessToken(string? token, out string message);

        User? ValidateAccessToken(string token, RoleModel.Roles[] roles, out string message);

        AuthenticationToken GenerateTokens(User user, int accessDuration = 10, int refreshDuration = 60);

        public ClaimsPrincipal GetPrincipalsFromToken(string token);

        public IEnumerable<Claim> GetPrincipalsFromGoogleToken(string token);

    };
}
