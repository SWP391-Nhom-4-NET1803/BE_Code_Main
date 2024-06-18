using ClinicPlatformBusinessObject;
using ClinicPlatformDAOs;
using ClinicPlatformDTOs.ClinicModels;
using ClinicPlatformRepositories.Contracts;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformRepositories
{
    public class ClinicServiceRepository : IClinicServiceRepository
    {
        private readonly ClinicServiceDAO clinicServiceDAO;
        private readonly ServiceDAO serviceDAO;
        private bool disposedValue;

        public ClinicServiceRepository()
        {
            this.clinicServiceDAO = new ClinicServiceDAO();
            this.serviceDAO = new ServiceDAO();
        }

        public ClinicServiceInfoModel? CreateBaseService(ClinicServiceInfoModel service)
        {
            if (service.Name.IsNullOrEmpty())
            {
                return null;
            }

            if (serviceDAO.GetAllService().Any(x => x.ServiceName == service.Name))
            {
                return null;
            }

            Service newService = new Service()
            {
                ServiceName = service.Name!
            };

            serviceDAO.AddService(newService);

            return service;
        }

        public ClinicServiceInfoModel? AddClinicService(ClinicServiceInfoModel clinicServiceInfo)
        {
            Service? baseService = serviceDAO.GetAllService()
                .Where(x => x.ServiceId == clinicServiceInfo.ServiceId)
                .FirstOrDefault();

            if (baseService == null)
            {
                return null;
            }

            ClinicService newClinicService = new ClinicService()
            {
                ClinicId = (int) clinicServiceInfo.ClinicId!,
                Price = clinicServiceInfo.Price,
                ServiceId = (int) clinicServiceInfo.ServiceId!,
                Description = clinicServiceInfo.Description,
            };

            clinicServiceDAO.AddClinicService(newClinicService);
            return clinicServiceInfo;

        }

        public void DeleteBaseService(int serviceId)
        {
            serviceDAO.DeleteService(serviceId);
        }

        public void DeleteClinicService(Guid clinicServiceId)
        {
            clinicServiceDAO.DeleteClinicService(clinicServiceId);
        }

        public IEnumerable<ClinicServiceInfoModel> GetAll(int clinicId)
        {
            // I was crying on the floor laughing because of this.
            return from service in clinicServiceDAO.GetAllClinicService()
                   join baseService in serviceDAO.GetAllService() on service.ServiceId equals baseService.ServiceId
                   where service.ClinicId == clinicId
                   select new ClinicServiceInfoModel()
                   {
                       ClinicServiceId = service.ClinicServiceId,
                       Price = service.Price,
                       ClinicId = clinicId,
                       Description= service.Description,
                       Name = baseService.ServiceName
                   };
        }

        public IEnumerable<ClinicServiceInfoModel> GetAll()
        {
            return from service in clinicServiceDAO.GetAllClinicService()
                   join baseService in serviceDAO.GetAllService() on service.ServiceId equals baseService.ServiceId
                   select new ClinicServiceInfoModel()
                   {
                       ClinicServiceId = service.ClinicServiceId,
                       Price = service.Price,
                       ClinicId = service.ClinicId,
                       Description = service.Description,
                       Name = baseService.ServiceName
                   };
        }

        public IEnumerable<ClinicServiceInfoModel> GetAllBaseService()
        {
            return from baseService in serviceDAO.GetAllService()
                   select new ClinicServiceInfoModel()
                   {
                       Name = baseService.ServiceName,
                       ServiceId = baseService.ServiceId,
                   };
        }

        public ClinicServiceInfoModel? GetBaseService(int serviceId)
        {
            var result = serviceDAO.GetService(serviceId);

            if (result != null)
            {
                return new ClinicServiceInfoModel() { ServiceId = result.ServiceId, Name = result.ServiceName };
            }
            return null;
        }

        public ClinicServiceInfoModel? GetClinicServie(Guid clinicServiceId)
        {
            var result = clinicServiceDAO.GetClinicService(clinicServiceId);

            var baseService = result != null ? serviceDAO.GetService(result.ServiceId) : null;

            if (baseService != null && result != null)
            {
                return new ClinicServiceInfoModel()
                {
                    ClinicServiceId = result.ClinicServiceId,
                    ClinicId = result.ClinicId,
                    Description = result.Description,
                    Price = result.Price,
                    ServiceId = result.ServiceId,
                    Name = baseService.ServiceName,
                };
            }

            return null;
        }

        public ClinicServiceInfoModel? UpdateBaseService(ClinicServiceInfoModel serviceInfo)
        {
            Service? service = serviceInfo.ServiceId != null ? serviceDAO.GetService((int) serviceInfo.ServiceId): null;
            
            if (service != null)
            {
                service.ServiceName = serviceInfo.Name ?? service.ServiceName;

                serviceDAO.UpdateService(service);
                return serviceInfo;
            }

            return null;
        }

        public ClinicServiceInfoModel? UpdateClinicService(ClinicServiceInfoModel serviceInfo)
        {
            ClinicService? clinicService = clinicServiceDAO.GetClinicService(serviceInfo.ClinicServiceId);

            Service? service = clinicService != null && serviceInfo.ServiceId != null ? serviceDAO.GetService((int)serviceInfo.ServiceId) : null;

            if (clinicService != null)
            {
                clinicService.Price = serviceInfo.Price ?? clinicService.Price;
                clinicService.Description = serviceInfo.Description ?? clinicService.Description;
                clinicService.ServiceId = service != null ? service.ServiceId : clinicService.ServiceId;

                clinicServiceDAO.UpdateClinicService(clinicService);
                return serviceInfo;
            }

            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    serviceDAO.Dispose();
                    clinicServiceDAO.Dispose(); 
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        // Mappers (currently have no use)
        private ClinicServiceInfoModel MapClinicServiceToClinicServiceModel(ClinicService service)
        {
            return new()
            {
                ClinicServiceId = service.ClinicServiceId,
                Name = service.Service.ServiceName,
                Description = service.Description,
                Price = service.Price,
                ServiceId = service.ServiceId,
                ClinicId = service.ClinicId,
            };
        }

        private ClinicService MapClinicServiceModelToClinicService(ClinicServiceInfoModel serviceInfo)
        {
            return new()
            {
                ClinicServiceId = serviceInfo.ClinicServiceId,
                Description = serviceInfo.Description,
                Price = serviceInfo.Price,
                ServiceId = serviceInfo.ServiceId ?? 0,
                ClinicId = serviceInfo.ClinicId ?? 0,
            };
        }
    }
}
