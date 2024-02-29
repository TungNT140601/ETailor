using AutoMapper;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Ultity.CustomException;
using Etailor.API.WebAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
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

        [HttpGet("order/{orderId}/{id}")]
        public async Task<IActionResult> GetProduct(string orderId, string id)
        {
            try
            {
                //var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                //if (role == null)
                //{
                //    return Unauthorized("Chưa đăng nhập");
                //}
                //else
                //{
                //var staffid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                //    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                //    if ((role != RoleName.CUSTOMER && !staffService.CheckSecrectKey(staffid, secrectKey)) || (role == RoleName.CUSTOMER && !customerService.CheckSecerctKey(staffid, secrectKey)))
                //    {
                //        return Unauthorized("Chưa đăng nhập");
                //    }
                //    else
                //    {
                var product = new Product();
                //if (role == RoleName.CUSTOMER)
                //{
                //    product = await productService.GetProductOrderByCus(id, orderId, staffid);
                //}
                //else
                //{
                product = await productService.GetProductOrder(id, orderId);
                //}

                if (product != null)
                {
                    var productVM = mapper.Map<ProductDetailOrderVM>(product);

                    if (!string.IsNullOrWhiteSpace(product.SaveOrderComponents))
                    {
                        var productComponents = JsonConvert.DeserializeObject<List<ProductComponent>>(product.SaveOrderComponents);

                        if (productComponents != null && productComponents.Any() && productComponents.Count > 0)
                        {
                            var componentIds = productComponents.Select(c => c.ComponentId).ToList();

                            productVM.ComponentTypeOrders = mapper.Map<List<ComponentTypeOrderVM>>(productTemplateService.GetTemplateComponent(product.ProductTemplateId).ToList());

                            if (productVM.ComponentTypeOrders != null && productVM.ComponentTypeOrders.Any() && productVM.ComponentTypeOrders.Count > 0)
                            {
                                var tasks = new List<Task>();
                                foreach (var component in productVM.ComponentTypeOrders)
                                {
                                    tasks.Add(Task.Run(() =>
                                    {
                                        if (component.Components != null && component.Components.Any() && component.Components.Count > 0)
                                        {
                                            component.Components.RemoveAll(x => !componentIds.Contains(x.Id));
                                        }
                                    }));
                                }
                                await Task.WhenAll(tasks);
                            }

                            productVM.MaterialId = productComponents.First().ProductComponentMaterials.First().MaterialId;
                        }
                    }

                    return Ok(productVM);
                }

                return NotFound(id);
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


        [HttpGet("order/{orderId}")]
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
                        if (role == RoleName.CUSTOMER)
                        {
                            var products = await productService.GetProductsByOrderIdOfCus(orderId, staffid);
                            List<ProductListOrderDetailVM> productVms = new List<ProductListOrderDetailVM>();
                            foreach (var product in products)
                            {
                                var productVM = mapper.Map<ProductListOrderDetailVM>(product);
                                var template = await productTemplateService.GetById(product.ProductTemplateId);
                                if (template != null)
                                {
                                    if (!string.IsNullOrWhiteSpace(template.ThumbnailImage))
                                    {
                                        productVM.TemplateThumnailImage = template.ThumbnailImage;
                                    }
                                }
                                productVms.Add(productVM);
                            }
                            return Ok(productVms);
                        }
                        else
                        {
                            var products = await productService.GetProductsByOrderId(orderId);
                            List<ProductListOrderDetailVM> productVms = new List<ProductListOrderDetailVM>();
                            foreach (var product in products)
                            {
                                var productVM = mapper.Map<ProductListOrderDetailVM>(product);
                                var template = await productTemplateService.GetById(product.ProductTemplateId);
                                if (template != null)
                                {
                                    if (!string.IsNullOrWhiteSpace(template.ThumbnailImage))
                                    {
                                        productVM.TemplateThumnailImage = template.ThumbnailImage;
                                    }
                                }
                                productVms.Add(productVM);
                            }
                            return Ok(productVms);
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
