using ClinicPlatformDTOs.ClinicModels;
using ClinicPlatformDTOs.ClinicModels.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformServices.Contracts
{
    public interface IClinicService: IDisposable
    {
        bool RegisterClinic(ClinicRegistrationModel registrationInfo, out string message);

        ClinicInfoModel? GetClinicWithId(int id);

        IEnumerable<ClinicInfoModel> GetClinicNameStartWith(string prefix);

        IEnumerable<ClinicInfoModel> GetClinicWorkingInHourRange(TimeOnly start, TimeOnly end);

        ClinicInfoModel? GetClinicWithOwnerId(int ownerId);

        IEnumerable<ClinicInfoModel> GetClinicHasService(int serviceId);

        bool UpdateClinicInformation(ClinicInfoModel clinicInfo, out string message);

        public bool AddClinicService(ClinicServiceInfoModel clinicService, out string message);

        public bool AddClinicServices(IEnumerable<ClinicServiceInfoModel> clinicServices, out string message);

        bool DeleteClinic(int clinicId);

        bool InactivateClinic(int clinicId, out string message);

        bool ActivateClinic(int clinicId, out string message);

        bool IsClinicNameAvailable(string name);

        bool IsClinicCurrentlyOpen(int ClinicId, out string message);
    }
}
