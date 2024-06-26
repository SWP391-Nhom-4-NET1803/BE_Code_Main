﻿using ClinicPlatformDTOs.ClinicModels;
using ClinicPlatformDTOs.SlotModels;
using ClinicPlatformObjects.ClinicModels;
using ClinicPlatformObjects.ServiceModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClinicPlatformWebAPI.Helpers.ModelMapper
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClinicMapper : ControllerBase
    {
        public static ClinicInfoModel MapToClinicInfo(ClinicRegistrationModel model)
        {
            return new ClinicInfoModel
            {
                Description = model.Description,
                Name = model.Name,
                Address = model.Address,
                Email = model.Email,
                Phone = model.Phone,
                Status = "unverified",
                Working = false,
                OpenHour = (TimeOnly) model.OpenHour!,
                CloseHour = (TimeOnly) model.CloseHour!,
            };
        }

        public static ClinicSlotInfoModel MapToSlotInfo(ClinicSlotRegistrationModel model)
        {
            return new ClinicSlotInfoModel
            {
                 ClinicId = model.ClinicId,
                 SlotId = model.SlotId,
                 MaxCheckup = model.MaxCheckup,
                 MaxTreatment = model.MaxTreatment,
                 Weekday = model.Weekday,
            };
        }

        public static ClinicServiceInfoModel MapToServiceInfo(ClinicServiceRegistrationModel model)
        {
            return new ClinicServiceInfoModel
            {
                Name = model.ServiceName ?? "",
                Description = model.ServiceDescription,
                Price = model.servicePrice,
                ClinicId = model.clinicId,
                CategoryId = model.ServiceCategory
            };
        }
    }
}
