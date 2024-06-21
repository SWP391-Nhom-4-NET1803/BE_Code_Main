using AutoMapper;
using Core.HttpModels.ObjectModels.BookingModels;
using Core.HttpModels.ObjectModels.ClinicModels;
using Core.HttpModels.ObjectModels.SlotModels;
using Core.HttpModels.ObjectModels.UserModel;
using Repositories.Models;

namespace WebAPI.Helper.Mappers
{
    public class MapperProfile: Profile
    {
        public MapperProfile() 
        {
            // The generic one
            CreateMap<User, UserInfoModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId));

            CreateMap<User, CustomerInfoModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Fullname, opt => opt.MapFrom(src => src.Fullname))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.RoleId))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.Customer.CustomerId))
            .ForMember(dest => dest.Sex, opt => opt.MapFrom(src => src.Customer.Sex))
            .ForMember(dest => dest.Birthdate, opt => opt.MapFrom(src => src.Customer.BirthDate))
            .ForMember(dest => dest.Insurance, opt => opt.MapFrom(src => src.Customer.Insurance));

            CreateMap<User, ClinicStaffInfoModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Fullname, opt => opt.MapFrom(src => src.Fullname))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.RoleId))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.StaffId, opt => opt.MapFrom(src => src.ClinicStaff.StaffId))
            .ForMember(dest => dest.IsOwner, opt => opt.MapFrom(src => src.ClinicStaff.IsOwner));

            CreateMap<CustomerInfoModel, User>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Fullname, opt => opt.MapFrom(src => src.Fullname))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.Role))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => new Customer() { UserId = src.Id, CustomerId = (int)src.CustomerId!, Insurance = src.Insurance, BirthDate = src.Birthdate }));

            CreateMap<ClinicStaffInfoModel, User>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Fullname, opt => opt.MapFrom(src => src.Fullname))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.Role))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => new ClinicStaff() { UserId = src.Id, StaffId = (int)src.StaffId!, ClinicId = src.ClinicId, IsOwner = src.IsOwner??false }));

            CreateMap<Customer, CustomerInfoModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Fullname, opt => opt.MapFrom(src => src.User.Fullname))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.User.Password))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.User.RoleId))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.User.Status))
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
            .ForMember(dest => dest.Sex, opt => opt.MapFrom(src => src.Sex))
            .ForMember(dest => dest.Birthdate, opt => opt.MapFrom(src => src.BirthDate))
            .ForMember(dest => dest.Insurance, opt => opt.MapFrom(src => src.Insurance))
            .ForMember(dest => dest.JoinedDate, opt => opt.MapFrom(src => src.User.CreationDate));

            CreateMap<ClinicStaff, ClinicStaffInfoModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Fullname, opt => opt.MapFrom(src => src.User.Fullname))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.User.Password))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.User.RoleId))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.User.Status))
            .ForMember(dest => dest.StaffId, opt => opt.MapFrom(src => src.StaffId))
            .ForMember(dest => dest.ClinicId, opt => opt.MapFrom(src => src.ClinicId))
            .ForMember(dest => dest.IsOwner, opt => opt.MapFrom(src => src.IsOwner))
            .ForMember(dest => dest.JoinedDate, opt => opt.MapFrom(src => src.User.CreationDate));

            CreateMap<ClinicService, ClinicServiceModel>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ClinicServiceId))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Service.ServiceName))
              .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
              .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
              .ForMember(dest => dest.ClinicId, opt => opt.MapFrom(src => src.ClinicId))
              .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.ServiceId));
                
            CreateMap<Clinic, ClinicInformationModel>()
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ClinicId))
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
             .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
             .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
             .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
             .ForMember(dest => dest.OpenHour, opt => opt.MapFrom(src => src.OpenHour))
             .ForMember(dest => dest.CloseHour, opt => opt.MapFrom(src => src.CloseHour))
             .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
             .ForMember(dest => dest.ClinicServices, opt => opt.MapFrom(src => src.ClinicServices))
             .ForMember(dest => dest.ClinicStaff, opt => opt.MapFrom(src => src.ClinicStaffs));

            CreateMap<Booking, BookingModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.BookId))
                .ForMember(dest => dest.ClinicId, opt => opt.MapFrom(src => src.ClinicId))
                .ForMember(dest => dest.ClinicName, opt => opt.MapFrom(src => src.Clinic.Name))
                .ForMember(dest => dest.ClinicPhone, opt => opt.MapFrom(src => src.Clinic.Phone))
                .ForMember(dest => dest.ClinicAddress, opt => opt.MapFrom(src => src.Clinic.Address))
                .ForMember(dest => dest.appointmentDate, opt => opt.MapFrom(src => src.AppointmentDate))
                .ForMember(dest => dest.AppointmentTime, opt => opt.MapFrom(src => src.ScheduleSlot.Slot.StartTime))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.BookingType))
                .ForMember(dest => dest.SelectedService, opt => opt.MapFrom(src => src.BookingServiceId))
                .ForMember(dest => dest.SelectedServiceName, opt => opt.MapFrom(src => src.BookingService!.Service.ServiceName ?? null))
                .ForMember(dest => dest.CreationTime, opt => opt.MapFrom(src => src.CreationDate))
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
                .ForMember(dest => dest.CustomerFullName, opt => opt.MapFrom(src => src.Customer.User.Fullname))
                .ForMember(dest => dest.DentistId, opt => opt.MapFrom(src => src.DentistId))
                .ForMember(dest => dest.DentistFullname, opt => opt.MapFrom(src => src.Dentist.User.Fullname))
                .ForMember(dest => dest.timeSlotId, opt => opt.MapFrom(src => src.ScheduleSlotId))
                .ForMember(dest => dest.ExpectedEndTime, opt => opt.MapFrom(src => src.ScheduleSlot.Slot.EndTime));

                CreateMap<ScheduledSlot, ClinicSlotInfoModel>()
                .ForMember(dest => dest.ClinicSlotId, opt => opt.MapFrom(src => src.ScheduleSlotId))
                .ForMember(dest => dest.MaxAppointment, opt => opt.MapFrom(src => src.MaxAppointments))
                .ForMember(dest => dest.Weekday, opt => opt.MapFrom(src => src.DateOfWeek))
                .ForMember(dest => dest.start, opt => opt.MapFrom(src => src.Slot.StartTime))
                .ForMember(dest => dest.end, opt => opt.MapFrom(src => src.Slot.EndTime));
        }
    }
}
