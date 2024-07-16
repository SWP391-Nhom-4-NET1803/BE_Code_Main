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
            string? token = context.Request.Headers.Authorization;

            if (!string.IsNullOrEmpty(token) )
            {
                UserInfoModel? user = authService.ValidateAccessToken(token.Split(" ").Last(), out var message);

                Console.WriteLine(message);

                context.Items.Add("user", user);
            }

            return next(context);
        }
    }
}
