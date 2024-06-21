using ClinicPlatformDTOs.ClinicModels;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.Comparator
{
    public class ClinicServiceComparer : IEqualityComparer<ClinicServiceInfoModel>
    {
        public bool Equals(ClinicServiceInfoModel? x, ClinicServiceInfoModel? y)
        {
            return x == null || y == null ? false : GetHashCode(x) == GetHashCode(y);
        }

        public int GetHashCode([DisallowNull] ClinicServiceInfoModel obj)
        {
            return obj.ClinicId.GetHashCode() * 10 + obj.ServiceId.GetHashCode();
        }
    }
}
