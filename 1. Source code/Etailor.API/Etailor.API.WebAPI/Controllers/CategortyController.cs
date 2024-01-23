using AutoMapper;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Ultity.CustomException;
using Etailor.API.WebAPI.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/category-management")]
    [ApiController]
    public class CategortyController : ControllerBase
    {

        private readonly IStaffService staffService;
        private readonly ICategoryService categoryService;
        private readonly IMapper mapper;

        public CategortyController(IStaffService staffService, IMapper mapper, ICategoryService categoryService)
        {
            this.staffService = staffService;
            this.mapper = mapper;
            this.categoryService = categoryService;
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] CategoryVM category)
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
                        return categoryService.AddCategory(mapper.Map<Category>(category)) ? Ok() : BadRequest();
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(string id, [FromBody] CategoryVM category)
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
                    var staffid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(staffid, secrectKey))
                    {
                        return Unauthorized();
                    }
                    else
                    {
                        if (id == null || id != category.Id)
                        {
                            return NotFound("Id danh mục không tồn tại");
                        }
                        return categoryService.UpdateCategory(mapper.Map<Category>(category)) ? Ok() : BadRequest();
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(string id)
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
                    var staffid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(staffid, secrectKey))
                    {
                        return Unauthorized();
                    }
                    else
                    {
                        if (id == null)
                        {
                            return NotFound("Id danh mục không tồn tại");
                        }
                        return categoryService.DeleteCategory(id) ? Ok() : BadRequest();
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(string id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound("Id danh mục không tồn tại");
                }
                else
                {
                    var category = categoryService.GetCategory(id);
                    return category != null ? Ok(mapper.Map<CategoryVM>(category)) : NotFound(id);
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
        [HttpGet]
        public async Task<IActionResult> GetCategorys(string? search)
        {
            try
            {
                return Ok(mapper.Map<IEnumerable<CategoryVM>>(categoryService.GetCategorys(search)));
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
