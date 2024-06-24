using ClinicPlatformDTOs.ClinicModels;
using ClinicPlatformDTOs.Comparator;
using ClinicPlatformRepositories;
using ClinicPlatformRepositories.Contracts;
using ClinicPlatformServices.Contracts;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformServices
{
    public class PlatformClinicService : IClinicService
    {
        private readonly IClinicRepository clinicRepository;
        private readonly IClinicServiceRepository clinicServiceRepository;
        private readonly IUserRepository userRepository;
        private bool disposedValue;

        public PlatformClinicService(IClinicRepository clinicRepo, IClinicServiceRepository serviceRepo, IUserRepository userRepository)
        {
            clinicRepository = clinicRepo;
            clinicServiceRepository = serviceRepo;
            this.userRepository = userRepository;
        }

        public ClinicInfoModel? RegisterClinic(ClinicInfoModel registrationInfo, out string message)
        {

            if (GetClinicWithOwnerId(registrationInfo.OwnerId) != null)
            {
                message = "User already created clinic!";
                return null;
            }

            if (userRepository.GetUser(registrationInfo.OwnerId) == null)
            {
                message = "Error, can not get user!";
                return null;
            }

            ClinicInfoModel? clinic = clinicRepository.AddClinc(registrationInfo);

            if (clinic == null)
            {
                message = "Failed! Unknown error while insert clinic info to database.";
                return null;
            }

            message = "Successfully create new clinic.";
            return clinic;
        }

        public IEnumerable<ClinicInfoModel> GetAllClinic(int top = 0, int page = 0)
        {
            var result = clinicRepository.GetAllClinic();

            if (page >= 0)
            {
                result = result.Skip(top * page);
            }

            if (top > 0)
            {
                result = result.Take(top).ToList();
            }

            return result;
        }

        public ClinicInfoModel? GetClinicWithId(int id)
        {
            return clinicRepository.GetClinic(id);
        }

        public IEnumerable<ClinicInfoModel> GetClinicWithName(string prefix)
        {
            return clinicRepository.GetAllClinic().Where(x => x.Name.Contains(prefix));
        }

        public IEnumerable<ClinicInfoModel> GetClinicWorkingInHourRange(TimeOnly start, TimeOnly end)
        {
            return clinicRepository.GetAllClinic().Where(x => x.OpenHour > start && x.CloseHour < end);
        }

        public ClinicInfoModel? GetClinicWithOwnerId(int ownerId)
        {
            return clinicRepository.GetAllClinic().Where(x => x.OwnerId == ownerId).FirstOrDefault();
        }

        public IEnumerable<ClinicInfoModel> GetClinicHasService(int categoryId)
        {
            var allService = clinicServiceRepository.GetAllClinicService().Where(x => x.CategoryId == categoryId);

            return (from service in allService select GetClinicWithId((int)service.ClinicId!)).Distinct();
        }

        public IEnumerable<ClinicServiceInfoModel> GetClinicServicesWithName(string name)
        {
            return clinicServiceRepository.GetAllClinicService().Where(x => x.Name! == name);
        }

        public IEnumerable<ClinicServiceInfoModel> GetClinicServiceNameContain(string sub)
        {
            return clinicServiceRepository.GetAllClinicService().Where(x => x.Name!.Contains(sub, StringComparison.OrdinalIgnoreCase));

        }

        public ClinicServiceInfoModel? GetClinicServiceOfType(int categoryId)
        {
            return clinicServiceRepository.GetAllClinicService().Where(x => x.CategoryId == categoryId).FirstOrDefault();
        }

        public ClinicInfoModel? UpdateClinicInformation(ClinicInfoModel clinicInfo, out string message)
        {
            var clinic = clinicRepository.UpdateClinic(clinicInfo);
            if (clinic == null)
            {
                message = $"Error while trying to update clinic information! Update invoked for {clinicInfo.Name} ({clinicInfo.Id})";
                return null;
            };

            message = $"Updated clinic {clinicInfo.Name} information!";
            return clinic;
        }

        public bool AddClinicService(ClinicServiceInfoModel clinicService, out string message)
        {
            var allClinicServices = clinicServiceRepository.GetAllServiceCategory();

            message = $"Adding {clinicService.Name} for {clinicService.ClinicId}: ";

            if (!allClinicServices.Any(x => x.Id == clinicService.CategoryId))
            {
                message += $" No category exist with Id {clinicService.CategoryId}";
                return false;
            }
            ClinicInfoModel? clinic = clinicRepository.GetClinic((int)clinicService.ClinicId!);

            if (clinic == null)
            {
                message += $"Failed{(clinic == null ? " (Cannot find clinic information)" : "")}. ";
                return false;
            }
            else
            {
                message += "Sucess.";
                return clinicServiceRepository.AddClinicService(clinicService) != null;
            }
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
                    message += $"Failed while adding {clinicService.Name}: {tempt_message} ";
                }
                else
                {
                    countPassed++;
                    message += $"Successfully added {clinicService.Name}. ";
                }
            }

            message += $"Finished batch! {countPassed}/{clinicServices.Count()}. {countFailed} Failed add attempt!";
            return true;
        }


        public bool UpdateClinicService(ClinicServiceInfoModel clinicService, out string message)
        {
            if (clinicServiceRepository.UpdateClinicService(clinicService) != null)
            {
                message = $"Success.";
                return true;
            }

            message = $"Error while updating information for {clinicService.ClinicServiceId}.";
            return false;
        }


        public ClinicServiceInfoModel? GetClinicServiceWithId(Guid id)
        {
            return clinicServiceRepository.GetClinicService(id);
        }

        public IEnumerable<ClinicServiceInfoModel> GetClinicServiceWithPirce(int id, long minimum, long maximum)
        {
            return clinicServiceRepository.GetAllClinicService().Where(x => x.CategoryId == id && minimum < x.Price && x.Price < maximum);
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
                    message += $"Failed while updating {clinicService.ClinicServiceId}: {tempt_message} ";
                }
                else
                {
                    countPassed++;
                    message += $"Successfully updating {clinicService.ClinicServiceId}. ";
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

            if (clinicServiceRepository.GetClinicService(clinicServiceId) != null)
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

            if (!clinic.Working)
            {
                message = "The clinic is already inactive";
                return false;
            }

            clinic.Working = false;

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

            if (clinic.Working)
            {
                message = "The clinic is already active";
                return false;
            }

            clinic.Working= true;

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
            return clinicRepository.GetAllClinic().Where(x => x.Name!.Equals(name, StringComparison.CurrentCultureIgnoreCase)).Any();
        }

        public bool IsClinicCurrentlyWorking(int ClinicId, out string message)
        {
            ClinicInfoModel? clinic = clinicRepository.GetClinic(ClinicId);

            if (clinic == null)
            {
                message = "Clinic not exist";
                return false;
            }
            DateTime now = DateTime.Now;

            message = "Operation Success";
            return clinic.OpenHour < TimeOnly.FromDateTime(now) && TimeOnly.FromDateTime(now) < clinic.CloseHour;
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

        public bool AddBaseService(ServiceCategoryModel service, out string message)
        {
            if (clinicServiceRepository.GetAllServiceCategory().Any(x => x.Name == service.Name))
            {
                message = "There is another service with this name exist";
                return false;
            }

            if (clinicServiceRepository.AddServiceCategory(service))
            {
                message = "Added new service successfully";
                return true;
            }

            message = "Failed";
            return false;
        }

        public IEnumerable<ServiceCategoryModel> GetBaseServices()
        {
            throw new NotImplementedException();
        }

        public ServiceCategoryModel? GetBaseService(int categoryId)
        {
            return clinicServiceRepository.GetServiceCategory(categoryId);
        }

        public ServiceCategoryModel? GetCategoryWithId(string name)
        {
            return clinicServiceRepository.GetAllServiceCategory().Where(x => x.Name == name).FirstOrDefault();
        }

        public ServiceCategoryModel? GetCategoryWithName(string name)
        {
            return clinicServiceRepository.GetAllServiceCategory().Where(x => x.Name == name).FirstOrDefault();
        }

        public IEnumerable<ClinicInfoModel> FilterClinicList(IEnumerable<ClinicInfoModel> clinics, TimeOnly? from = null, TimeOnly? to = null, bool includeInactive = false, bool includeRemoved = false, string? includeService = null)
        {
            if (includeService != null)
            {
                var service = GetCategoryWithName(includeService);

                if (service != null)
                {
                    var clinic = GetClinicHasService(service.Id);

                    clinics = clinics.Where(x => clinic.Any(y => y.Id == x.Id));
                }
            }

            if (from != null)
            {
                clinics = clinics.Where(x => x.OpenHour >= from);
            }

            if (to != null)
            {
                clinics = clinics.Where(x => x.CloseHour <= to);
            }

            if (!includeInactive)
            {
                clinics = clinics.Where(x => x.Working);
            }

            if (!includeRemoved)
            {
                clinics = clinics.Where(x => x.Status != "removed");
            }

            

            return clinics;
        }

        ClinicInfoModel? IClinicService.GetClinicWithName(string name)
        {
            throw new NotImplementedException();
        }

        public ClinicServiceInfoModel? GetClinicServiceWithName(string name)
        {
            throw new NotImplementedException();
        }

        public ClinicServiceInfoModel? GetClinicOnCategory(int categoryId)
        {
            throw new NotImplementedException();
        }

        public bool DisableClinicService(Guid clinicServiceId, out string message)
        {
            throw new NotImplementedException();
        }

        public bool EnableClinicService(Guid clinicServiceId, out string message)
        {
            throw new NotImplementedException();
        }
    }
}
