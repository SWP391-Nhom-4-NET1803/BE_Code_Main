namespace ClinicPlatformDTOs.ClinicModels
{
    public class ClinicRegistrationModel
    {
        public int? OwnerId { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? OpenHour { get; set; }
        public string? CloseHour { get; set; }
        public List<int>? ClinicServices { get; set; }
        public List<string>? Attachments { get; set; }
    }
}