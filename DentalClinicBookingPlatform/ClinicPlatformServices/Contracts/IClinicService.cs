using ClinicPlatformDTOs.ClinicModels;

namespace ClinicPlatformServices.Contracts
{
    public interface IClinicService : IDisposable
    {
        bool RegisterClinic(ClinicInfoModel registrationInfo, out string message);
        IEnumerable<ClinicInfoModel> GetClinicWorkingInHourRange(TimeOnly start, TimeOnly end);
        ClinicInfoModel? GetClinicWithId(int id);
        IEnumerable<ClinicInfoModel> GetAllClinic(int size, int page);
        ClinicInfoModel? GetClinicWithName(string name);
        ClinicInfoModel? GetClinicWithOwnerId(int ownerId);
        bool UpdateClinicInformation(ClinicInfoModel clinicInfo, out string message);
        bool InactivateClinic(int clinicId, out string message);
        bool ActivateClinic(int clinicId, out string message);
        bool IsClinicNameAvailable(string name);
        bool IsClinicCurrentlyWorking(int ClinicId, out string message);
        bool DeleteClinic(int clinicId);

        

        ClinicServiceInfoModel? GetClinicServiceWithId(Guid id);
        ClinicServiceInfoModel? GetClinicServiceWithName(string name);
        ClinicServiceInfoModel? GetClinicOnCategory(int categoryId);
        IEnumerable<ClinicServiceInfoModel> GetClinicServiceWithPirce(int id,long maximum, long minimum=0);
        
        bool AddClinicService(ClinicServiceInfoModel clinicService, out string message);
        bool UpdateClinicService(ClinicServiceInfoModel clinicService, out string message);
        bool DisableClinicService(Guid clinicServiceId, out string message);
        bool EnableClinicService(Guid clinicServiceId, out string message);
        bool DeleteClinicServices(Guid clinicServiceId, out string message);
    }
}
