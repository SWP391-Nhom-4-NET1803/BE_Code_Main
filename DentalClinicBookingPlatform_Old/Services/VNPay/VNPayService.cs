using Core.Misc;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Services.VNPay
{
    public class VNPayService: IVNPayService
    {
        private SortedList<String, String> _requestData = new SortedList<String, String>(new VNPayCompare());
        
        public void AddRequestData(string key, string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            StringBuilder data = new StringBuilder();

            // Thêm vào chuỗi query các tham số có giá trị xác định (dựa trên đầu vào)
            foreach (KeyValuePair<string, string> kv in _requestData)
            {
                if (!String.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }

            // Tạo chuỗi query và áp vào baseUrl để tạo request URL hoàn chỉnh.
            string queryString = data.ToString();

            baseUrl += "?" + queryString;

            // Xóa bớt dấu '&' dư thừa ở cuối Request URL sau bước thêm các tham số.
            String signData = queryString;
            if (signData.Length > 0)
            {
                signData = signData.Remove(data.Length - 1, 1);
            }

            // Tạo chuỗi khóa mã hóa bằng hàm băm sử dụng mã khóa cung cấp bởi vnpay và request URL hoàn chỉnh
            string vnp_SecureHash =  this.HmacSHA512(vnp_HashSecret, signData);
            
            // Bước cuối cùng là dùng chuỗi khóa mã hóa để 
            baseUrl += "vnp_SecureHash=" + vnp_SecureHash;

            return baseUrl;
        }

        /// <summary>
        ///  <para>Hàm để tạo chuỗi khóa mã hóa sử dụng hàm băm SHA512.</para>
        /// </summary>
        /// <param name="key">Khóa băm</param>
        /// <param name="inputData">Dữ liệu đầu vào</param>
        /// <returns>Chuỗi đã được "mã hóa"</returns>
        public String HmacSHA512(string key, String inputData)
        {
            var hash = new StringBuilder();
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }
    }
}
