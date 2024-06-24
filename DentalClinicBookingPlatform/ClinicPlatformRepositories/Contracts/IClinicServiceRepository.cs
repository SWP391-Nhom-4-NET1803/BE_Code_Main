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
        IEnumerable<ClinicServiceInfoModel> GetAllClinicService();
        ClinicServiceInfoModel? GetClinicService(Guid clinicServiceId);
        ClinicServiceInfoModel? UpdateClinicService(ClinicServiceInfoModel serviceInfo);
        bool DeleteClinicService(Guid clinicServiceId);

        // Service "Type"
        IEnumerable<ServiceCategoryModel> GetAllServiceCategory();
        ServiceCategoryModel? GetServiceCategory(int categoryId);
        ServiceCategoryModel? AddServiceCategory(ServiceCategoryModel category);
        ServiceCategoryModel? UpdateServiceCategory(ServiceCategoryModel categoryId);
        bool DeleteServiceCategory(int categoryId);
    }
}
