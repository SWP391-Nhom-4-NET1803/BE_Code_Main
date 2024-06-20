using ClinicPlatformDTOs.ClinicModels;

namespace ClinicPlatformServices.Contracts
{
    public interface IClinicService : IDisposable
    {
        bool RegisterClinic(ClinicRegistrationModel registrationInfo, out string message);

        ClinicInfoModel? GetClinicWithId(int id);

        IEnumerable<ClinicInfoModel> GetAll();

        IEnumerable<ClinicInfoModel> GetClinicPaginated(int top, int page);

        IEnumerable<ClinicInfoModel> GetClinicHasServices(IEnumerable<int> serviceId);

        IEnumerable<ClinicInfoModel> GetClinicNameStartWith(string prefix);

        IEnumerable<ClinicInfoModel> GetClinicWorkingInHourRange(TimeOnly start, TimeOnly end);

        ClinicInfoModel? GetClinicWithOwnerId(int ownerId);

        IEnumerable<ClinicInfoModel> GetClinicHasService(int serviceId);

        bool UpdateClinicInformation(ClinicInfoModel clinicInfo, out string message);

        bool AddClinicService(ClinicServiceInfoModel clinicService, out string message);

        ClinicServiceInfoModel? GetClinicServiceWithId(Guid id);

        public ClinicServiceInfoModel? GetClinicServiceWithName(string name);

        IEnumerable<ClinicServiceInfoModel> GetClinicServiceWithPirce(int id, long minimum, long maximum);

        bool AddClinicServices(IEnumerable<ClinicServiceInfoModel> clinicServices, out string message);

        bool UpdateClinicService(ClinicServiceInfoModel clinicService, out string message);

        bool UpdateClinicServices(IEnumerable<ClinicServiceInfoModel> clinicServices, out string message);

        bool DeleteClinic(int clinicId);

        bool DeleteClinicServices(Guid clinicServiceId, out string message);

        bool DeleteClinicServices(IEnumerable<Guid> clinicServiceId, out string message);

        bool InactivateClinic(int clinicId, out string message);

        bool ActivateClinic(int clinicId, out string message);

        bool IsClinicNameAvailable(string name);

        bool IsClinicCurrentlyOpen(int ClinicId, out string message);
    }
}
