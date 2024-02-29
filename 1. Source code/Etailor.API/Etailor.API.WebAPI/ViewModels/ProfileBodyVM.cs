using Etailor.API.Repository.EntityModels;
using Etailor.API.Service.Service;
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

    public class GetAllProfileBodyOfCustomerVM
    {
        public string? Id { get; set; }
        public string? CustomerName { get; set; }
        public string? StaffId { get; set; }
        public string? StaffName { get; set; }
        public string? Name { get; set; }
        public bool? IsLocked { get; set; }
        public DateTime? CreatedTime { get; set; }
    }

    public class CreateProfileBodyVM 
    {
        public string? Name { get; set; }
        public string CustomerId { get; set; }

        public List<ValueBodyAttribute> valueBodyAttribute { get; set; }
    }

    public class ValueBodyAttribute
    {
        public string Id { get; set; }
        public decimal Value { get; set; }
    }

    //public class CreateProfileBodyByCustomerVM
    //{
    //    public string? Name { get; set; }

    //    public List<ValueBodyAttribute> valueBodyAttribute { get; set; }
    //}

    public class UpdateProfileBodyVM
    {
        public string Id { get; set; }

        public string? Name { get; set; }
    }

    public class DetailProfileBody
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public decimal Value { get; set; }
        public string? Image { get; set; }

    }

    public class GetDetailProfileBodyVM
    {
        public string? CustomerId { get; set; }
        public string? StaffId { get; set; }
        public string? Name { get; set; }
        public bool? IsLocked { get; set; }
        public List<DetailProfileBody> valueBodyAttribute { get; set; }
    }
}
