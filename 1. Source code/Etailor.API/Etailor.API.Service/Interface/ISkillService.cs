using Etailor.API.Repository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface ISkillService
    {
        Skill FindById(string id);

        Skill GetAllSkill(string id);

        bool CreateSkill (Skill skill);

        bool UpdateSkill(Skill skill);

        bool DeactiveSkill(Skill skill);

        bool ActiveSkill(Skill skill);
    }
}
