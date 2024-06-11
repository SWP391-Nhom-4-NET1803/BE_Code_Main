using Microsoft.AspNetCore.Http;
using Services.JwtManager;
using Services.TokenManager;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebAPI.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IJwtTokenService tokenManager)
        {
            string? token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            var user = tokenManager.ValidateAccessToken(token, out _);

            Console.WriteLine(token);
            Console.WriteLine($"Is user authorized: {user?.Email}");

            // Set the user item for further usage down the API endpoint.
            if (user != null)
            {
                context.Items["user"] = user;
            }

            await _next(context);
        }
    }
}
