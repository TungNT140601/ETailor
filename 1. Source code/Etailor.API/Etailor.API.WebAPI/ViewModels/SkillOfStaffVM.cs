using Etailor.API.Repository.EntityModels;

namespace Etailor.API.WebAPI.ViewModels
{
    public class SkillOfStaffCreateVM
    {
        public string Id { get; set; } = null!;
        public string? SkillId { get; set; }
        public string? StaffId { get; set; }
    }


    public class SkillOfStaffUpdateVM
    {
        public string Id { get; set; } = null!;
        public string? SkillId { get; set; }
        public string? StaffId { get; set; }
    }

    public class SkillOfStaffListVM
    {
        public string Id { get; set; } = null!;
        public virtual SkillVM Skill { get; set; }

        public virtual StaffVM Staffs { get; set; }
    }
}
