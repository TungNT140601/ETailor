using Etailor.API.Service.Interface;
using Etailor.API.Ultity.CustomException;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService roleService;
        public RoleController(IRoleService roleService)
        {
            this.roleService = roleService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                return Ok(new
                {
                    Status = 200,
                    Data = roleService.GetRoles()
                });
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
