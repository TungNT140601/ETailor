using Etailor.API.Repository.EntityModels;
using System.Text.Json.Serialization;

namespace Etailor.API.WebAPI.ViewModels
{
    public class ProfileBodyVM
    {
        public string? Id { get; set; }
        public string? CustomerId { get; set; }
        public string? StaffId { get; set; }
        public string? Name { get; set; }
        public bool? IsLocked { get; set; }
    }

    public class CreateProfileBodyByStaffVM 
    {
        public string? Name { get; set; }
        //public string customerId {  get; set; }

        public List<ValueBodyAttribute> valueBodyAttribute { get; set; }
    }

    public class ValueBodyAttribute
    {
        public string Id { get; set; }
        public decimal Value { get; set; }
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
