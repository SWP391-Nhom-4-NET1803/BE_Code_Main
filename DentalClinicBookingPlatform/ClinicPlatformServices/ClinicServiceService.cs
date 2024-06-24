using ClinicPlatformDTOs.ClinicModels;
using ClinicPlatformRepositories.Contracts;
using ClinicPlatformServices.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformServices
{
    public class ClinicServiceService : IClinicServiceService
    {
        private IClinicServiceRepository clinicServiceRepository;
        private IClinicRepository clinicRepository;

        private bool disposedValue;

        public ClinicServiceService(IClinicServiceRepository clinicServiceRepository, IClinicRepository clinicRepository)
        {
            this.clinicServiceRepository = clinicServiceRepository;
            this.clinicRepository = clinicRepository;
        }

        public ClinicServiceInfoModel? AddClinicService(ClinicServiceInfoModel model, out string message)
        {
            var clinic = clinicRepository.GetClinic(model.ClinicId);

            if (clinic == null)
            {
                message = "Can not find clinic information";
                return null;
            }

            var clinicCategory = clinicServiceRepository.GetServiceCategory(model.CategoryId);

            if (clinicCategory == null)
            {
                message = "Can not find specified service category";
                return null;
            }

            var service = clinicServiceRepository.AddClinicService(model);
            
            if (service != null)
            {
                message = "Create new clinic service";
                return service;
            }

            message = "Failed while creating clinic service";
            return null;
        }

        public bool AddClinicServices(IEnumerable<ClinicServiceInfoModel> clinicServices, out string message)
        {
            int passed = 0;
            foreach (var clinicService in clinicServices)
            {
                var tempt = AddClinicService(clinicService, out message);

                if (tempt != null)
                {
                    passed += 0;
                }

            }

            message = $"Added {passed}/{clinicServices.Count()} service.";
            return passed != 0 ? true : false;
        }

        public bool AddServiceCategory(ServiceCategoryModel service, out string message)
        {
            throw new NotImplementedException();
        }

        public bool DeleteClinicServices(Guid clinicServiceId, out string message)
        {
            throw new NotImplementedException();
        }

        public bool DeleteClinicServices(IEnumerable<Guid> clinicServiceId, out string message)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ServiceCategoryModel> GetAllCategory()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ClinicServiceInfoModel> GetAllClinicService(int clinicId)
        {
            return clinicServiceRepository.GetAllClinicService().Where(x => x.ClinicId == clinicId);
        }

        public ServiceCategoryModel? GetCategory(int serviceId)
        {
            return clinicServiceRepository.GetServiceCategory(serviceId);
        }

        public ClinicServiceInfoModel? GetClinicService(Guid serviceId)
        {
            return clinicServiceRepository.GetClinicService(serviceId);
        }

        public IEnumerable<ClinicServiceInfoModel> GetClinicServiceWithPrice(int serviceId, int minimum, int maximum)
        {
            return clinicServiceRepository.GetAllClinicService().Where(x => x.CategoryId == serviceId && minimum < x.Price && x.Price < maximum);
        }

        public bool UpdateCategory(ServiceCategoryModel service, out string message)
        {
            throw new NotImplementedException();
        }

        public ClinicServiceInfoModel? UpdateClinicService(ClinicServiceInfoModel clinicService, out string message)
        {
            throw new NotImplementedException();
        }

        public bool UpdateClinicServices(IEnumerable<ClinicServiceInfoModel> clinicServices, out string message)
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.clinicRepository.Dispose();
                    this.clinicServiceRepository.Dispose();
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
