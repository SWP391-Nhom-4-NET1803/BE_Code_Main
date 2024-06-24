using ClinicPlatformDTOs.SlotModels;

namespace ClinicPlatformDTOs.ClinicModels
{
    public class ClinicRegistrationModel
    {
        public string OwnerFullName { get; set; } = null!;
        public string OwnerUserName { get; set; } = null!;
        public string OwnerPassword { get; set; } = null!;
        public string OwnerEmail { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Email { get; set; } = null!;
        public TimeOnly? OpenHour { get; set; }
        public TimeOnly? CloseHour { get; set; }
        public List<int>? ClinicServices { get; set; }
    }
}