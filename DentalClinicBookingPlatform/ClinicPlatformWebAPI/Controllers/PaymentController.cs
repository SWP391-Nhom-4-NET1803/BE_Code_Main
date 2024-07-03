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

namespace ClinicPlatformWebAPI.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private IPaymentService paymentService;
        private IBookingService bookingService;
        private string url = "http://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        private string returnUrl = "https://localhost:7163/api/payment/vnpay/success";
        private string tmCode = string.Empty;
        private string hashSecret = string.Empty;

        public PaymentController(IConfiguration configuration, IPaymentService paymentService, IBookingService bookingService)
        {
            this.paymentService = paymentService;
            this.bookingService = bookingService;
            tmCode = configuration.GetValue<string>("VNPay:TMCode")!;
            hashSecret = configuration.GetValue<string>("VNPay:VNPay")!;
        }


        [HttpPost]
        [Route("vnpay")]
        public ActionResult CreatePayment(VNPayInfoModel sentInfo)
        {
            IVNPayService pay = HttpContext.RequestServices.GetService<IVNPayService>()!;

            string paymentId = pay.CreateTransactionId();

            pay.AddRequestData("vnp_Version", "2.1.0");
            pay.AddRequestData("vnp_Command", "pay");
            pay.AddRequestData("vnp_TmnCode", tmCode);
            pay.AddRequestData("vnp_Amount", $"{sentInfo.amount}00");
            pay.AddRequestData("vnp_BankCode", "");
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", "VND");
            pay.AddRequestData("vnp_IpAddr", "127.0.0.1");
            pay.AddRequestData("vnp_Locale", "vn");
            pay.AddRequestData("vnp_OrderInfo", sentInfo.orderInfo);
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", returnUrl);
            pay.AddRequestData("vnp_TxnRef", paymentId);

            string paymentUrl = pay.CreateRequestUrl(url, hashSecret);

            var payment = paymentService.CreateNewPayment(Decimal.Parse(sentInfo.amount), paymentId, sentInfo.orderInfo, sentInfo.appointmentId, "VNPay", DateTime.UtcNow.AddHours(1), out var message);

            if (payment == null)
            {
                return BadRequest(new HttpResponseModel
                {
                    StatusCode = 400,
                    Message = message,
                    Success = false,
                    Content = paymentUrl,
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

        [HttpGet]
        [Route("vnpay/success")]
        public IActionResult PaymentConfirm()
        {
            if (Request.QueryString.HasValue)
            {
                var queryString = Request.QueryString.Value;
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

        private bool ValidateSignature(string rspraw, string inputHash, string secretKey)
        {
            string myChecksum = HttpContext.RequestServices.GetService<IVNPayService>()!.HmacSHA512(secretKey, rspraw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
