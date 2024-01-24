using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.Repository;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity.CustomException;
using Etailor.API.Ultity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class SkillOfStaffService : ISkillOfStaffService
    {
        private readonly ISkillOfStaffRepository skillOfStaffRepository;
        private readonly ISkillRepository skillRepository;
        private readonly IStaffRepository staffRepository;
        
        public SkillOfStaffService(ISkillOfStaffRepository skillOfStaffRepository, ISkillRepository skillRepository, IStaffRepository staffRepository)
        {
            this.skillOfStaffRepository = skillOfStaffRepository;
            this.skillRepository = skillRepository;
            this.staffRepository = staffRepository;
        }

        //Check ko cho trùng data
        private bool CheckDuplicate(string? idSkill, string idStaff)
        {
            return skillOfStaffRepository.GetAll(x => (x.SkillId == idSkill && x.StaffId == idStaff)).Any();
        }

        public SkillOfStaff FindById(string id)
        {
            return skillOfStaffRepository.Get(id);
        }

        public IEnumerable<SkillOfStaff> GetAllSkillOfStaffByStaffId(string? search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return skillOfStaffRepository.GetAll(x => (x.Skill.IsActive == true && x.Staff.IsActive == true));
            }
            else
            {
                return skillOfStaffRepository.GetAll(x => ((x.Staff.Fullname != null && x.Staff.Fullname.Trim().ToLower().Contains(search.Trim().ToLower())) && x.Staff.IsActive == true)
                                                       || ((x.Skill.Name != null && x.Skill.Name.Trim().ToLower().Contains(search.Trim().ToLower())) && x.Skill.IsActive == true));
            }
        }

        public IEnumerable<SkillOfStaff> GetAllSkillOfStaffBySkillId(string? search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return skillOfStaffRepository.GetAll(x => (x.Skill.IsActive == true && x.Staff.IsActive == true));
            }
            else
            {
                return skillOfStaffRepository.GetAll(x => ((x.Staff.Fullname != null && x.Staff.Fullname.Trim().ToLower().Contains(search.Trim().ToLower())) && x.Staff.IsActive == true)
                                                       || ((x.Skill.Name != null && x.Skill.Name.Trim().ToLower().Contains(search.Trim().ToLower())) && x.Skill.IsActive == true));
            }
        }

        //Create Skill Of Staff
        //Param: list of skills, staffId
        //Gen Id (skill of staff) / skills + staff.
        public bool CreateSkillOfStaff(string skillId, string staffId)
        {
            SkillOfStaff skillOfStaff = new SkillOfStaff();
            skillOfStaff.Id = Ultils.GenGuidString();

            if (CheckDuplicate(skillId, staffId))
            {
                throw new UserException("Nhân viên này đã được thêm kỹ năng này.");
            }
            skillOfStaff.StaffId = staffId;
            skillOfStaff.SkillId = skillId;

            return skillOfStaffRepository.Create(skillOfStaff);
        }

        public bool DeleteSkillOfStaff(string id)
        {
            var existSkillOfStaff = skillOfStaffRepository.Get(id);
            if (existSkillOfStaff == null)
            {
                throw new Exception("Không tìm thấy ID");
            }
            else
            {
                return skillOfStaffRepository.Delete(id);
            }
        }


    }
}
