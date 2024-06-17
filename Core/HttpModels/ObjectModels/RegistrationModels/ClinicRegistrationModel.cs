public class ClinicRegistrationModel
{
    public int? OwnerId { get; set; } = null;
    public string? Name { get; set; } = null;
    public string? Address { get; set; } = null;
    public string? Phone { get; set; } = null;
    public string? Email { get; set; } = null;
    public string? OpenHour { get; set; } = null;
    public string? CloseHour { get; set; } = null;
    public List<int>? ClinicServices { get; set; }
    public List<string>? Certifications { get; set; }
}
