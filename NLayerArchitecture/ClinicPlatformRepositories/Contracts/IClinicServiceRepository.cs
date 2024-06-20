using ClinicPlatformDTOs.ClinicModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformRepositories.Contracts
{
    public interface IClinicServiceRepository: IDisposable
    {
        // ClincServices
        bool AddClinicService(ClinicServiceInfoModel clinicServiceInfo);
        IEnumerable<ClinicServiceInfoModel> GetAll();
        ClinicServiceInfoModel? GetClinicServie(Guid clinicServiceId);
        bool UpdateClinicService(ClinicServiceInfoModel serviceInfo);
        bool DeleteClinicService(Guid clinicServiceId);

        // Service "Type"
        IEnumerable<ClinicServiceInfoModel> GetAllBaseService();
        ClinicServiceInfoModel? GetBaseService(int serviceId);
        bool AddBaseService(ClinicServiceInfoModel service);
        bool UpdateBaseService(ClinicServiceInfoModel serviceId);
        bool DeleteBaseService(int serviceId);
    }
}
