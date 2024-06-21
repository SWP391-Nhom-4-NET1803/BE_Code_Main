using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.PayementModels
{
    public class VNPayCompare : IComparer<string>
    {
        /// <summary>
        ///  Model giúp sắp xếp lại các query parameter trong chuỗi quẻy khi gửi vể VNPay.
        /// </summary>
        /// <param name="x">biến query thứ nhất</param>
        /// <param name="y">biến query thứ hai</param>
        /// <returns>
        ///     <list type="bullet">0 nếu cả hai đều bằng nhau (và bằng null)</list>
        ///     <list type="bullet">-1 nếu biến thứ nhất lớn hơn (về mặt kí tự)</list>
        ///     <list type="bullet">1 nếu biến thứ nhất  nhỏ hơn (về mặt kí tự)</list>
        /// </returns>
        public int Compare(string? x, string? y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            var vnpCompare = CompareInfo.GetCompareInfo("en-US");

            return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
        }
    }
}
