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
    [Route("api/skill-management")]
    [ApiController]
    public class SkillController : ControllerBase
    {
        private readonly ISkillService skillService;
        private readonly IStaffService staffService;
        private readonly IConfiguration configuration;
        private readonly IMapper mapper;

        public SkillController(ISkillService skillService, IStaffService staffService, IConfiguration configuration, IMapper mapper)
        {
            this.skillService = skillService;
            this.staffService = staffService;
            this.configuration = configuration;
            this.mapper = mapper;
        }

        [HttpGet("Get-All")]
        public IActionResult GetAllSkill(string? search)
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
                    var staffId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(staffId, secrectKey))
                    {
                        return Unauthorized();
                    }
                    else
                    {
                        var skills = skillService.GetAllSkill(search).ToList();
                        return Ok(new
                        {
                            TotalData = skills.Count(),
                            Data = mapper.Map<List<SkillListVM>>(skills)
                        });
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

        [HttpPost()]
        public IActionResult CreateSkill([FromBody] SkillCreateVM skillVM)
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
                    var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(id, secrectKey))
                    {
                        return Unauthorized();
                    }
                    else
                    {
                        return skillService.CreateSkill(mapper.Map<Skill>(skillVM)) ? Ok() : BadRequest();
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

        [HttpPut()]
        public IActionResult UpdateSkill(string? id, [FromBody] SkillUpdateVM skillUpdateVM)
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
                    var staffId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(staffId, secrectKey))
                    {
                        return Unauthorized();
                    }
                    else
                    {
                        if (id == null)
                        {
                            throw new UserException("Id skill không được để trống");
                        }
                        else if (id != skillUpdateVM.Id)
                        {
                            throw new UserException("Id skill không khớp");
                        }
                        else
                        {
                            return skillService.UpdateSkill(mapper.Map<Skill>(skillUpdateVM)) ? Ok() : BadRequest();
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

        [HttpPut("Deactive-Skill/{id}")]
        public IActionResult DeactiveSkill(string id)
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
                    var staffId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(staffId, secrectKey))
                    {
                        return Unauthorized();
                    }
                    else
                    {
                        return skillService.DeactiveSkill(id) ? Ok() : BadRequest();
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

        [HttpPut("Active-Skill/{id}")]
        public IActionResult ActiveSkill(string id)
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
                    var staffId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(staffId, secrectKey))
                    {
                        return Unauthorized();
                    }
                    else
                    {
                        return skillService.ActiveSkill(id) ? Ok() : BadRequest();
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
