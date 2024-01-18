using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class SkillForStage
    {
        public string Id { get; set; } = null!;
        public string? SkillId { get; set; }
        public string? ProductStageId { get; set; }

        public virtual ProductStage? ProductStage { get; set; }
        public virtual Skill? Skill { get; set; }
    }
}
