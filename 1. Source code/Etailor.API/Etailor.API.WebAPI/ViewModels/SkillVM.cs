using System.ComponentModel.DataAnnotations;

namespace Etailor.API.WebAPI.ViewModels
{
    public class SkillVM
    {
        public string Id { get; set; } = null!;

        [Required]
        public string? Name { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }
    }
}
