using ClinicPlatformObjects.TokenModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformRepositories.Contracts
{
    public interface ITokenRepository: IDisposable
    {
        TokenInfoModel? GetTokenWithId(Guid id);

        IEnumerable<TokenInfoModel> GetTokens();

        IEnumerable<TokenInfoModel> GetUserToken(int userId);

        IEnumerable<TokenInfoModel> GetUnusedToken();

        IEnumerable<TokenInfoModel> GetTokenWithReason(string reason);

        IEnumerable<TokenInfoModel> GetTokenCreatedOn(DateOnly date);

        TokenInfoModel? CreateToken(TokenInfoModel tokenInfo);

        TokenInfoModel? UpdateToken(TokenInfoModel tokenInfo);

        bool DeleteToken(Guid token);
    }
}
