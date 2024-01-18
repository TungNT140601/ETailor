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
}
