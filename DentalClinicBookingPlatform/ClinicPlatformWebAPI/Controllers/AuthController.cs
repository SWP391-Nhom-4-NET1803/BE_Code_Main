using ClinicPlatformDTOs.AuthenticationModels;
using ClinicPlatformServices.Contracts;
using ClinicPlatformWebAPI.Helpers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClinicPlatformWebAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IAuthService authService;

        public AuthController(IUserService userService, IAuthService authService)
        {
            this.userService = userService;
            this.authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public ActionResult<IHttpResponseModel<AuthenticationTokenModel>> Login([FromBody] UserAuthenticationRequestModel loginInfo)
        {
            bool isValidUser = userService.CheckLogin(loginInfo.Username, loginInfo.Password, out var user);

            if (isValidUser)
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Message = "Login Successfully",
                    Content = authService.GenerateTokens(user!)
                });
            }
            else
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Message = "Login Failed",
                    Detail = "Invalid user or username"
                });
            }

        }

        [HttpPost("login-google")]
        [AllowAnonymous]
        public ActionResult<IHttpResponseModel<AuthenticationTokenModel>> LoginGoogle([FromBody] GoogleAuthenticationModel loginInfo)
        {

            IEnumerable<Claim> userPrincipal = authService.GetPrincipalsFromGoogleToken(loginInfo.GoogleToken);

            foreach (var claim in userPrincipal)
            {
                Console.WriteLine($"{claim.Type} : {claim.Value}  ({claim.ValueType})");
            }

            string userEmail = userPrincipal.First(x => x.Type == "Email").Value;

            bool isValidUser = true;

            if (isValidUser)
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Message = "Login Successfully",
                    //Content = authService.GenerateTokens(user!)
                });
            }
            else
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Message = "Login Failed",
                    Detail = "Invalid user or username"
                });
            }

        }
    }
}
