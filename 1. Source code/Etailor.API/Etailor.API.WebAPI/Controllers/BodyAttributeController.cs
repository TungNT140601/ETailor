using AutoMapper;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity.CustomException;
using Etailor.API.WebAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/body-attribute")]
    [ApiController]
    public class BodyAttributeController : ControllerBase
    {
        private readonly IBodyAttributeService bodyAttributeService;
        private readonly IProfileBodyService profileBodyService;
        private readonly IBodySizeService bodySizeService;
        private readonly IMapper mapper;

        public BodyAttributeController(IBodyAttributeService bodyAttributeService, IMapper mapper)
        {
            this.bodyAttributeService = bodyAttributeService;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> AddBodyAttribute([FromBody] BodyAttributeVM bodyAttributeVM)
        {
            try
            {
                //var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                //if (role == null)
                //{
                //    return Unauthorized("Chưa đăng nhập");
                //}
                //else if (role != RoleName.MANAGER)
                //{
                //    return Unauthorized("Không có quyền truy cập");
                //}
                //else
                //{
                //var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                //var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                //if (!staffService.CheckSecrectKey(id, secrectKey))
                //{
                //    return Unauthorized("Chưa đăng nhập");
                //}
                //else
                //{
                return (await bodyAttributeService.AddBodyAttribute(mapper.Map<BodyAttribute>(bodyAttributeVM))) ? Ok("Thêm số đo thành công") : BadRequest("Thêm số đo thất bại");
                //}
                //}
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBodyAttribute(string id, [FromBody] BodyAttributeVM bodyAttributeVM)
        {
            try
            {
                //var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                //if (role == null)
                //{
                //    return Unauthorized("Chưa đăng nhập");
                //}
                //else if (role != RoleName.MANAGER)
                //{
                //    return Unauthorized("Không có quyền truy cập");
                //}
                //else
                //{
                //    var staffid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                //    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                //    if (!staffService.CheckSecrectKey(staffid, secrectKey))
                //    {
                //        return Unauthorized("Chưa đăng nhập");
                //    }
                //    else
                //    {
                if (id == null || id != bodyAttributeVM.Id)
                {
                    return NotFound("Id số đo không tồn tại");
                }
                return (await bodyAttributeService.UpdateBodyAttribute(mapper.Map<BodyAttribute>(bodyAttributeVM))) ? Ok("Cập nhật số đo thành công") : BadRequest("Cập nhật số đo thất bại");
                //    }
                //}
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBodyAttribute(string id)
        {
            try
            {
                //var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                //if (role == null)
                //{
                //    return Unauthorized("Chưa đăng nhập");
                //}
                //else if (role != RoleName.MANAGER)
                //{
                //    return Unauthorized("Không có quyền truy cập");
                //}
                //else
                //{
                //var staffid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                //var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                //if (!staffService.CheckSecrectKey(staffid, secrectKey))
                //{
                //    return Unauthorized("Chưa đăng nhập");
                //}
                //else
                //{
                if (id == null)
                {
                    return NotFound("Id số đo không tồn tại");
                }
                return (await bodyAttributeService.DeleteBodyAttribute(id)) ? Ok("Xóa số đo thành công") : BadRequest("Xóa số đo thất bại");
                //    }
                //}
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBodyAttribute(string id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound("Id số đo không tồn tại");
                }
                else
                {
                    var bodyAttribute = bodyAttributeService.GetBodyAttribute(id);
                    return bodyAttribute != null ? Ok(mapper.Map<BodyAttributeVM>(bodyAttribute)) : NotFound(id);
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

        [HttpGet("/get-body-attributes-by-profile-body-id")]
        public async Task<IActionResult> GetBodyAttributesByProfileBodyId(string? profileBodyId)
        {
            try
            {
                return Ok(mapper.Map<IEnumerable<BodyAttributeVM>>(bodyAttributeService.GetBodyAttributesByProfileBodyId(profileBodyId)));
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

        [HttpGet("/get-body-attributes-by-body-size-id")]
        public async Task<IActionResult> GetProfileBodysByBodySizeId(string? bodySizeId)
        {
            try
            {
                return Ok(mapper.Map<IEnumerable<BodyAttributeVM>>(bodyAttributeService.GetBodyAttributesByBodySizeId(bodySizeId)));
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
