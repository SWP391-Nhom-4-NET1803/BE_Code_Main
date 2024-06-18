using ClinicPlatformBusinessObject;
using ClinicPlatformDTOs.ClinicModels;
using ClinicPlatformDTOs.ClinicModels.Registration;
using ClinicPlatformRepositories;
using ClinicPlatformRepositories.Contracts;
using ClinicPlatformServices.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformServices
{
    public class PlatformClinicService: IClinicService
    {
        private readonly IClinicRepository clinicRepository;
        private readonly IClinicServiceRepository clinicServiceRepository;
        private bool disposedValue;

        public PlatformClinicService(IClinicRepository clinicRepo, IClinicServiceRepository serviceRepo) 
        {
            clinicRepository = clinicRepo;
            clinicServiceRepository = serviceRepo;
        }

        public bool RegisterClinic(ClinicRegistrationModel registrationInfo, out string message)
        {
            if (registrationInfo.OwnerId == null)
            {
                message = "No user id was provied for this method";
                return false;
            }

            ClinicInfoModel? clinicInfo = new ClinicInfoModel()
            {
                Address = registrationInfo.Address,
                Name = registrationInfo.Name,
                Description = registrationInfo.Description,
                Email = registrationInfo.Email,
                Phone = registrationInfo.Phone,
                OwnerId = registrationInfo.OwnerId,
                OpenHour = registrationInfo.OpenHour,
                CloseHour = registrationInfo.CloseHour,
                Status = false
            };

            if (clinicRepository.AddClinc(clinicInfo) == null)
            {
                message = "Failed! Unknown error while insert clinic info to database.";
                return false;
            }

            clinicInfo = GetClinicWithOwnerId((int)clinicInfo.OwnerId!)!;

            var registeredServices = from service in clinicServiceRepository.GetAllBaseService()
                                     join serviceId in registrationInfo.ClinicServices! on service.ServiceId equals serviceId
                                     select new ClinicServiceInfoModel
                                     {
                                         ClinicId = clinicInfo.Id,
                                         Price = 0,
                                         ServiceId = serviceId,
                                     };

            foreach( var service in registeredServices )
            {
                if (clinicServiceRepository.AddClinicService(service) == null)
                {
                    message = $"Error while trying to register clinic {service.Name} for clinic {clinicInfo.Name}.";
                    return false;
                }
            }

            message = "Successfully create new clinic.";
            return true;
        }

        public ClinicInfoModel? GetClinicWithId(int id)
        {
            return clinicRepository.GetClinic(id);
        }

        public IEnumerable<ClinicInfoModel> GetClinicNameStartWith(string prefix)
        {
            return clinicRepository.GetAll().Where(x => x.Name!.StartsWith(prefix,true, System.Globalization.CultureInfo.InvariantCulture) );
        }

        public IEnumerable<ClinicInfoModel> GetClinicWorkingInHourRange(TimeOnly start, TimeOnly end)
        {
            return clinicRepository.GetAll().Where(x => x.OpenHour > start && x.CloseHour < end);
        }

        public ClinicInfoModel? GetClinicWithOwnerId(int ownerId)
        {
            return clinicRepository.GetAll().Where(x => x.OwnerId == ownerId).FirstOrDefault();
        }

        public IEnumerable<ClinicInfoModel> GetClinicHasService(int serviceId)
        {
            throw new NotImplementedException();
        }

        public bool UpdateClinicInformation(ClinicInfoModel clinicInfo, out string message)
        {
            if (clinicRepository.UpdateClinic(clinicInfo) == null)
            {
                message = $"Error while trying to update clinic information! Update invoked for {clinicInfo.Name} ({clinicInfo.Id})";
                return false;
            };

            foreach (var clinicService in clinicInfo.ClinicServices)
            {
                if (clinicServiceRepository.UpdateClinicService(clinicService) == null)
                {
                    message = $"Error while trying to update clinic service information!\n Update invoked for {clinicService.Name} ({clinicService.ClinicServiceId})";
                    return false;
                }
            }   

            message = $"Updated clinic {clinicInfo.Name} information!";
            return true;
        }

        public bool AddClinicService(ClinicServiceInfoModel clinicService, out string message)
        {
            var allClinicServices = clinicServiceRepository.GetAll();

            if (allClinicServices.Any(x => x.ClinicId == clinicService.ClinicId && x.ServiceId == clinicService.ServiceId))
            {
                message = "You already added this ";
                return false;
            }

            if (clinicServiceRepository.AddClinicService(clinicService) == null)
            {
                message = $"Error while trying to update clinic service information!\n Update invoked for {clinicService.Name} ({clinicService.ClinicServiceId})";
                return false;
            }

            message = "Success";
            return true;
        }

        public bool AddClinicServices(IEnumerable<ClinicServiceInfoModel> clinicServices, out string message)
        {
            

            foreach (var clinicService in clinicServices)
            {
                if (!AddClinicService(clinicService, out message))
                {
                    return false;
                }
            }

            message = $"Updated clinic service information!";
            return true;
        }

        public bool DeleteClinic(int clinicId)
        {
            clinicRepository.DeleteClinic(clinicId);

            return true;
        }

        public bool InactivateClinic(int clinicId, out string message)
        {
            var clinic = clinicRepository.GetClinic(clinicId);

            if (clinic == null)
            {
                message = "Clinic not found";
                return false;
            }

            if (!clinic.Status ?? false)
            {
                message = "The clinic is already inactive";
                return false;
            }

            clinic.Status = false;

            if (clinicRepository.UpdateClinic(clinic) == null)
            {
                message = "Error while updating clinic status";
                return false;
            }

            message = "Successfully deactivated clinic";
            return true;
        }

        public bool ActivateClinic(int clinicId, out string message)
        {
            var clinic = clinicRepository.GetClinic(clinicId);

            if (clinic == null)
            {
                message = "Clinic not found";
                return false;
            }

            if (clinic.Status ?? true)
            {
                message = "The clinic is already active";
                return false;
            }

            clinic.Status = true;

            if (clinicRepository.UpdateClinic(clinic) == null)
            {
                message = "Error while updating clinic status";
                return false;
            }

            message = "Successfully activated clinic";
            return true;
        }

        public bool IsClinicNameAvailable(string name)
        {
            return clinicRepository.GetAll().Where(x => x.Name!.Equals(name, StringComparison.CurrentCultureIgnoreCase)).Any();
        }

        public bool IsClinicCurrentlyOpen(int ClinicId, out string message)
        {
            ClinicInfoModel? clinic = clinicRepository.GetClinic(ClinicId);

            if (clinic == null)
            {
                message = "Clinic not exist";
                return false;
            }
            DateTime now = DateTime.Now;

            message = "Operation Success";
            return clinic.OpenHour < TimeOnly.FromDateTime(now) && TimeOnly.FromDateTime(now) < clinic.CloseHour ;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    clinicRepository.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
