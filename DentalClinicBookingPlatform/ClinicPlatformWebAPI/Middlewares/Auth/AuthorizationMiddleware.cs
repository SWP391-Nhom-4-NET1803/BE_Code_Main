using ClinicPlatformDTOs.UserModels;
using ClinicPlatformServices.Contracts;

namespace ClinicPlatformWebAPI.Middlewares.Authentication
{
    public class AuthorizationMiddleware : IMiddleware
    {
        private readonly IAuthService authService;

        public AuthorizationMiddleware(IAuthService authService)
        {
            this.authService = authService;
        }

        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            UserInfoModel? user = authService.ValidateAccessToken(context.Request.Headers.Authorization, out var message);

            context.Items.Add("user", user);

            return next(context);
        }
    }
}
