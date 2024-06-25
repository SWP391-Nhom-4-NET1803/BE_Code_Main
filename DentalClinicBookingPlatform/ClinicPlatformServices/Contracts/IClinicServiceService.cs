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
        bool RemoveClinicServices(Guid clinicServiceId, out string message);

        bool AddServiceCategory(ClinicServiceCategoryModel service, out string message);
        IEnumerable<ClinicServiceCategoryModel> GetAllCategory();
        ClinicServiceCategoryModel? GetCategory(int serviceId);
        bool UpdateCategory(ClinicServiceCategoryModel service, out string message);
    }
}
