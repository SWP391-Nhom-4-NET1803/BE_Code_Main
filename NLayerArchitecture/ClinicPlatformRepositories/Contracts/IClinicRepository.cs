using ClinicPlatformDTOs.ClinicModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformRepositories.Contracts
{
    public interface IClinicRepository: IDisposable
    {
        IEnumerable<ClinicInfoModel> GetAll();

        ClinicInfoModel? GetClinic(int clinicId);

        ClinicInfoModel? AddClinc(ClinicInfoModel clinicInfo);

        ClinicInfoModel? UpdateClinic(ClinicInfoModel clinicInfo);

        void DeleteClinic(int clinicId);

        void SaveChanges();

        // Mappers
    }
}
