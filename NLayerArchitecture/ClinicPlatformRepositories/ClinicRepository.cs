using ClinicPlatformBusinessObject;
using ClinicPlatformDAOs;
using ClinicPlatformDTOs.ClinicModels;
using ClinicPlatformRepositories.Contracts;

namespace ClinicPlatformRepositories
{
    public class ClinicRepository : IClinicRepository
    {
        private readonly ClinicDAO clinicDAO;
        private bool disposedValue;

        public ClinicRepository()
        {
            clinicDAO = new ClinicDAO();
        }

        public ClinicInfoModel? AddClinc(ClinicInfoModel clinicInfo)
        {
            Clinic clinic = new()
            {
                Name = clinicInfo.Name!,
                Description = clinicInfo.Description,
                Address = clinicInfo.Address!,
                Email = clinicInfo.Email!,
                Phone = clinicInfo.Phone!,
                OpenHour = (TimeOnly)clinicInfo.OpenHour!,
                CloseHour = (TimeOnly)clinicInfo.CloseHour!,
                OwnerId = (int) clinicInfo.OwnerId!,
                Status = clinicInfo.Status ?? false,
            };

            if (clinicDAO.AddClinic(clinic) == null)
            {
                return null;
            }

            return clinicInfo;
        }

        public ClinicInfoModel? UpdateClinic(ClinicInfoModel clinicInfo)
        {
            Clinic? target = clinicDAO.GetClinic(clinicInfo.Id);
            

            if (target != null)
            {
                target.Name = clinicInfo.Name ?? target.Name;
                target.Address = clinicInfo.Address ?? target.Address;
                target.Description = clinicInfo.Description ?? target.Description;
                target.Phone = clinicInfo.Phone ?? target.Phone;
                target.OpenHour = clinicInfo.OpenHour ?? target.OpenHour;
                target.OwnerId = clinicInfo.OwnerId ?? target.OwnerId;
                target.Status = clinicInfo.Status ?? target.Status;

                clinicDAO.UpdateClinic(target);
                SaveChanges();

                return MapFromClinicToClinicModel(target);
            }

            return null;
        }

        public IEnumerable<ClinicInfoModel> GetAll()
        {
            var mapped = from clinic in clinicDAO.GetAllClinic()
                         select new ClinicInfoModel
                         {
                             Id = clinic.ClinicId,
                             Address = clinic.Address,
                             Name = clinic.Name,
                             Description = clinic.Description,
                             Email = clinic.Email,
                             Phone = clinic.Phone,
                             OpenHour = clinic.OpenHour,
                             CloseHour = clinic.CloseHour,
                             OwnerId = clinic.OwnerId,
                             Status = clinic.Status,
                         };

            return mapped;
        }

        public ClinicInfoModel? GetClinic(int clinicId)
        {
            var result = clinicDAO.GetClinic(clinicId);

            if (result != null)
            {
                return new ClinicInfoModel()
                {
                    Id = clinicId,
                    Name = result.Name,
                    Address = result.Address,
                    Description = result.Description,
                    Email = result.Email,
                    Phone = result.Phone,
                    OpenHour = result.OpenHour,
                    CloseHour = result.CloseHour,
                    Status = result.Status,
                    OwnerId = result.OwnerId,
                };
            }

            return null;
        }

        public void DeleteClinic(int clinicId)
        {
            clinicDAO.DeleteClinic(clinicId);
        }

        public void SaveChanges()
        {
            clinicDAO.SaveChanges();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    clinicDAO.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        // Mappers
        private static ClinicInfoModel MapFromClinicToClinicModel(Clinic clinic)
        {
            return new ClinicInfoModel()
            {
                Name = clinic.Name,
                Description = clinic.Description,
                Address = clinic.Address!,
                Email = clinic.Email!,
                Phone = clinic.Phone!,
                OpenHour = (TimeOnly)clinic.OpenHour!,
                CloseHour = (TimeOnly)clinic.CloseHour!,
                OwnerId = clinic.OwnerId,
                Status = clinic.Status,
            };
        }
    }
}
