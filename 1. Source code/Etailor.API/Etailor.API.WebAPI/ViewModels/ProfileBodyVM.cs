namespace Etailor.API.WebAPI.ViewModels
{
    public class ProfileBodyVM
    {
        public string Id { get; set; }
        public string? CustomerId { get; set; }
        public string? StaffId { get; set; }
        public string? Name { get; set; }
        public bool? IsLocked { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }
    }

    public class CreateProfileBodyByStaffVM
    {
        public string? Name { get; set; }

        public string? CustomerId { get; set; }
    }

    public class CreateProfileBodyByCustomerVM
    {
        public string? Name { get; set; }
    }

    public class UpdateProfileBodyVM
    {
        public string Id { get; set; }

        public string? Name { get; set; }
    }
}
