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
        private readonly IStaffService staffService;
        private readonly ICustomerService customerService;
        private readonly IProductTemplateService productTemplateService;
        private readonly IOrderService orderService;
        private readonly IMapper mapper;

        public ProductController(IProductService productService, IProductTemplateService productTemplateService, IOrderService orderService, IMapper mapper, IStaffService staffService, ICustomerService customerService)
        {
            this.productService = productService;
            this.productTemplateService = productTemplateService;
            this.orderService = orderService;
            this.mapper = mapper;
            this.customerService = customerService;
            this.staffService = staffService;
        }

        [HttpPost("{orderId}")]
        public async Task<IActionResult> AddProduct(string orderId, [FromBody] ProductOrderVM productVM)
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
                //check quantity product
                var product = mapper.Map<Product>(productVM);
                var productComponents = mapper.Map<List<ProductComponent>>(productVM.ProductComponents);
                var check = await productService.AddProduct(orderId, product, productComponents,
                    productVM.MaterialId, productVM.ProfileId, productVM.IsCusMaterial.HasValue ? productVM.IsCusMaterial.Value : false,
                    productVM.MaterialQuantity.HasValue ? productVM.MaterialQuantity.Value : 0);
                return !string.IsNullOrEmpty(check) ? Ok(check) : BadRequest("Thêm sản phẩm vào hóa đơn thất bại");
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

        [HttpPut("{orderId}")]
        public async Task<IActionResult> UpdateProduct(string orderId, [FromBody] ProductOrderVM productVM)
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
                var product = mapper.Map<Product>(productVM);
                var productComponents = mapper.Map<List<ProductComponent>>(productVM.ProductComponents);
                var check = await productService.UpdateProduct(orderId, product, productComponents,
                     productVM.MaterialId, productVM.ProfileId, productVM.IsCusMaterial.HasValue ? productVM.IsCusMaterial.Value : false,
                     productVM.MaterialQuantity.HasValue ? productVM.MaterialQuantity.Value : 0);
                return !string.IsNullOrEmpty(check) ? Ok(check) : BadRequest("Cập nhật sản phẩm thất bại");
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
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else
                {
                    var staffid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if ((role != RoleName.CUSTOMER && !staffService.CheckSecrectKey(staffid, secrectKey)) || (role == RoleName.CUSTOMER && !customerService.CheckSecerctKey(staffid, secrectKey)))
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        var returnData = new List<Product>();
                        if (role == RoleName.CUSTOMER)
                        {
                            returnData = productService.GetProductsByOrderIdOfCus(orderId, staffid).ToList();
                        }
                        else
                        {
                            returnData = productService.GetProductsByOrderId(orderId).ToList();
                        }
                        return Ok(mapper.Map<IEnumerable<ProductVM>>(returnData));
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
