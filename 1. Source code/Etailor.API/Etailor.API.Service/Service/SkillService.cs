using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.Repository;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CustomException;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class SkillService : ISkillService
    {
        private readonly ISkillRepository skillRepository;
        public SkillService(ISkillRepository skillRepository)
        {
            this.skillRepository = skillRepository;
        }

        public Skill FindById(string id)
        {
            return skillRepository.Get(id);
        }

        public IEnumerable<Skill> GetAllSkill(string? search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return skillRepository.GetAll(x => x.IsActive == true);
            }
            else
            {
                return skillRepository.GetAll(x => ((x.Name != null && x.Name.Trim().ToLower().Contains(search.Trim().ToLower()))) && x.IsActive == true);
            }
        }

        public bool CreateSkill(Skill skill)
        {
            if (skillRepository.GetAll(x => x.Name == skill.Name && x.IsActive == true).Any())
            {
                throw new UserException("Tên kỹ năng đã được sử dụng");
            }
            skill.Id = Ultils.GenGuidString();

            skill.IsActive = true;

            skill.CreatedTime = DateTime.Now;

            skill.LastestUpdatedTime = DateTime.Now;


            return skillRepository.Create(skill);
        }

        public bool UpdateSkill(Skill skill)
        {
            var dbSkill = skillRepository.Get(skill.Id);
            if (dbSkill != null)
            {
                if (skillRepository.GetAll(x => x.Id != dbSkill.Id && x.Name == skill.Name && x.IsActive == true).Any())
                {
                    throw new UserException("Tên kỹ năng đã được sử dụng");
                }
                else
                {
                    dbSkill.Name = skill.Name;
                }

                dbSkill.LastestUpdatedTime = DateTime.Now;

                return skillRepository.Update(dbSkill.Id, dbSkill);
            }
            else
            {
                throw new UserException("Không tìm thấy kỹ năng");
            }
        }

        public bool DeactiveSkill(string id)
        {
            var dbSkill = skillRepository.Get(id);
            if (dbSkill != null)
            {
                dbSkill.IsActive = false;

                dbSkill.InactiveTime = DateTime.Now;

                dbSkill.LastestUpdatedTime = DateTime.Now;

                return skillRepository.Update(dbSkill.Id, dbSkill);
            }
            else
            {
                throw new UserException("Không tìm thấy kỹ năng");
            }
        }

        public bool ActiveSkill(string id)
        {
            var dbSkill = skillRepository.Get(id);
            if (dbSkill != null)
            {
                dbSkill.IsActive = true;

                dbSkill.InactiveTime = null;

                dbSkill.LastestUpdatedTime = DateTime.Now;

                return skillRepository.Update(dbSkill.Id, dbSkill);
            }
            else
            {
                throw new UserException("Không tìm thấy kỹ năng");
            }
        }
    }
}
