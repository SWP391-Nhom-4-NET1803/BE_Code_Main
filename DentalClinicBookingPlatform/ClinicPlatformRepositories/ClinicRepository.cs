using ClinicPlatformDatabaseObject;
using ClinicPlatformDTOs.ClinicModels;
using ClinicPlatformRepositories.Contracts;

namespace ClinicPlatformRepositories
{
    public class ClinicRepository : IClinicRepository
    {
        private readonly DentalClinicPlatformContext context;
        private bool disposedValue;

        public ClinicRepository(DentalClinicPlatformContext context)
        {
            this.context = context;
        }

        public ClinicInfoModel? AddClinc(ClinicInfoModel clinicInfo)
        {
            Clinic clinic = MapToClinic(clinicInfo);

            context.Clinics.Add(clinic);
            context.SaveChanges();

            return MapToClinicInfo(clinic);
        }

        public ClinicInfoModel? UpdateClinic(ClinicInfoModel clinicInfo)
        {
            Clinic? clinic = context.Clinics.Find(clinicInfo.Id);

            if (clinic != null)
            {
                clinic.OwnerId = clinicInfo.OwnerId;
                clinic.Name = clinicInfo.Name;
                clinic.Address = clinicInfo.Address;
                clinic.Email = clinicInfo.Email;
                clinic.Phone = clinicInfo.Phone;
                clinic.Description = clinicInfo.Description;
                clinic.Working = clinicInfo.Working;
                clinic.OpenHour = clinicInfo.OpenHour;
                clinic.CloseHour = clinicInfo.CloseHour;
                clinic.Status = clinicInfo.Status;

                context.Clinics.Update(clinic);
                context.SaveChanges();
                return MapToClinicInfo(clinic);
            }

            return null;
        }

        public IEnumerable<ClinicInfoModel> GetAllClinic(bool includeRemoved = true, bool includeUnverified = true)
        {
            IEnumerable<ClinicInfoModel> result = from clinic in context.Clinics.ToList() select MapToClinicInfo(clinic);

            if (!includeRemoved) 
            {
                result = result.Where(x => x.Status != "removed");
            }

            if(!includeUnverified)
            {
                result = result.Where(x => x.Status != "unverified");
            }

            return result;
        }

        public ClinicInfoModel? GetClinic(int clinicId)
        {
            var result = context.Clinics.Find(clinicId);

            if (result != null)
            {
                return MapToClinicInfo(result);
            }

            return null;
        }

        public void DeleteClinic(int clinicId)
        {
            Clinic? clinic = context.Clinics.Find(clinicId);

            if ( clinic != null )
            {
                clinic.Working = false;
                clinic.Status = "removed";
                context.Update(clinicId);
                context.SaveChanges();
            }
            

            
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    context.Dispose();
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
        private Clinic MapToClinic(ClinicInfoModel clinicInfo)
        {
            return new Clinic
            {
                ClinicId = clinicInfo.Id,
                Name = clinicInfo.Name,
                Address = clinicInfo.Address,
                Description = clinicInfo.Description,
                Email = clinicInfo.Email,
                Phone = clinicInfo.Phone,
                OpenHour = clinicInfo.OpenHour,
                CloseHour = clinicInfo.CloseHour,
                Status = clinicInfo.Status,
                OwnerId = clinicInfo.OwnerId,
                Working = clinicInfo.Working,
            };
        }

        private ClinicInfoModel MapToClinicInfo(Clinic clinic)
        {
            return new ClinicInfoModel
            {
                Id = clinic.ClinicId,
                Name = clinic.Name,
                Description = clinic.Description,
                Address = clinic.Address!,
                Email = clinic.Email!,
                Phone = clinic.Phone!,
                OpenHour = (TimeOnly)clinic.OpenHour!,
                CloseHour = (TimeOnly)clinic.CloseHour!,
                OwnerId = clinic.OwnerId,
                Status = clinic.Status,
                Working = clinic.Working
            };
        }
    }
}
