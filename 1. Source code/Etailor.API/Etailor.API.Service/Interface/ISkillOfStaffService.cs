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
        bool CreateSkillOfStaff(SkillOfStaff skillOfStaff);

        bool UpdateSkillOfStaff(SkillOfStaff skillOfStaff);

        bool DeleteSkillOfStaff(string id);

        bool ActiveSkillOfStaff(string id);

        SkillOfStaff GetSkillOfStaff(string id);

        IEnumerable<SkillOfStaff> GetSkillOfStaffs(string? search);    
    }
}
