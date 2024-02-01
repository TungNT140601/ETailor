namespace Etailor.API.WebAPI.ViewModels
{
    public class ProfileBodyVM
    {
        public string Id { get; set; }
        public string? CustomerId { get; set; }
        public string? StaffId { get; set; }
        public string? Name { get; set; }
        public bool? IsLocked { get; set; }
    }
}
