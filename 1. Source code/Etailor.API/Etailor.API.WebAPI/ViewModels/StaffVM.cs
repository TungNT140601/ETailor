namespace Etailor.API.WebAPI.ViewModels
{
    public class StaffCreateVM
    {
        public string? ImageBase64 { get; set; }
        public string? ImageName { get; set; }
        public string? Fullname { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
    public class StaffUpdateVM
    {
        public string? Id { get; set; }
        public string? ImageBase64 { get; set; }
        public string? ImageName { get; set; }
        public string? Fullname { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Username { get; set; }
    }
    public class StaffListVM
    {
        public int? STT { get; set; }
        public string? Id { get; set; }
        public string? Avatar { get; set; }
        public string? Fullname { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Username { get; set; }
    }
    public class StaffChangePassVM
    {
        public string? OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
