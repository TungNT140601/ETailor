using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class Skill
    {
        public Skill()
        {
            SkillForStages = new HashSet<SkillForStage>();
            SkillOfStaffs = new HashSet<SkillOfStaff>();
        }

        public string Id { get; set; } = null!;
        public string? Name { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual ICollection<SkillForStage> SkillForStages { get; set; }
        public virtual ICollection<SkillOfStaff> SkillOfStaffs { get; set; }
    }
}
