using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using ClinicPlatformDTOs.PayementModels;
using ClinicPlatformWebAPI.Services.VNPayService;
using ClinicPlatformServices.Contracts;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using System.Collections.Specialized;
using ClinicPlatformWebAPI.Helpers.Models;
using ClinicPlatformObjects.PayementModels;
using ClinicPlatformDTOs.BookingModels;
using Microsoft.IdentityModel.Tokens;

namespace ClinicPlatformWebAPI.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private IPaymentService paymentService;
        private IUserService userService;
        private IClinicService clinicService;
        private IBookingService bookingService;
        private string url = "http://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        private string returnUrl = "https://localhost:7163/api/payment/vnpay/return";
        private string tmCode = string.Empty;
        private string hashSecret = string.Empty;

        public PaymentController(IConfiguration configuration, IPaymentService paymentService, IBookingService bookingService, IUserService userService, IClinicService clinicService)
        {
            var return_url = configuration.GetValue<string>("Frontend:PaymentSuccessPage");

            this.paymentService = paymentService;
            this.bookingService = bookingService;
            this.userService = userService;
            this.returnUrl = return_url == null || return_url.Length == 0 ? returnUrl : return_url;
            tmCode = configuration.GetValue<string>("VNPay:TMCode")!;
            hashSecret = configuration.GetValue<string>("VNPay:VNPay")!;
            this.clinicService = clinicService;
        }


        [HttpPost("vnpay")]
        public ActionResult CreatePayment(VNPayInfoModel sentInfo)
        {
            IVNPayService pay = HttpContext.RequestServices.GetService<IVNPayService>()!;

            string hostName = System.Net.Dns.GetHostName();
            string clientIPAddress = System.Net.Dns.GetHostAddresses(hostName).GetValue(0)!.ToString()!;
            string paymentId = pay.CreateTransactionId();

            var booking = bookingService.GetBooking(sentInfo.appointmentId);

            if (booking == null)
            {
                return BadRequest(new HttpResponseModel
                {
                    StatusCode = 400,
                    Success = false,
                    Message = "Can not find the appointment information"
                });
            }

            if (booking.Status != "pending")
            {
                return BadRequest(new HttpResponseModel
                {
                    StatusCode = 400,
                    Success = false,
                    Message = $"Can not create new payment for this appointment. Because the appointment is already {booking.Status}."
                });
            }

            pay.AddRequestData("vnp_Version", "2.1.0");
            pay.AddRequestData("vnp_Command", "pay");
            pay.AddRequestData("vnp_TmnCode", tmCode);
            pay.AddRequestData("vnp_Amount", $"{booking.AppointmentFee}00");
            pay.AddRequestData("vnp_BankCode", "");
            pay.AddRequestData("vnp_CreateDate", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_ExpireDate", DateTime.UtcNow.AddHours(1).ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", "VND");
            pay.AddRequestData("vnp_IpAddr", clientIPAddress);
            pay.AddRequestData("vnp_Locale", "vn");
            pay.AddRequestData("vnp_OrderInfo", sentInfo.orderInfo);
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", returnUrl);
            pay.AddRequestData("vnp_TxnRef", paymentId);

            string paymentUrl = pay.CreateRequestUrl(url, hashSecret);

            var payment = paymentService.CreateNewPayment(booking.AppointmentFee, paymentId, sentInfo.orderInfo, sentInfo.appointmentId, "VNPay", DateTime.UtcNow.AddHours(1), out var message);

            if (payment == null)
            {
                return BadRequest(new HttpResponseModel
                {
                    StatusCode = 400,
                    Message = message,
                    Success = false,
                });
            }

            return Ok( new HttpResponseModel
            {
                StatusCode = 200,
                Message = message,
                Success = true,
                Content = paymentUrl,
            });
        }

        [HttpGet("vnpay/success")]
        public IActionResult PaymentConfirm()
        {
            if (Request.QueryString.HasValue)
            {
                var queryString = Request.QueryString.Value;
                Console.WriteLine(queryString.ToString());
                var json = HttpUtility.ParseQueryString(queryString);

                string orderId = json["vnp_TxnRef"]!;
                //string orderInformaion = json["vnp_OrderInfo"].ToString();
                //long vnpayTranId = Convert.ToInt64(json["vnp_TransactionNo"]);
                string vnp_ResponseCode = json["vnp_ResponseCode"]!.ToString();
                string vnp_SecureHash = json["vnp_SecureHash"]!.ToString(); //hash của dữ liệu trả về
                var pos = Request.QueryString.Value.IndexOf("&vnp_SecureHash");

                bool checkSignature = ValidateSignature(Request.QueryString.Value.Substring(1, pos - 1), vnp_SecureHash, hashSecret); //check chữ ký đúng hay không?
                if (checkSignature && tmCode == json["vnp_TmnCode"]!.ToString())
                {
                    PaymentInfoModel? payment = paymentService.GetPayment(orderId);

                    if (payment == null)
                    {
                        return BadRequest(new HttpResponseModel
                        {
                            StatusCode = 400,
                            Success = false,
                            Message = "Can not find payment information",
                        });
                    }

                    if (vnp_ResponseCode == "00")
                    {
                        payment = paymentService.SetPaymentStatusToCompleted(orderId, out var message);

                        if (payment == null)
                        {
                            return BadRequest(new HttpResponseModel
                            {
                                StatusCode = 400,
                                Success = false,
                                Message = message
                            });
                        }

                        AppointmentInfoModel? appointment = bookingService.SetAppoinmentStatus(payment.AppointmentId, "booked", out message);

                        if (appointment == null)
                        {
                            return BadRequest(new HttpResponseModel
                            {
                                StatusCode = 400,
                                Success = false,
                                Message = message
                            });
                        }

                        return Ok(new HttpResponseModel
                        {
                            StatusCode = 200,
                            Success = true,
                            Message = message,
                            Content = payment
                        });
                    }
                    else
                    {
                        BadRequest(new HttpResponseModel
                        {
                            StatusCode = 400,
                            Success = false,
                            Message = $"Payment failed with error code {vnp_ResponseCode}"
                        });
                    }
                }
                else
                {
                    return BadRequest(new HttpResponseModel
                    {
                        StatusCode = 400,
                        Success = false,
                        Message = "Invalid Signature"
                    });
                }
            }

            return BadRequest(new HttpResponseModel
            {
                StatusCode = 400,
                Success = false,
                Message = "Invalid request"
            });
        }

        /// <summary>
        ///  This is an example of how (what) to process when VNPay return the transaction result.
        ///  We just need to forward the query string to the preference endpoint for further processing.
        /// </summary>
        /// <returns>action result of the operation. In this case, since it a redirect, the return value will be based on vnpay/success endpoint.</returns>
        [HttpGet("vnpay/return")]
        public IActionResult PaymentReturn()
        {
            return Redirect(@"https://localhost:7163/api/payment/vnpay/success" + Request.QueryString);
        }

        [HttpGet("customer/{id}")]
        public IActionResult GetCustomerPayment(int id)
        {
            var user = userService.GetUserWithCustomerId(id);
            if (user == null)
            {
                return BadRequest(new HttpResponseModel
                {
                    StatusCode = 400,
                    Success = false,
                    Message = "Can not find customer with provided Id",
                });
            }

            return Ok(new HttpResponseModel
            {
                StatusCode = 200,
                Success = true,
                Message = "Success",
                Content = paymentService.GetPaymentOfCustomer(id),
            });
        }

        [HttpGet("clinic/{id}")]
        public IActionResult GetClinicPayment(int id)
        {
            if (clinicService.GetClinicWithId(id) == null)
            {
                return BadRequest(new HttpResponseModel
                {
                    StatusCode = 400,
                    Success = false,
                    Message = "Can not find clinic with provided Id"
                });
            }

            var result = paymentService.GetAllClinicAppointmentPayment(id);

            return Ok(new HttpResponseModel
            {
                StatusCode = 200,
                Success = true,
                Message = $"Found {result.Count()} result",
                Content = result
            });
        }

        private bool ValidateSignature(string rspraw, string inputHash, string secretKey)
        {
            string myChecksum = HttpContext.RequestServices.GetService<IVNPayService>()!.HmacSHA512(secretKey, rspraw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
