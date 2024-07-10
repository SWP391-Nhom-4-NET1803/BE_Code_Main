using ClinicPlatformDTOs.ClinicModels;

namespace ClinicPlatformServices.Contracts
{
    public interface IClinicService : IDisposable
    {
        ClinicInfoModel? RegisterClinic(ClinicInfoModel registrationInfo, out string message);
        IEnumerable<ClinicInfoModel> GetClinicWorkingInHourRange(TimeOnly start, TimeOnly end);
        ClinicInfoModel? GetClinicWithId(int id);
        IEnumerable<ClinicInfoModel> GetAllClinic(int size, int page);
        IEnumerable<ClinicInfoModel> GetVerifiedClinics();
        IEnumerable<ClinicInfoModel> GetUnverifiedClinics();
        IEnumerable<ClinicInfoModel> GetClinicWithName(string name);
        ClinicInfoModel? GetClinicWithOwnerId(int ownerId);
        ClinicInfoModel? UpdateClinicInformation(ClinicInfoModel clinicInfo, out string message);
        ClinicInfoModel? InactivateClinic(int clinicId, out string message);
        ClinicInfoModel? ActivateClinic(int clinicId, out string message);
        bool IsClinicNameAvailable(string name);
        bool IsClinicCurrentlyWorking(int ClinicId, out string message);
        bool DeleteClinic(int clinicId);


        ClinicServiceCategoryModel? AddServiceCategory(ClinicServiceCategoryModel service, out string message);
        ClinicServiceCategoryModel? UpdateServiceCategory(ClinicServiceCategoryModel service, out string message);
        IEnumerable<ClinicServiceCategoryModel> GetServiceCategories();


        ClinicServiceInfoModel? GetClinicServiceWithId(Guid id);
        IEnumerable<ClinicServiceInfoModel> GetClinicServiceWithName(int clinicId, string name);
        ClinicServiceInfoModel? GetClinicOnCategory(int categoryId);
        IEnumerable<ClinicServiceInfoModel> GetClinicServiceWithPirce(int id,long maximum, long minimum=0);
        
        ClinicServiceInfoModel? AddClinicService(ClinicServiceInfoModel clinicService, out string message);
        ClinicServiceInfoModel? UpdateClinicService(ClinicServiceInfoModel clinicService, out string message);
        ClinicServiceInfoModel? DisableClinicService(Guid clinicServiceId, out string message);
        ClinicServiceInfoModel? EnableClinicService(Guid clinicServiceId, out string message);
        bool DeleteClinicServices(Guid clinicServiceId, out string message);
    }
}
