using System.ComponentModel.DataAnnotations;

namespace Etailor.API.WebAPI.ViewModels
{
    public class CustomerVM
    {
        public IFormFile? AvatarImage { get; set; }
        public string? Fullname { get; set; }
        public string? Address { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
    }
    public class CustomerAllVM
    {
        public string? Id { get; set; }
        public string? Avatar { get; set; }
        public string? Fullname { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }
    public class CusRegis
    {
        public IFormFile? AvatarImage { get; set; }
        public string? Fullname { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
