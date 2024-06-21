using Core.HttpModels;
using Core.HttpModels.ObjectModels.AuthenticationModels;
using Core.HttpModels.ObjectModels.RegistrationModels;
using Core.HttpModels.ObjectModels.RoleModels;
using Core.HttpModels.ObjectModels.UserModel;
using Core.Misc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Repositories;
using Repositories.Models;
using Services.EmailSerivce;
using Services.JwtManager;
using Services.TokenManager;
using Services.UserService;
using System.Net;
using System.Security.Claims;
using System.Text;
using WebAPI.Helper.AuthorizationPolicy;


namespace WebAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly UnitOfWork _unitOfWork;

        public AuthenticationController(IConfiguration configuration, DentalClinicPlatformContext context)
        {
            _config = configuration;
            _unitOfWork = new UnitOfWork(context);
        }

        // ================================== Tested and ready to deploy ==============================================

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public IActionResult LogUserIn([FromBody] UserAuthenticationRequestModel requestObject)
        {
            User? user = _unitOfWork.UserRepository.Authenticate(requestObject.UserName, requestObject.Password);

            if (user != null)
            {
                try
                {
                    var token = HttpContext.RequestServices.GetService<IJwtTokenService>()?.GenerateTokens(user);
                    return Ok(new HttpResponseModel() { StatusCode = 200, Message = "Authorized", Content = token });
                }
                catch (Exception ex)
                {
                    return Ok(new HttpResponseModel() { StatusCode = 401, Message = "Unauthorized", Detail=ex.Message });
                }
            }
            else
            {
                return BadRequest(new HttpResponseModel() { StatusCode = 401, Message = "Unauthorized", Detail="Username or Password is invalid." });
            }
        }

        [HttpPost]
        [Route("google")]
        [AllowAnonymous]
        public async Task<IActionResult> LogUserInWithGoogle([FromBody] GoogleAuthModel Authtoken)
        {
            var tokenService = HttpContext.RequestServices.GetService<IJwtTokenService>()!;
            var userService = HttpContext.RequestServices.GetService<IUserService>()!;

            var principals = tokenService.GetPrincipalsFromGoogleToken(Authtoken.GoogleToken);

            User? user = _unitOfWork.UserRepository.GetUserWithEmail(principals.First(x => x.Type == "email").Value)!;

            if (user == null)
            {
                UserRegistrationModel registration = new UserRegistrationModel()
                {
                    Username = principals.First(x => x.Type == "email").Value,
                    Password = userService.CreatePassword(10),
                    Email = principals.First(x => x.Type == "email").Value
                };

                if (!userService.CreateCustomer(registration, out var message))
                {
                    return BadRequest(new HttpResponseModel() { StatusCode = 500, Message = "Internal Server Error", Content = message });
                };
                _unitOfWork.Save();

                var emailService = HttpContext.RequestServices.GetService<IEmailService>()!;

                string body = $"Xin chào người dùng! <br/>" +
                    $"Chúng tôi đã nhận được yêu cầu tạo tài khoản cho email {principals.First(x => x.Type == "email").Value}, cảm ơn bạn đã đăng kí dịch vụ của chúng tôi. <br/>" +
                    $"Vui lòng xác thực tài khoản thông qua cổng xác thực của chúng tôi tại [Tạo trang xác thực bên phía front-end call tới api xác thực phía backend]";

                await emailService.SendMailGoogleSmtp(principals.First(x => x.Type == "email").Value, "Xác nhận yêu cầu tạo tài khoản người dùng", body);

                user = _unitOfWork.UserRepository.GetUserWithEmail(principals.First(x => x.Type == "email").Value)!;
            }

            var token = HttpContext.RequestServices.GetService<IJwtTokenService>()?.GenerateTokens(user);
            return Ok(new HttpResponseModel() { StatusCode = 200, Message = "Authorized", Content = token });

        }

        [HttpPost]
        [Route("logout")]
        [JwtTokenAuthorization]
        public IActionResult LogUserOut()
        {
            IJwtTokenService JwtService = HttpContext.RequestServices.GetService<IJwtTokenService>()!;

            string token = Request.Headers.Authorization.ToString();

            if (token.IsNullOrEmpty())
            {
                return BadRequest(new HttpResponseModel() {StatusCode=401, Message="Unauthorized", Detail="User is not authorized!"}); 
            }

            var claims = JwtService.GetPrincipalsFromToken(token);

            var userID = int.Parse(claims.Claims.FirstOrDefault(claim => claim.Type == "id")!.Value);

            User user = _unitOfWork.UserRepository.GetById(userID)!;

            // Hiện tại có thể làm một cách đơn giản đó là trả lại cho bên kia một cái RefreshToken hết hạn.
            AuthenticationToken newToken =  JwtService.GenerateTokens(user, 0, 0);

            return Ok(new HttpResponseModel() { StatusCode = 200, Message = "Authorized", Content=newToken });
        }

        [HttpPost]
        [Route("refresh")]
        [AllowAnonymous]
        public IActionResult RefreshToken([FromBody] AuthenticationToken tokens)
        {
            var tokenService = HttpContext.RequestServices.GetService<IJwtTokenService>()!;

            // Getting tokens from the request body for validation and new key generation.
            string accessToken = tokens.AccessToken;
            string refreshToken = tokens.RefreshToken;

            try
            {
                string[] refreshTokenParts = Encoding.UTF8.GetString(Convert.FromBase64String(refreshToken)).Split("|");

                if (DateTime.Compare(DateTime.Parse(refreshTokenParts[2]), DateTime.UtcNow) < 0)
                {
                    return BadRequest(new HttpResponseModel() { StatusCode = 400, Message = "Refresh Token is expired" });
                }

                var principals = tokenService.GetPrincipalsFromToken(accessToken);

                Claim userIdClaim = principals.Claims.First(claim => claim.Type == "id");

                User user = _unitOfWork.UserRepository.GetById(int.Parse(userIdClaim.Value))!;

                var token = tokenService.GenerateTokens(user);

                return Ok(new HttpResponseModel() { StatusCode = 200, Message = "Authorized", Content = token });
            }
            catch (Exception ex)
            {
                return BadRequest(new HttpResponseModel() { StatusCode = 400, Message = "Error while refreshing the user tokens", Detail = ex.Message });
            }

        }

        [HttpGet]
        [Route("activate/{id}")]
        [AllowAnonymous]
        public IActionResult ActivateUserAccount(int userId)
        {


            var user = _unitOfWork.UserRepository.GetById(userId);

            if (user != null)
            {

                if (user.Status == true)
                {
                    return Ok(new HttpResponseModel() { StatusCode = 400, Message = "Invalid Request", Detail = "This user account has been activated" });
                }

                user.Status = true;
                _unitOfWork.UserRepository.Update(user);
                _unitOfWork.Save();

                var emailService = HttpContext.RequestServices.GetService<IEmailService>()!;

                string emailSubject = "Thông báo kích hoạt tài khoản thành công";

                string emailBody = $"Xin chào người dùng! <br/>" +
                    $"Chúng tôi đã kích hoạt tài khoản cho email {user.Email}, cảm ơn bạn đã đăng kí dịch vụ của chúng tôi.<br>" +
                    $"Nếu bạn không phải là người đăng kí tài khoản trên, hãy truy cập vào [Link xóa tài khoản] để hủy việc tạo tài khoản của bạn. Chúc bạn có một ngày mới vui vẻ!";

                emailService.SendMailGoogleSmtp(target: user.Email, subject: emailSubject, body: emailBody);

                return Ok(new HttpResponseModel() { StatusCode = 200, Message = "User account activated!" });
            };

            return BadRequest(new HttpResponseModel() { StatusCode = 400, Message = "Invalid request", Detail = "Can not find specified user" });
        }

        // ==================================== Finished and untested  =================================================

        // ===========================================  Unfinished =====================================================



        // ================================================== FOR TESTING PURPOSES ======================================================

        [HttpGet]
        [Route("check-login-admin")]
        [JwtTokenAuthorization(Roles: RoleModel.Roles.Admin)]
        public ActionResult CheckLoginAdmin()
        {
            return Ok(new { message = "Authorized", time = DateTime.UtcNow });
        }

        [HttpGet]
        [Route("check-login-user")]
        [JwtTokenAuthorization]
        public ActionResult CheckLogin()
        {
            return Ok(new { message = "Authorized", time = DateTime.UtcNow });
        }
    }
}
