using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformWebAPI.Services.VNPayService
{
    public interface IVNPayService
    {
        public void AddRequestData(string key, string value);

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret);

        public string CreateTransactionId();

        public string HmacSHA512(string key, string inputData);

    }
}
