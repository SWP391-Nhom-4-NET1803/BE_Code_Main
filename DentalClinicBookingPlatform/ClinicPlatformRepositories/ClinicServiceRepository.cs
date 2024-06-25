using ClinicPlatformDatabaseObject;
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
        private readonly DentalClinicPlatformContext context;
        private bool disposedValue;

        public ClinicServiceRepository(DentalClinicPlatformContext context)
        {
            this.context = context;
        }

        public ClinicServiceCategoryModel? AddServiceCategory(ClinicServiceCategoryModel service)
        {
            if (service.Name.IsNullOrEmpty())
            {
                return null;
            }

            if (context.ServiceCategories.Any(x => x.Name == service.Name))
            {
                return null;
            }

            ServiceCategory newService = new ServiceCategory()
            {
                Name = service.Name!
                // Avaiable = service.Available;
            };

            context.ServiceCategories.Add(newService);
            context.SaveChanges();

            service.Id = newService.Id;

            return service;
        }

        public ClinicServiceInfoModel? AddClinicService(ClinicServiceInfoModel clinicServiceInfo)
        {
            ServiceCategory? category = context.ServiceCategories
                .Where(x => x.Id == clinicServiceInfo.CategoryId)
                .FirstOrDefault();

            if (category == null)
            {
                return null;
            }

            ClinicService newClinicService = MapToClinicService(clinicServiceInfo);

            context.ClinicServices.Add(newClinicService);
            return MapToClinicServiceModel(newClinicService);

        }

        public IEnumerable<ClinicServiceInfoModel> GetAllClinicService()
        {
            return from service in context.ClinicServices
                   join category in context.ServiceCategories on service.CategoryId equals category.Id
                   select new ClinicServiceInfoModel()
                   {
                       ClinicServiceId = service.Id,
                       Price = service.Price,
                       ClinicId = service.ClinicId,
                       Description = service.Description,
                       Name = service.CustomName ?? category.Name,
                       CategoryId = category.Id,
                       Available = service.Available,
                       Removed = service.Removed
                   };
        }

        public IEnumerable<ClinicServiceCategoryModel> GetAllServiceCategory()
        {
            return from category in context.ServiceCategories
                   select new ClinicServiceCategoryModel()
                   {
                       Name = category.Name,
                       Id = category.Id,
                   };
        }

        public ClinicServiceCategoryModel? GetServiceCategory(int categoryId)
        {
            var result = context.ServiceCategories.Find(categoryId);

            if (result != null)
            {
                return new ClinicServiceCategoryModel() { Id = result.Id, Name = result.Name };
            }
            return null;
        }

        public ClinicServiceInfoModel? GetClinicService(Guid clinicServiceId)
        {
            var result = context.ClinicServices.Find(clinicServiceId);

            var category = result != null ? context.ServiceCategories.Find(result.CategoryId) : null;

            if (category != null && result != null)
            {
                return new ClinicServiceInfoModel()
                {
                    ClinicServiceId = result.Id,
                    ClinicId = result.ClinicId,
                    Description = result.Description,
                    Price = result.Price,
                    CategoryId = result.CategoryId,
                    Name = result.CustomName ?? category.Name,
                };
            }

            return null;
        }

        public ClinicServiceCategoryModel? UpdateServiceCategory(ClinicServiceCategoryModel serviceInfo)
        {
            ServiceCategory? service = context.ServiceCategories.Find((int)serviceInfo.Id);
            
            if (service != null)
            {
                service.Name = serviceInfo.Name ?? service.Name;
                //service.Available = serviceInfo.Available

                context.ServiceCategories.Update(service);
                return new ClinicServiceCategoryModel
                {
                    Id = service.Id,
                    Name = service.Name
                };
            }

            return null;
        }

        public ClinicServiceInfoModel? UpdateClinicService(ClinicServiceInfoModel serviceInfo)
        {
            ClinicService? clinicService = context.ClinicServices.Find(serviceInfo.ClinicServiceId);

            if (clinicService != null)
            {
                clinicService.CustomName = serviceInfo.Name;
                clinicService.Description = serviceInfo.Description;
                clinicService.Available = serviceInfo.Available;
                clinicService.Removed = serviceInfo.Removed;
                clinicService.Price = serviceInfo.Price;
                clinicService.CategoryId = serviceInfo.CategoryId;

                context.ClinicServices.Update(clinicService);
                return MapToClinicServiceModel(clinicService);
            }

            return null;
        }

        public bool DeleteServiceCategory(int serviceId)
        {

            ServiceCategory? category = context.ServiceCategories.Find(serviceId);

            if (category != null)
            {
                context.ServiceCategories.Remove(category);

                return true;
            }


            return false;
        }

        public bool DeleteClinicService(Guid clinicServiceId)
        {
            ClinicService? service = context.ClinicServices.Find(clinicServiceId);

            if (service != null)
            {
                service.Removed = true;
                context.ClinicServices.Update(service);

                return true;
            }

            return false;
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

        private ClinicServiceInfoModel MapToClinicServiceModel(ClinicService service)
        {
            return new()
            {
                ClinicServiceId = service.Id,
                Name = service.CustomName ?? service.Category.Name,
                Description = service.Description,
                Price = service.Price,
                CategoryId = service.CategoryId,
                ClinicId = service.ClinicId,
                Available = service.Available,
                Removed = service.Removed,
            };
        }

        private ClinicService MapToClinicService(ClinicServiceInfoModel serviceInfo)
        {
            return new()
            {
                Id = serviceInfo.ClinicServiceId,
                CustomName = serviceInfo.Name,
                Description = serviceInfo.Description,
                Price = serviceInfo.Price,
                CategoryId = serviceInfo.CategoryId,
                ClinicId = serviceInfo.ClinicId,
                Available = serviceInfo.Available,
                Removed = serviceInfo.Removed,
            };
        }
    }
}
