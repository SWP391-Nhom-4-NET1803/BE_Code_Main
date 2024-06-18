namespace ClinicPlatformDTOs.ClinicModels
{
    public class ClinicRegistrationModel
    {
        public int? OwnerId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public TimeOnly? OpenHour { get; set; }
        public TimeOnly? CloseHour { get; set; }
        public List<int>? ClinicServices { get; set; }
        public List<string>? Attachments { get; set; }
    }
}