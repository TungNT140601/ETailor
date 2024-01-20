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
            try
            {
                return skillRepository.Get(id);
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }

        public IEnumerable<Skill> GetAllSkill(string? search)
        {
            try
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
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }

        public bool CreateSkill(Skill skill)
        {
            try
            {
                skill.Id = Ultils.GenGuidString();

                skill.IsActive = true;

                skill.CreatedTime = DateTime.Now;

                skill.LastestUpdatedTime = DateTime.Now;
 

                return skillRepository.Create(skill);
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }

        public bool UpdateSkill(Skill skill)
        {
            try
            {
                var dbSkill = skillRepository.Get(skill.Id);

                dbSkill.Name = skill.Name;

                dbSkill.LastestUpdatedTime = DateTime.Now;

                return skillRepository.Update(dbSkill.Id, dbSkill);
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }

        public bool DeactiveSkill(string id)
        {
            try
            {
                var dbSkill = skillRepository.Get(id);

                dbSkill.IsActive = false;

                dbSkill.InactiveTime = DateTime.Now;

                dbSkill.LastestUpdatedTime = DateTime.Now;

                return skillRepository.Update(dbSkill.Id, dbSkill);
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }

        public bool ActiveSkill(string id)
        {
            try
            {
                var dbSkill = skillRepository.Get(id);

                dbSkill.IsActive = true;

                dbSkill.InactiveTime = null;

                dbSkill.LastestUpdatedTime = DateTime.Now;

                return skillRepository.Update(dbSkill.Id, dbSkill);
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }
    }
}
