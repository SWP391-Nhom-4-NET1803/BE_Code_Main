using ClinicPlatformBusinessObject;
using ClinicPlatformDTOs.ClinicModels;
using ClinicPlatformDTOs.Comparator;
using ClinicPlatformRepositories;
using ClinicPlatformRepositories.Contracts;
using ClinicPlatformServices.Contracts;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

            if (GetClinicWithOwnerId((int) registrationInfo.OwnerId) != null)
            {
                message = "User already created clinic!";
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
            message = $"adding {registeredServices.Count()} services. ";

            foreach (var service in registeredServices)
            {
                message += $"Adding {service.ServiceId} for clinic {service.ClinicId}: ";

                if (clinicServiceRepository.AddClinicService(service))
                {
                    message += "Success. ";
                }
                else
                {
                    message += "Failed. ";
                }
            }

            message = "Successfully create new clinic.";
            return true;
        }

        public IEnumerable<ClinicInfoModel> GetAll()
        {
            return clinicRepository.GetAll();
        }

        public IEnumerable<ClinicInfoModel> GetClinicPaginated(int top = 0, int page = 0)
        {
            var result = clinicRepository.GetAll();

            if (page >= 0)
            {
                result = result.Skip((int) (top * page));
            }

            if (top > 0)
            {
                result = result.Take((int) top).ToList();
            }

            return result;
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
            var allService = clinicServiceRepository.GetAll().Where(x => x.ServiceId == serviceId).OrderBy(x => x.Price);

            var result = (from service in allService select GetClinicWithId((int) service.ClinicId!))
                .GroupBy(x => x.Id)
                .Select(x => x.First());
            return result;
        }

        public IEnumerable<ClinicInfoModel> GetClinicHasServices(IEnumerable<int> serviceId)
        {
            var allService = clinicServiceRepository.GetAll().Where(x => serviceId.Contains((int) x.ServiceId!));

            return (from service in allService select GetClinicWithId((int)service.ClinicId!)).Distinct();
        }

        public ClinicServiceInfoModel? GetClinicServiceWithName(string name)
        {
            return clinicServiceRepository.GetAll().Where(x => x.Name == name).FirstOrDefault();
        }

        public bool UpdateClinicInformation(ClinicInfoModel clinicInfo, out string message)
        {
            if (clinicRepository.UpdateClinic(clinicInfo) == null)
            {
                message = $"Error while trying to update clinic information! Update invoked for {clinicInfo.Name} ({clinicInfo.Id})";
                return false;
            };

            message = $"Updated clinic {clinicInfo.Name} information!";
            return true;
        }

        public bool AddClinicService(ClinicServiceInfoModel clinicService, out string message)
        {
            var allClinicServices = clinicServiceRepository.GetAll();

            message = $"Adding {clinicService.Name} for {clinicService.ClinicId}: ";

            if (!allClinicServices.Any(x => x.ServiceId == clinicService.ServiceId))
            {
                message += $" No base clinicService exist with Id {clinicService.ServiceId}";
                return false;
            }
            ClinicInfoModel? clinic = clinicRepository.GetClinic((int)clinicService.ClinicId!);

            message += $"Adding {clinicService.Name} for {clinicService.ClinicId}: ";

            if (clinicServiceRepository.AddClinicService(clinicService))
            {
                message += "Sucess. ";
                return true;
            }
            else
            {
                message += $"Failed{(clinic == null ? " (Cannot find clinic information)" : "")}. ";
                return false;
            }
        }

        public bool AddClinicService(IEnumerable<ClinicServiceInfoModel> clinicServices, out string message)
        {

            message = $"Adding {clinicServices.Count()} requested service! ";

            var available = clinicServiceRepository.GetAllBaseService();

            foreach (var service in clinicServices)
            {
                if (available.Any(x => x.ServiceId == service.ServiceId))
                {

                    ClinicInfoModel? clinic = clinicRepository.GetClinic((int) service.ClinicId!);

                    message += $"Adding {service.Name} for {service.ClinicId}: ";

                    if (clinicServiceRepository.AddClinicService(service))
                    {
                        message += "Sucess. ";
                    }
                    else
                    {
                        message += "Failed. ";
                        message += clinic == null ? " (Cannot find clinic information)" : "";
                    }
                }
                else
                {
                    message += $" No base service exist with Id {service.ServiceId}";
                }
                
            }

            return true;
        }

        public bool UpdateClinicService(ClinicServiceInfoModel clinicService, out string message)
        {
            if (clinicServiceRepository.UpdateClinicService(clinicService))
            {
                message = $"Success.";
                return true;
            }

            message = $"Error while updating information for {clinicService.ClinicServiceId}.";
            return false;
        }

        public bool AddClinicServices(IEnumerable<ClinicServiceInfoModel> clinicServices, out string message)
        {
            int countPassed = 0;
            int countFailed = 0;

            message = $"Found {clinicServices.Count()} service addition requests. ";

            foreach (var clinicService in clinicServices)
            {
                if (!AddClinicService(clinicService, out var tempt_message))
                {
                    countFailed++;
                    message += $"Failed while adding {clinicService.ServiceId}: {tempt_message} ";
                }
                else
                {
                    countPassed++;
                    message += $"Successfully added {clinicService.ServiceId}. ";
                }
            }

            message += $"Finished batch! {countPassed}/{clinicServices.Count()}. {countFailed} Failed add attempt!";
            return true;
        }

        public ClinicServiceInfoModel? GetClinicServiceWithId(Guid id)
        {
            return clinicServiceRepository.GetClinicServie(id);
        }

        public IEnumerable<ClinicServiceInfoModel> GetClinicServiceWithPirce(int id, long minimum, long maximum)
        {
            return clinicServiceRepository.GetAll().Where(x => x.ServiceId == id && minimum < x.Price && x.Price < maximum);
        }

        public bool UpdateClinicServices(IEnumerable<ClinicServiceInfoModel> clinicServices, out string message)
        {
            int countPassed = 0;
            int countFailed = 0;

            message = $"Found {clinicServices.Count()} service update requests. ";

            foreach (var clinicService in clinicServices)
            {
                if (!UpdateClinicService(clinicService, out var tempt_message))
                {
                    countFailed++;
                    message += $"Failed while updating {clinicService.ServiceId}: {tempt_message} ";
                }
                else
                {
                    countPassed++;
                    message += $"Successfully updating {clinicService.ServiceId}. ";
                }
            }

            message += $"Finished batch! {countPassed}/{clinicServices.Count()}. {countFailed} Failed add attempt!";
            return true;
        }

        public bool DeleteClinic(int clinicId)
        {
            clinicRepository.DeleteClinic(clinicId);

            if (clinicRepository.GetClinic(clinicId) != null)
            {
                return false;
            }

            return true;
        }

        public bool DeleteClinicServices(Guid clinicServiceId, out string message)
        {
            clinicServiceRepository.DeleteClinicService(clinicServiceId);

            if (clinicServiceRepository.GetClinicServie(clinicServiceId) != null)
            {
                message = $"Can not delete {clinicServiceId}.";
                return false;
            }

            message = $"Deleted service {clinicServiceId}.";
            return true;
        }

        public bool DeleteClinicServices(IEnumerable<Guid> clinicServiceId, out string message)
        {
            foreach (var id in clinicServiceId)
            {
                clinicServiceRepository.DeleteClinicService(id);
            }

            message = $"Deleted {clinicServiceId.Count()} service";
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
