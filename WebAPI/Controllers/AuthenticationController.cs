using Core.HttpModels;
using Core.HttpModels.ObjectModels;
using Core.Misc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Repositories;
using Repositories.Models;
using Services.EmailSerivce;
using Services.JwtManager;
using Services.TokenManager;
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
                    return Ok(token);
                }
                catch (Exception ex)
                {
                    return Ok(new HttpResponseModel() { StatusCode = 401, Message = ex.Message });
                }
            }
            else
            {
                return Ok(new HttpResponseModel() { StatusCode = 401, Message = "Username or Password is invalid." });
            }
        }

        [HttpPost]
        [Route("google")]
        [AllowAnonymous]
        public IActionResult LogUserInWithGoogle([FromBody] GoogleAuthModel Authtoken)
        {
            var service = HttpContext.RequestServices.GetService<IJwtTokenService>()!;

            var principals = service.GetPrincipalsFromGoogleToken(Authtoken.GoogleToken);

            foreach (var item in principals)
            {
                Console.WriteLine($"{item.Type} : {item.Value}");
            }

            User? user = _unitOfWork.UserRepository.GetUserWithEmail(principals.First(x => x.Type == "email").Value)!;

            if (user != null)
            {
                try
                {
                    var token = HttpContext.RequestServices.GetService<IJwtTokenService>()?.GenerateTokens(user);
                    return Ok(token);
                }
                catch (Exception ex)
                {
                    return Ok(new HttpResponseModel() { StatusCode = 500, Message = ex.Message });
                }
            }
            else
            {
                return Ok(new HttpResponseModel() { StatusCode = 401, Message = "This user does not exist in the system" });
            }
        }

        [HttpPost]
        [Route("logout")]
        [JwtTokenAuthorization]
        public IActionResult LogUserOut()
        {
            IJwtTokenService JwtService = HttpContext.RequestServices.GetService<IJwtTokenService>()!;

            string token = Request.Headers.Authorization.ToString();

            var claims = JwtService.GetPrincipalsFromToken(token);

            var userID = int.Parse(claims.Claims.FirstOrDefault(claim => claim.Type == "id")!.Value);

            User user = _unitOfWork.UserRepository.GetById(userID)!;

            // Hiện tại có thể làm một cách đơn giản đó là trả lại cho bên kia một cái RefreshToken hết hạn rồi kêu nó xài.
            AuthenticationToken newToken = new AuthenticationToken()
            {
                AccessToken = JwtService.GenerateAccessToken(user, 0),
                RefreshToken = JwtService.GenerateRefreshToken(user, 0)
            };

            // Đang tìm cách logout user, có một cách tạm thời đó là sử dụng một trường thuộc tính hoặc một bảng.
            // để ghi nhớ cái refreshToken hiện tại của người dùng trong database.

            return Ok(newToken);
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

            string[] refreshTokenParts = Encoding.UTF8.GetString(Convert.FromBase64String(refreshToken)).Split("|");

            Console.WriteLine(DateTime.Parse(refreshTokenParts[2]));
            Console.WriteLine(DateTime.UtcNow);
            Console.WriteLine(DateTime.Compare(DateTime.Parse(refreshTokenParts[2]), DateTime.UtcNow));

            if (DateTime.Compare(DateTime.Parse(refreshTokenParts[2]), DateTime.UtcNow) < 0)
            {
                return Ok(new HttpResponseModel() { StatusCode = 400, Message = "Refresh Token is expired" });
            }

            var principals = tokenService.GetPrincipalsFromToken(accessToken);
            User user = _unitOfWork.UserRepository.GetById(int.Parse(principals.Claims.First(claim => claim.Type == "id").Value))!;

            var token = HttpContext.RequestServices.GetService<IJwtTokenService>()?.GenerateTokens(user);
            return Ok(token);
        }

        [HttpGet]
        [Route("activate/{id}")]
        [AllowAnonymous]
        public IActionResult ActivateUserAccount(int id)
        {
            var user = _unitOfWork.UserRepository.GetById(id);
            if (user != null)
            {

                if (user.Status == true)
                {
                    return Ok(new HttpResponseModel() { StatusCode = 400, Message ="Invalid Request", Detail="This user account has been activated" });
                }

                user.Status = true;
                _unitOfWork.UserRepository.Update(user);
                _unitOfWork.Save();

                var emailService = HttpContext.RequestServices.GetService<IEmailService>()!;

                string emailBody = $"Xin chào người dùng! <br/>" +
                    $"Chúng tôi đã kích hoạt tài khoản cho email {user.Email}, cảm ơn bạn đã đăng kí dịch vụ của chúng tôi.<br>" +
                    $"Nếu bạn không phải là người đăng kí tài khoản trên, hãy truy cập vào [Link xóa tài khoản] để hủy việc tạo tài khoản của bạn. Chúc bạn có một ngày mới vui vẻ!";

                emailService.SendMailGoogleSmtp(target: user.Email, subject: "Thông báo kích hoạt tài khoản thành công", body: emailBody);

                return Ok(new HttpResponseModel() { StatusCode = 200, Message = "User account activated!" });
            };

            return Ok(new HttpResponseModel() { StatusCode = 400, Message = "User not found!" });
        }

        // ================================================== FOR TESTING PURPOSES ======================================================

        [HttpGet]
        [Route("check-login-admin")]
        [JwtTokenAuthorization(Roles: "Admin")]
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
