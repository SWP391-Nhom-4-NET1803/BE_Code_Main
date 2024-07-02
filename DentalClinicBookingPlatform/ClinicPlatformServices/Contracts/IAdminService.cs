using ClinicPlatformObjects.ReportModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformServices.Contracts
{
    public interface IAdminService: IDisposable
    {
        AdminSumarizedInfo GetSummaryReportOnDate(DateOnly date);
        AdminSumarizedInfo GetTodaySummaryReport();
        IEnumerable<AdminSumarizedInfo> GetReportInDateRange(DateOnly start, DateOnly end);
    }
}
