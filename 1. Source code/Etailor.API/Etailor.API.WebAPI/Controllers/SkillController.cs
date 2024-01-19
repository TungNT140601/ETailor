using AutoMapper;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
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
        private readonly IConfiguration configuration;
        private readonly IMapper mapper;

        public SkillController(ISkillService skillService, IConfiguration configuration, IMapper mapper)
        {
            this.skillService = skillService;
            this.configuration = configuration;
            this.mapper = mapper;
        }

        //[HttpPost("/managed-skill")]
        //public IActionResult CreateSkill([FromBody] SkillVM skillVM)
        //{
        //    try
        //    {
                
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
    }
}
