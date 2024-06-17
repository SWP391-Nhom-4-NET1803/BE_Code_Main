using Core.HttpModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.Models;
using Services.ClinicsService;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.ClinicsService
{
    public class ClinicsService : IClinicsService
    {
        UnitOfWork _unitOfWork;
        private bool disposedValue;

        public ClinicsService(DentalClinicPlatformContext context)
        {
            _unitOfWork = new UnitOfWork(context);
        }

        public bool CreateClinic(ClinicRegistrationModel clinic, out string message)
        {
            ClinicStaff? owner = _unitOfWork.UserRepository.GetStaffInfo((int)clinic.OwnerId!);

            if (owner == null || owner.IsOwner == false)
            {
                message = "You are unauthorized to create a clinic.";
                return false;
            }

            Clinic newClinic = new Clinic()
            {
                Name = clinic.Name!,
                Address = clinic.Address!,
                Phone = clinic.Phone!,
                Email = clinic.Email!,
                OpenHour = TimeOnly.Parse(clinic.OpenHour!),
                CloseHour = TimeOnly.Parse(clinic.CloseHour!),
                Status = true,
                Owner = owner.User
            };
            
            // Add clinic services
            if(clinic.ClinicServices != null)
            {
                foreach (var serviceId in clinic.ClinicServices!)
                {
                    var service = _unitOfWork._context.Services.Find(serviceId);
                    if (service != null)
                    {
                        newClinic.ClinicServices.Add(new ClinicService
                        {
                            ServiceId = service.ServiceId,
                            Clinic = newClinic,
                            Service = service
                        });
                    }
                    else
                    {
                        message = $"Service with ID {serviceId} does not exist";
                        return false;
                    }
                }
            }

            // Add the clinic to the owner inside ClinicStaff table.
            owner.Clinic = newClinic;

            _unitOfWork.clinicStaffRepository.Update(owner);
            _unitOfWork.ClinicRepository.Add(newClinic);
            _unitOfWork.Save();

            message = "success";
            return true;
        }

        public bool CreateClinicService(ClinicService service)
        {
            if (_unitOfWork._context.ClinicServices.Where(x => x.ClinicId == service.ClinicId && x.Service.ServiceId == service.ServiceId).Any())
            {
                return false;
            }

            _unitOfWork.ClinicServiceRepository.Add(service);

            return true;
        }

        public IEnumerable<Clinic> GetAllClinicInformation()
        {
            return _unitOfWork._context.Clinics.ToList();
        }

        public Clinic? GetClinicInformation(int clinicId)
        {
            return _unitOfWork._context.Clinics.Include(x => x.ClinicServices).Where(x => x.ClinicId == clinicId).Include(x => x.ClinicStaffs).ThenInclude(y => y.User).Include(x => x.ClinicStaffs).ToList().FirstOrDefault();
        }

        public IEnumerable<ClinicService> GetClinicServices(int clinicId)
        {
            return _unitOfWork._context.ClinicServices.Include(x=>x.Service).Where(x => x.ClinicId == clinicId).ToList() ;
        }

        public ClinicService GetServiceInfo(Guid serviceId)
        {

            return _unitOfWork._context.ClinicServices.Include(x => x.Service).Where(x => x.ClinicServiceId == serviceId).First();
        }

        public bool UpdateClinicInformation(Clinic clinic, out string message)
        {
            _unitOfWork.ClinicRepository.Update(clinic);
            message = string.Empty;
            return true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
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
