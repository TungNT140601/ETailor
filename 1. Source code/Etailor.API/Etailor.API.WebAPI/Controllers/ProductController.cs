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
    [Route("api/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService productService;
        private readonly IProductTemplateService productTemplateService;
        private readonly IOrderService orderService;
        private readonly IMapper mapper;

        public ProductController(IProductService productService, IProductTemplateService productTemplateService, IOrderService orderService, IMapper mapper)
        {
            this.productService = productService;
            this.productTemplateService = productTemplateService;
            this.orderService = orderService;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] ProductVM product)
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
                //    return Forbid("Không có quyền truy cập");
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
                        return (await productService.AddProduct(mapper.Map<Product>(product))) ? Ok("Thêm sản phẩm vào thành công") : BadRequest("Thêm sản phẩm vào thất bại");
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
        public async Task<IActionResult> UpdateProduct(string id, [FromBody] ProductVM productVM)
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
                //    return Forbid("Không có quyền truy cập");
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
                        if (id == null || id != productVM.Id)
                        {
                            return NotFound("Id danh mục không tồn tại");
                        }
                        return (await productService.UpdateProduct(mapper.Map<Product>(productVM))) ? Ok("Cập nhật sản phẩm thành công") : BadRequest("Cập nhật sản phẩm thất bại");
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
        public async Task<IActionResult> DeleteProduct(string id)
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
                //    return Forbid("Không có quyền truy cập");
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
                            return NotFound("Id sản phẩm không tồn tại");
                        }
                        return (await productService.DeleteProduct(id)) ? Ok("Xóa sản phẩm thành công") : BadRequest("Xóa sản phẩm thất bại");
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
        public async Task<IActionResult> GetProduct(string id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound("Id Product không tồn tại");
                }
                else
                {
                    var product = productService.GetProduct(id);
                    return product != null ? Ok(mapper.Map<ProductVM>(product)) : NotFound(id);
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
        [HttpGet("/get-product-by-order-id")]
        public async Task<IActionResult> GetProductsByOrderId(string? orderId)
        {
            try
            {
                return Ok(mapper.Map<IEnumerable<ProductVM>>(productService.GetProductsByOrderId(orderId)));
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
