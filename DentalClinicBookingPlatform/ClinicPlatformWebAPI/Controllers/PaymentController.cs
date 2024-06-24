using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using ClinicPlatformDTOs.PayementModels;
using ClinicPlatformWebAPI.Services.VNPayService;
using ClinicPlatformServices.Contracts;

namespace ClinicPlatformWebAPI.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private IPaymentService paymentService;
        private string url = "http://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        private string returnUrl = $"https://localhost:7163/vnpayAPI/PaymentConfirm";
        private string tmCode = string.Empty;
        private string hashSecret = string.Empty;

        public PaymentController(IConfiguration configuration, IPaymentService paymentService)
        {
            this.paymentService = paymentService;
            tmCode = configuration.GetValue<string>("VNPay:TMCode")!;
            hashSecret = configuration.GetValue<string>("VNPay:VNPay")!;
        }


        [HttpPost]
        [Route("vnpay")]
        public ActionResult CreatePayment(VNPayInfoModel sentInfo)
        {
            string hostName = System.Net.Dns.GetHostName();
            string clientIPAddress = System.Net.Dns.GetHostAddresses(hostName).GetValue(0)!.ToString()!;

            IVNPayService pay = HttpContext.RequestServices.GetService<IVNPayService>()!;

            pay.AddRequestData("vnp_Version", "2.1.0");
            pay.AddRequestData("vnp_Command", "pay"); //Mã API sử dụng, mã cho giao dịch thanh toán là 'pay'
            pay.AddRequestData("vnp_TmnCode", tmCode);
            pay.AddRequestData("vnp_Amount", sentInfo.amount);
            pay.AddRequestData("vnp_BankCode", ""); //Mã Ngân hàng thanh toán (tham khảo: https://sandbox.vnpayment.vn/apis/danh-sach-ngan-hang/), có thể để trống, người dùng có thể chọn trên cổng thanh toán VNPAY
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", "VND");
            pay.AddRequestData("vnp_IpAddr", clientIPAddress);
            pay.AddRequestData("vnp_Locale", "vn");
            pay.AddRequestData("vnp_OrderInfo", sentInfo.info);
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", returnUrl);
            pay.AddRequestData("vnp_TxnRef", sentInfo.order_info); //mã hóa đơn

            // Database operation

            // TODO: Create database operations (such as storing payment details)

            string paymentUrl = pay.CreateRequestUrl(url, hashSecret);
            return Redirect(paymentUrl);
        }

        [HttpGet]
        [Route("vnpay/payment-confirm")]
        public IActionResult PaymentConfirm()
        {
            if (Request.QueryString.HasValue)
            {
                //lấy toàn bộ dữ liệu trả về
                var queryString = Request.QueryString.Value;
                var json = HttpUtility.ParseQueryString(queryString);

                foreach (var item in json)
                {
                    Console.WriteLine($" {item.ToString()}");
                }

                //long orderId = Convert.ToInt64(json["vnp_TxnRef"]); //mã hóa đơn
                //string orderInfor = json["vnp_OrderInfo"]!.ToString(); //Thông tin giao dịch
                //long vnpayTranId = Convert.ToInt64(json["vnp_TransactionNo"]); //mã giao dịch tại hệ thống VNPAY
                string vnp_ResponseCode = json["vnp_ResponseCode"]!.ToString(); //response code: 00 - thành công, khác 00 - xem thêm https://sandbox.vnpayment.vn/apis/docs/bang-ma-loi/
                string vnp_SecureHash = json["vnp_SecureHash"]!.ToString(); //hash của dữ liệu trả về
                var pos = Request.QueryString.Value.IndexOf("&vnp_SecureHash");

                //return Ok(Request.QueryString.Value.Substring(1, pos-1) + "\n" + vnp_SecureHash + "\n"+ PayLib.HmacSHA512(hashSecret, Request.QueryString.Value.Substring(1, pos-1)));
                bool checkSignature = ValidateSignature(Request.QueryString.Value.Substring(1, pos - 1), vnp_SecureHash, hashSecret); //check chữ ký đúng hay không?

                if (checkSignature && tmCode == json["vnp_TmnCode"]!.ToString())
                {
                    if (vnp_ResponseCode == "00")
                    {
                        //Thanh toán thành công
                        return Redirect("[Thay đổi điều hướng]");
                    }
                    else
                    {
                        //Thanh toán không thành công. Mã lỗi: vnp_ResponseCode
                        return Redirect("[Thay đổi điều hướng]");
                    }
                }
                else
                {
                    //phản hồi không khớp với chữ ký
                    return Redirect("[Thay đổi điều hướng]");
                }
            }
            //phản hồi không hợp lệ
            return Redirect("[Thay đổi điều hướng]");
        }

        private bool ValidateSignature(string rspraw, string inputHash, string secretKey)
        {
            string myChecksum = HttpContext.RequestServices.GetService<IVNPayService>()!.HmacSHA512(secretKey, rspraw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
