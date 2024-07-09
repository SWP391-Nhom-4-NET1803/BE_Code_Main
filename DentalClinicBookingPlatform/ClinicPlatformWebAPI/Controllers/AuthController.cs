using ClinicPlatformDTOs.AuthenticationModels;
using ClinicPlatformDTOs.UserModels;
using ClinicPlatformServices.Contracts;
using ClinicPlatformWebAPI.Helpers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace ClinicPlatformWebAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    [AllowAnonymous]
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
                    Success = true,
                    Message = "Login Successfully",
                    Content = authService.GenerateTokens(user!)
                });
            }
            else
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Success = false,
                    Message = "Invalid user or username"
                });
            }

        }


        [HttpPost("refresh")]
        public ActionResult<IHttpResponseModel<AuthenticationTokenModel>> RefreshToken([FromBody] AuthenticationTokenModel tokens)
        {
            try
            {
                string[] refreshTokenParts = Encoding.UTF8.GetString(Convert.FromBase64String(tokens.RefreshToken)).Split("|");

                if (DateTime.Compare(DateTime.Parse(refreshTokenParts[2]), DateTime.Now) < 0)
                {
                    return BadRequest(new HttpResponseModel
                    { 
                        StatusCode = 400, 
                        Success = false,
                        Message = "Refresh Token is expired" 
                    });
                }

                var principals = authService.GetPrincipalsFromToken(tokens.AccessToken);

                Claim userIdClaim = principals.Claims.First(claim => claim.Type == "id");

                UserInfoModel user = userService.GetUserWithUserId(int.Parse(userIdClaim.Value))!;

                var token = authService.GenerateTokens(user);

                return Ok(new HttpResponseModel() { StatusCode = 200, Message = "Authorized", Content = token });
            }
            catch (Exception ex)
            {
                return BadRequest(new HttpResponseModel() { 
                    StatusCode = 400, 
                    Success = false,
                    Message = ex.Message });
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

            string userEmail = userPrincipal.First(x => x.Type == "email").Value;

            UserInfoModel? user = userService.GetUserWithEmail(userEmail);

            if (user == null)
            {
                string username = "User" + userPrincipal.First(x => x.Type == "nbf").Value;

                user = userService.RegisterAccount(new UserInfoModel { Username=username, Email = userEmail, PasswordHash=username }, "Customer", out var message);

                if (user == null)
                {
                    return BadRequest(new HttpResponseModel
                    { 
                        StatusCode = 500, 
                        Success = false,
                        Message = "Internal Server Error", 
                        Content = message });
                };
            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Success = true,
                Message = "Login Successfully",
                Content = authService.GenerateTokens(user!)
            });
        }
    }
}
