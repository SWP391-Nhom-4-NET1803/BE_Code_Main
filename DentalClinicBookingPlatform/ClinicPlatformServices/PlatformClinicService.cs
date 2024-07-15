using Azure;
using ClinicPlatformDTOs.ClinicModels;
using ClinicPlatformDTOs.Comparator;
using ClinicPlatformDTOs.UserModels;
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

            foreach (var clinic in result)
            {
                clinic.OwnerName = userRepository.GetUser(clinic.OwnerId)!.Fullname;
            }

            return result;
        }

        public IEnumerable<ClinicInfoModel> GetVerifiedClinics()
        {
            var result = clinicRepository.GetAllClinic(true, false, false);

            foreach (var clinic in result)
            {
                clinic.OwnerName = userRepository.GetUser(clinic.OwnerId)!.Fullname;
            }

            return result;
        }
        public IEnumerable<ClinicInfoModel> GetUnverifiedClinics()
        {
            var result = clinicRepository.GetAllClinic(false, false, true);

            foreach (var clinic in result)
            {
                clinic.OwnerName = userRepository.GetUser(clinic.OwnerId)!.Fullname;
            }

            return result;
        }

        public ClinicInfoModel? GetClinicWithId(int id)
        {
            ClinicInfoModel? result = clinicRepository.GetClinic(id);

            if (result != null)
            {
                result.OwnerName = userRepository.GetUser(result.OwnerId)!.Fullname;
            }

            return result;
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

        public ClinicServiceInfoModel? AddClinicService(ClinicServiceInfoModel clinicService, out string message)
        {
            var allClinicServices = clinicServiceRepository.GetAllServiceCategory();

            message = $"Adding {clinicService.Name} for {clinicService.ClinicId}: ";

            if (!allClinicServices.Any(x => x.Id == clinicService.CategoryId))
            {
                message += $" No category exist with Id {clinicService.CategoryId}";
                return null;
            }
            ClinicInfoModel? clinic = clinicRepository.GetClinic((int)clinicService.ClinicId!);

            if (clinic == null)
            {
                message += $"Failed{(clinic == null ? " (Cannot find clinic information)" : "")}. ";
                return null;
            }
            else
            {
                message += "Sucess.";
                return clinicServiceRepository.AddClinicService(clinicService);
            }
        }

        public bool AddClinicServices(IEnumerable<ClinicServiceInfoModel> clinicServices, out string message)
        {
            int countPassed = 0;
            int countFailed = 0;

            message = $"Found {clinicServices.Count()} service addition requests. ";

            foreach (var clinicService in clinicServices)
            {
                if (AddClinicService(clinicService, out var tempt_message) == null)
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


        public ClinicServiceInfoModel? UpdateClinicService(ClinicServiceInfoModel clinicService, out string message)
        {
            var serviceInfo = clinicServiceRepository.UpdateClinicService(clinicService);

            if (serviceInfo != null)
            {
                message = $"Success.";
                return serviceInfo;
            }

            message = $"Error while updating information for {clinicService.ClinicServiceId}.";
            return null;
        }


        public ClinicServiceInfoModel? GetClinicServiceWithId(Guid id)
        {
            return clinicServiceRepository.GetClinicService(id);
        }

        public IEnumerable<ClinicServiceInfoModel> GetClinicServiceWithPirce(int id, long minimum, long maximum)
        {
            return clinicServiceRepository.GetAllClinicService().Where(x => x.CategoryId == id && minimum < x.Price && x.Price < maximum);
        }

        public bool DeleteClinic(int clinicId)
        {
            ClinicInfoModel? clinic = clinicRepository.GetClinic(clinicId);

            if (clinic != null)
            {
                clinic.Status = "removed";

                clinic = clinicRepository.UpdateClinic(clinic);

                if (clinic != null)
                {
                    return true;
                }
            }   

            return false;
        }

        public ClinicInfoModel? InactivateClinic(int clinicId, out string message)
        {
            var clinic = clinicRepository.GetClinic(clinicId);

            if (clinic == null)
            {
                message = "Clinic not found";
                return null;
            }

            if (!clinic.Working)
            {
                message = "The clinic is already inactive";
                return null;
            }

            clinic.Working = false;

            if (clinicRepository.UpdateClinic(clinic) == null)
            {
                message = "Error while updating clinic status";
                return null;
            }

            message = "Successfully deactivated clinic";
            return clinic;
        }

        public ClinicInfoModel? ActivateClinic(int clinicId, out string message)
        {
            var clinic = clinicRepository.GetClinic(clinicId);

            if (clinic == null)
            {
                message = "Clinic not found";
                return null;
            }

            if (clinic.Working)
            {
                message = "The clinic is already active";
                return null;
            }

            clinic.Working= true;

            if (clinicRepository.UpdateClinic(clinic) == null)
            {
                message = "Error while updating clinic status";
                return null;
            }

            message = "Successfully activated clinic";
            return clinic;
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
            return clinic.OpenHour < TimeOnly.FromDateTime(now) && TimeOnly.FromDateTime(now) < clinic.CloseHour && clinic.Working;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    clinicRepository.Dispose();
                    clinicServiceRepository.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public ClinicServiceCategoryModel? AddServiceCategory(ClinicServiceCategoryModel service, out string message)
        {
            if (clinicServiceRepository.GetAllServiceCategory().Any(x => x.Name == service.Name))
            {
                message = "There is another service with this name exist";
                return null;
            }

            var serviceCategory = clinicServiceRepository.AddServiceCategory(service);

            if (serviceCategory != null)
            {
                message = "Added new service successfully";
                return serviceCategory;
            }

            message = "Failed";
            return null;
        }

        public ClinicServiceCategoryModel? UpdateServiceCategory(ClinicServiceCategoryModel service, out string message)
        {
            if (clinicServiceRepository.GetAllServiceCategory().Any(x => x.Name == service.Name))
            {
                message = "There is another service with this name exist";
                return null;
            }

            ClinicServiceCategoryModel? serviceCategory = clinicServiceRepository.UpdateServiceCategory(service);

            if (serviceCategory != null)
            {
                message = "Added new service successfully";
                return serviceCategory;
            }

            message = "Failed";
            return null;
        }

        public IEnumerable<ClinicServiceCategoryModel> GetServiceCategories()
        {
            return clinicServiceRepository.GetAllServiceCategory();
        }

        // Old relic from the pass.
        public IEnumerable<ClinicInfoModel> FilterClinicList( TimeOnly? from = null, TimeOnly? to = null, bool includeInactive = false, bool includeRemoved = false)
        {
            IEnumerable<ClinicInfoModel> clinics = clinicRepository.GetAllClinic();

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

        public IEnumerable<ClinicServiceInfoModel> GetClinicServiceWithName(int clinicId, string name)
        {
            return clinicServiceRepository.GetAllClinicService().Where(x => x.ClinicId == clinicId && x.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
        }

        public ClinicServiceInfoModel? GetClinicOnCategory(int categoryId)
        {
            throw new NotImplementedException();
        }

        public ClinicServiceInfoModel? DisableClinicService(Guid clinicServiceId, out string message)
        {
            ClinicServiceInfoModel? clinicService = clinicServiceRepository.GetClinicService(clinicServiceId);

            if (clinicService != null)
            {
                clinicService.Available = false;

                clinicService = clinicServiceRepository.UpdateClinicService(clinicService);

                message = clinicService != null ? $"Disabled clinic service {clinicServiceId}" : "Failed to disable clinic service!";
                return clinicService;
            }

            message = $"Can not find the specific clinic service {clinicServiceId}";
            return null;
        }

        public ClinicServiceInfoModel? EnableClinicService(Guid clinicServiceId, out string message)
        {
            ClinicServiceInfoModel? clinicService = clinicServiceRepository.GetClinicService(clinicServiceId);

            if (clinicService != null)
            {
                clinicService.Available = true;

                clinicService = clinicServiceRepository.UpdateClinicService(clinicService);

                message = clinicService != null ? $"Enabled clinic service {clinicServiceId}" : "Failed to enable clinic service!";
                return clinicService;
            }

            message = $"Can not find the specific clinic service {clinicServiceId}";
            return null;
        }

        public bool DeleteClinicServices(Guid clinicServiceId, out string message)
        {
            ClinicServiceInfoModel? clinicService = clinicServiceRepository.GetClinicService(clinicServiceId);

            if (clinicService != null)
            {
                clinicService.Removed = true;
                clinicService = clinicServiceRepository.UpdateClinicService(clinicService);

                if (clinicService != null)
                {
                    message = $"Deleted clinic service {clinicServiceId}";
                    return true;
                }

                message = "Failed to delete the clinic service!";
                return false;
            }

            message = $"Can not find the specific clinic service {clinicServiceId}";
            return false;
        }
    }
}
