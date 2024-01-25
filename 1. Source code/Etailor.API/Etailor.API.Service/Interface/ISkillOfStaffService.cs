using Etailor.API.Repository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface ISkillOfStaffService
    {
        IEnumerable<SkillOfStaff> GetAllSkillOfStaffByStaffId(string? search);

        IEnumerable<SkillOfStaff> GetAllSkillOfStaffBySkillId(string? search);

        bool CreateSkillOfStaff(string skillId, string staffId);

        bool DeleteSkillOfStaff(string id);
    }
}
