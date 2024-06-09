public class ClinicRegistrationModel
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string OpenHour { get; set; }
    public string CloseHour { get; set; }
    public List<int> ClinicServices { get; set; } = new List<int>();
    public List<string> Certifications { get; set; } = new List<string>();
}
