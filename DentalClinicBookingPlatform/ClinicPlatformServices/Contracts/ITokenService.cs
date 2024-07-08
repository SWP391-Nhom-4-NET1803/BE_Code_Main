using ClinicPlatformObjects.TokenModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformServices.Contracts
{
    public interface ITokenService: IDisposable
    {
        TokenInfoModel? CreateNewToken(int userId, string reason, out string message);

        TokenInfoModel? CreateUserPasswordResetToken(int userId, out string message);

        TokenInfoModel? MatchTokenValue(string tokenValue, out string message);

        TokenInfoModel? MarkTokenAsUsed(Guid tokenId);
    }
}
