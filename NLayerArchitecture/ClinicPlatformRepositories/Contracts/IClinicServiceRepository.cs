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
        ClinicServiceInfoModel? AddClinicService(ClinicServiceInfoModel clinicServiceInfo);
        IEnumerable<ClinicServiceInfoModel> GetAll(int clinicId);
        IEnumerable<ClinicServiceInfoModel> GetAll();
        ClinicServiceInfoModel? GetClinicServie(Guid clinicServiceId);
        ClinicServiceInfoModel? UpdateClinicService(ClinicServiceInfoModel serviceInfo);
        void DeleteClinicService(Guid clinicServiceId);

        // Service "Type"
        IEnumerable<ClinicServiceInfoModel> GetAllBaseService();
        ClinicServiceInfoModel? CreateBaseService(ClinicServiceInfoModel service);
        ClinicServiceInfoModel? GetBaseService(int serviceId);
        ClinicServiceInfoModel? UpdateBaseService(ClinicServiceInfoModel serviceId);
        void DeleteBaseService(int serviceId);
    }
}
