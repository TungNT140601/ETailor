using System.ComponentModel.DataAnnotations;

namespace Etailor.API.WebAPI.ViewModels
{
    public class CustomerVM
    {
        public string? Avatar { get; set; }

        [Required]
        public string? Fullname { get; set; }

        [Required]
        public string? Address { get; set; }
    }
}
