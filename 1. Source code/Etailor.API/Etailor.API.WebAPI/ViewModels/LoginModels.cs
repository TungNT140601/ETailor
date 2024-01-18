namespace Etailor.API.WebAPI.ViewModels
{
    public class CusLoginEmail
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
    public class StaffLogin
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
    public class VerifyOtp
    {
        public string PhoneOrEmail { get; set; }
        public string Otp { get; set; }
    }
    public class ChangePassModel
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
    public class CusRegis
    {
        public string? Avatar { get; set; }
        public string? Fullname { get; set; }
        public string? Address { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
