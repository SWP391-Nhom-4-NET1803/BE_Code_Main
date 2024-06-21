using Core.HttpModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Services.JwtManager;
using Core.HttpModels.ObjectModels.RoleModels;

namespace WebAPI.Helper.AuthorizationPolicy
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class JwtTokenAuthorization : Attribute, IAuthorizationFilter
    {
        private readonly RoleModel.Roles[] allowedRoles;

        public JwtTokenAuthorization()
        {
            allowedRoles = [];
        }

        public JwtTokenAuthorization(params RoleModel.Roles[]? Roles)
        {
            if (Roles == null)
            {
                allowedRoles = [];
            }
            else
            {
                allowedRoles = Roles;
            }
            
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {

            // Skip all endpoint methods that allow anonymous access ([AllowAnonymous] attribute)
            if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
            {
                return;
            }

            var tokenManager = context.HttpContext.RequestServices.GetService<IJwtTokenService>();
            var AccessToken = context.HttpContext.Request.Headers.Authorization.ToString();

            if (tokenManager!.ValidateAccessToken(AccessToken.Split(" ").Last(), allowedRoles, out var message) == null)
            {

                var response = new HttpResponseModel() {StatusCode = 404, Message = "Unauthorized access", Detail = message};

                context.Result = new JsonResult(new { response })
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    ContentType = "application/json",
                };
                return;
            }
        }
    }
}
