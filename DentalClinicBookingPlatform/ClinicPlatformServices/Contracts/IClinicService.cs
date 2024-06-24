using ClinicPlatformDTOs.ClinicModels;

namespace ClinicPlatformServices.Contracts
{
    public interface IClinicService : IDisposable
    {
        ClinicInfoModel? RegisterClinic(ClinicInfoModel registrationInfo, out string message);
        IEnumerable<ClinicInfoModel> GetClinicWorkingInHourRange(TimeOnly start, TimeOnly end);
        ClinicInfoModel? GetClinicWithId(int id);
        IEnumerable<ClinicInfoModel> GetAllClinic(int size, int page);
        IEnumerable<ClinicInfoModel> GetClinicWithName(string name);
        ClinicInfoModel? GetClinicWithOwnerId(int ownerId);
        ClinicInfoModel? UpdateClinicInformation(ClinicInfoModel clinicInfo, out string message);
        bool InactivateClinic(int clinicId, out string message);
        bool ActivateClinic(int clinicId, out string message);
        bool IsClinicNameAvailable(string name);
        bool IsClinicCurrentlyWorking(int ClinicId, out string message);
        bool DeleteClinic(int clinicId);


        ServiceCategoryModel? AddServiceCategory(ServiceCategoryModel service, out string message);
        ServiceCategoryModel? UpdateServiceCategory(ServiceCategoryModel service, out string message);
        IEnumerable<ServiceCategoryModel> GetServiceCategories();


        ClinicServiceInfoModel? GetClinicServiceWithId(Guid id);
        IEnumerable<ClinicServiceInfoModel> GetClinicServiceWithName(int clinicId, string name);
        ClinicServiceInfoModel? GetClinicOnCategory(int categoryId);
        IEnumerable<ClinicServiceInfoModel> GetClinicServiceWithPirce(int id,long maximum, long minimum=0);
        
        ClinicServiceInfoModel? AddClinicService(ClinicServiceInfoModel clinicService, out string message);
        ClinicServiceInfoModel? UpdateClinicService(ClinicServiceInfoModel clinicService, out string message);
        bool DisableClinicService(Guid clinicServiceId, out string message);
        bool EnableClinicService(Guid clinicServiceId, out string message);
        bool DeleteClinicServices(Guid clinicServiceId, out string message);
    }
}
