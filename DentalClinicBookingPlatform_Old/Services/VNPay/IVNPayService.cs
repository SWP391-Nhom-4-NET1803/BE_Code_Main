using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Services.VNPay
{
    public interface IVNPayService
    {
        public void AddRequestData(string key, string value);

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret);

        public String HmacSHA512(string key, String inputData);
        
    }
}
