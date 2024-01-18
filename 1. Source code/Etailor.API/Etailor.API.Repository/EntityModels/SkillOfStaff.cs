using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class SkillOfStaff
    {
        public string Id { get; set; } = null!;
        public string? SkillId { get; set; }
        public string? StaffId { get; set; }

        public virtual Skill? Skill { get; set; }
        public virtual Staff? Staff { get; set; }
    }
}
