using AutoMapper;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Ultity.CustomException;
using Etailor.API.WebAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/skill-of-staff")]
    [ApiController]
    public class SkillOfStaffController : ControllerBase
    {
        private readonly ISkillOfStaffService skillOfStaffService;
        private readonly IStaffService staffService;
        private readonly ISkillService skillService;
        private readonly IMapper mapper;

        public SkillOfStaffController(ISkillOfStaffService skillOfStaffService, IStaffService staffService, ISkillService skillService, IMapper mapper)
        {
            this.skillOfStaffService = skillOfStaffService;
            this.mapper = mapper;
            this.skillService = skillService;
            this.staffService = staffService;
        }

        //[HttpGet("get-all-by-staff-id")]
        //public IActionResult GetAll(string? search)
        //{
        //    try
        //    {
        //        var skillofstaffs = skillOfStaffService.GetAllSkillOfStaffByStaffId(search);
        //        return Ok(mapper.Map<IEnumerable<SkillOfStaffListVM>>(skillofstaffs));
        //    }
        //    catch (UserException ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //    catch (SystemsException ex)
        //    {
        //        return StatusCode(500, ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ex.Message);
        //    }
        //}

        [HttpPost]
        public IActionResult CreateSkillOfStaff(string staffId, string skillId)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized();
                }
                else if (role != RoleName.MANAGER)
                {
                    return Forbid();
                }
                else
                {
                    var managerId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(managerId, secrectKey))
                    {
                        return Unauthorized();
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(staffId) || string.IsNullOrEmpty(skillId))
                        {
                            throw new UserException("Nhập staff và skill");
                        }
                        else
                        {
                            return skillOfStaffService.CreateSkillOfStaff(skillId, staffId) ? Ok() : BadRequest();
                        }
                    }
                }
            }
            catch (UserException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (SystemsException ex)
            {
                return StatusCode(500, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
