using ClinicPlatformDTOs.ClinicModels;

namespace ClinicPlatformServices.Contracts
{
    public interface IClinicService : IDisposable
    {
        bool RegisterClinic(ClinicRegistrationModel registrationInfo, out string message);

        ClinicInfoModel? GetClinicWithId(int id);

        IEnumerable<ClinicInfoModel> GetClinicNameStartWith(string prefix);

        IEnumerable<ClinicInfoModel> GetClinicWorkingInHourRange(TimeOnly start, TimeOnly end);

        ClinicInfoModel? GetClinicWithOwnerId(int ownerId);

        IEnumerable<ClinicInfoModel> GetClinicHasService(int serviceId);

        bool UpdateClinicInformation(ClinicInfoModel clinicInfo, out string message);

        public bool AddClinicService(ClinicServiceInfoModel clinicService, out string message);

        public ClinicServiceInfoModel? GetClinicServiceWithId(Guid id);

        public IEnumerable<ClinicServiceInfoModel> GetClinicServiceWithPirce(int id, long minimum, long maximum);

        public bool AddClinicServices(IEnumerable<ClinicServiceInfoModel> clinicServices, out string message);

        public bool UpdateClinicService(ClinicServiceInfoModel clinicService, out string message);

        public bool UpdateClinicServices(IEnumerable<ClinicServiceInfoModel> clinicServices, out string message);

        bool DeleteClinic(int clinicId);

        public bool DeleteClinicServices(Guid clinicServiceId, out string message);

        public bool DeleteClinicServices(IEnumerable<Guid> clinicServiceId, out string message);

        bool InactivateClinic(int clinicId, out string message);

        bool ActivateClinic(int clinicId, out string message);

        bool IsClinicNameAvailable(string name);

        bool IsClinicCurrentlyOpen(int ClinicId, out string message);
    }
}
