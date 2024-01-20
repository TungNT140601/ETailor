using System.ComponentModel.DataAnnotations;

namespace Etailor.API.WebAPI.ViewModels
{
    public class SkillListVM
    {
        [Required]
        public string? Name { get; set; }

        public bool? IsActive { get; set; }
    }

    public class SkillCreateVM
    {
        [Required]
        public string? Name { get; set; }
    }

    public class SkillUpdateVM
    {
        public string? Id { get; set; }

        [Required]
        public string? Name { get; set; }
    }


}
