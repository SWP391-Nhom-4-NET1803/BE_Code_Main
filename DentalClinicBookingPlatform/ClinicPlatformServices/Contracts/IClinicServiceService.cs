using ClinicPlatformDTOs.ClinicModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformServices.Contracts
{
    public interface IClinicServiceService: IDisposable
    {
        ClinicServiceInfoModel? GetClinicService(Guid serviceId);
        IEnumerable<ClinicServiceInfoModel> GetClinicServiceWithPrice(int serviceId, int minimum, int maximum);
        
        IEnumerable<ClinicServiceInfoModel> GetAllClinicService(int clinicId);

        ClinicServiceInfoModel? AddClinicService(ClinicServiceInfoModel model, out string message);
        bool AddClinicServices(IEnumerable<ClinicServiceInfoModel> clinicServices, out string message);
        ClinicServiceInfoModel? UpdateClinicService(ClinicServiceInfoModel clinicService, out string message);
        bool UpdateClinicServices(IEnumerable<ClinicServiceInfoModel> clinicServices, out string message);
        bool DeleteClinicServices(Guid clinicServiceId, out string message);
        bool DeleteClinicServices(IEnumerable<Guid> clinicServiceId, out string message);

        bool AddServiceCategory(ServiceCategoryModel service, out string message);
        IEnumerable<ServiceCategoryModel> GetAllCategory();
        ServiceCategoryModel? GetCategory(int serviceId);
        bool UpdateCategory(ServiceCategoryModel service, out string message);
    }
}
