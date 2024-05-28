using AutoMapper;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using Etailor.API.Ultity;
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
        private readonly ITaskService taskService;
        private readonly IMapper mapper;
        private readonly string wwwrootPath;

        public ProductController(IProductService productService, IProductTemplateService productTemplateService
            , ITaskService taskService, IMapper mapper, IStaffService staffService, ICustomerService customerService
            , IWebHostEnvironment webHost)
        {
            this.productService = productService;
            this.productTemplateService = productTemplateService;
            this.taskService = taskService;
            this.mapper = mapper;
            this.customerService = customerService;
            this.staffService = staffService;
            this.wwwrootPath = webHost.WebRootPath;
        }

        [HttpPost("{orderId}")]
        public async Task<IActionResult> AddProduct(string orderId, int? quantity, [FromBody] ProductOrderVM productVM)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (role != RoleName.MANAGER && role != RoleName.STAFF)
                {
                    return Unauthorized("Không có quyền truy cập");
                }
                else
                {
                    var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(id, secrectKey))
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        var product = mapper.Map<Product>(productVM);
                        var productComponents = new List<ProductComponent>();

                        if (productVM.ProductComponents != null && productVM.ProductComponents.Any())
                        {
                            var tasks = new List<Task>();
                            foreach (var component in productVM.ProductComponents)
                            {
                                tasks.Add(Task.Run(async () =>
                                {
                                    var productComponent = mapper.Map<ProductComponent>(component);
                                    if (component.NoteImageFiles != null && component.NoteImageFiles.Any())
                                    {
                                        var insideTasks = new List<Task>();
                                        var images = new List<string>();
                                        foreach (var image in component.NoteImageFiles)
                                        {
                                            insideTasks.Add(Task.Run(() =>
                                            {
                                                images.Add(JsonConvert.SerializeObject(new FileDTO()
                                                {
                                                    Base64String = image.Base64String,
                                                    FileName = image.FileName,
                                                    ContentType = image.Type
                                                }));
                                            }));
                                        }
                                        await Task.WhenAll(insideTasks);
                                        productComponent.NoteImage = JsonConvert.SerializeObject(images);
                                    }
                                    productComponents.Add(productComponent);
                                }));
                            }
                            await Task.WhenAll(tasks);
                        }

                        var check = await productService.AddProduct(wwwrootPath, orderId, product, productComponents,
                            productVM.MaterialId, productVM.ProfileId, productVM.IsCusMaterial.HasValue ? productVM.IsCusMaterial.Value : false,
                            productVM.MaterialQuantity.HasValue ? productVM.MaterialQuantity.Value : 0, (quantity != null && quantity >= 1) ? quantity.Value : 1);
                        return !string.IsNullOrEmpty(check) ? Ok("Thêm sản phẩm vào hóa đơn thành công") : BadRequest("Thêm sản phẩm vào hóa đơn thất bại");
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

        [HttpPut("{orderId}/{productId}")]
        public async Task<IActionResult> UpdateProduct(string orderId, string productId, [FromBody] ProductOrderVM productVM)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (role != RoleName.MANAGER && role != RoleName.STAFF)
                {
                    return Unauthorized("Không có quyền truy cập");
                }
                else
                {
                    var staffid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(staffid, secrectKey))
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        var product = mapper.Map<Product>(productVM);
                        product.Id = productId;
                        var productComponents = new List<ProductComponent>();

                        if (productVM.ProductComponents != null && productVM.ProductComponents.Any())
                        {
                            var tasks = new List<Task>();
                            foreach (var component in productVM.ProductComponents)
                            {
                                tasks.Add(Task.Run(async () =>
                                {
                                    var productComponent = mapper.Map<ProductComponent>(component);

                                    var images = new List<string>();

                                    if (component.NoteImageObjects != null && component.NoteImageObjects.Any())
                                    {
                                        foreach (var img in component.NoteImageObjects)
                                        {
                                            images.Add(img);
                                        }
                                    }
                                    if (component.NoteImageFiles != null && component.NoteImageFiles.Any())
                                    {
                                        var insideTasks = new List<Task>();
                                        foreach (var image in component.NoteImageFiles)
                                        {
                                            insideTasks.Add(Task.Run(() =>
                                            {
                                                images.Add(JsonConvert.SerializeObject(new FileDTO()
                                                {
                                                    Base64String = image.Base64String,
                                                    FileName = image.FileName,
                                                    ContentType = image.Type
                                                }));
                                            }));
                                        }
                                        await Task.WhenAll(insideTasks);
                                        productComponent.NoteImage = JsonConvert.SerializeObject(images);
                                    }
                                    productComponents.Add(productComponent);
                                }));
                            }
                            await Task.WhenAll(tasks);
                        }

                        var check = await productService.UpdateProduct(wwwrootPath, orderId, product, productComponents,
                             productVM.MaterialId, productVM.ProfileId, productVM.IsCusMaterial.HasValue ? productVM.IsCusMaterial.Value : false,
                             productVM.MaterialQuantity.HasValue ? productVM.MaterialQuantity.Value : 0);
                        return !string.IsNullOrEmpty(check) ? Ok(check) : BadRequest("Cập nhật sản phẩm thất bại");
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

        [HttpPut("{orderId}/{productId}/price")]
        public async Task<IActionResult> UpdateProductPrice(string orderId, string productId, decimal? price)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (role != RoleName.MANAGER)
                {
                    return Unauthorized("Không có quyền truy cập");
                }
                else
                {
                    var staffid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(staffid, secrectKey))
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        return await productService.UpdateProductPrice(orderId, productId, price) ? Ok("Cập nhật giá sản phẩm thành công") : BadRequest("Cập nhật giá sản phẩm thất bại");
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

        [HttpPut("{orderId}/{productId}/defects")]
        public async Task<IActionResult> DefectsProduct(string orderId, string productId)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (role != RoleName.MANAGER)
                {
                    return Unauthorized("Không có quyền truy cập");
                }
                else
                {
                    var staffid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(staffid, secrectKey))
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        return await taskService.DefectsTask(productId, orderId) ? Ok("Báo lỗi sản phẩm thành công") : BadRequest("Báo lỗi sản phẩm thất bại");
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

        [HttpDelete("{orderId}/{id}")]
        public async Task<IActionResult> DeleteProduct(string orderId, string id)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (role != RoleName.MANAGER && role != RoleName.STAFF)
                {
                    return Unauthorized("Không có quyền truy cập");
                }
                else
                {
                    var staffid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(staffid, secrectKey))
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        if (id == null)
                        {
                            return NotFound("Id sản phẩm không tồn tại");
                        }
                        return (await productService.DeleteProduct(orderId, id)) ? Ok("Xóa sản phẩm thành công") : BadRequest("Xóa sản phẩm thất bại");
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

        [HttpGet("order/{orderId}/{id}")]
        public async Task<IActionResult> GetProduct(string orderId, string id)
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
                        var product = new Product();
                        if (role == RoleName.CUSTOMER)
                        {
                            product = await productService.GetProductOrderByCus(id, orderId, staffid);
                        }
                        else
                        {
                            product = await productService.GetProductOrder(id, orderId);
                        }

                        if (product != null)
                        {
                            var tasks = new List<Task>();
                            var productVM = mapper.Map<ProductDetailOrderVM>(product);

                            if (!string.IsNullOrWhiteSpace(product.SaveOrderComponents))
                            {
                                var productComponents = JsonConvert.DeserializeObject<List<ProductComponent>>(product.SaveOrderComponents);

                                if (productComponents != null && productComponents.Any() && productComponents.Count > 0)
                                {
                                    var componentIds = productComponents.Select(c => c.ComponentId).ToList();

                                    productVM.ComponentTypeOrders = mapper.Map<List<ComponentTypeOrderVM>>((await productTemplateService.GetTemplateComponent(product.ProductTemplateId))?.ToList());

                                    tasks.Add(Task.Run(() =>
                                    {
                                        if (product.ProductTemplate != null)
                                        {
                                            productVM.ProductTemplateName = product.ProductTemplate.Name;
                                            productVM.ProductTemplateId = product.ProductTemplate.Id;
                                            productVM.ProductTemplateImage = product.ProductTemplate.ThumbnailImage;
                                        }
                                    }));
                                    tasks.Add(Task.Run(() =>
                                    {
                                        if (product.ProductTemplate != null)
                                        {
                                            productVM.MaterialId = product.FabricMaterialId;
                                            productVM.ProfileId = product.ReferenceProfileBodyId;
                                        }
                                    }));

                                    if (productVM.ComponentTypeOrders != null && productVM.ComponentTypeOrders.Any() && productVM.ComponentTypeOrders.Count > 0)
                                    {
                                        foreach (var component in productVM.ComponentTypeOrders)
                                        {
                                            tasks.Add(Task.Run(async () =>
                                            {
                                                var insideTasks = new List<Task>();

                                                insideTasks.Add(Task.Run(() =>
                                                {
                                                    component.Component_Id = $"component_{component.Id}";
                                                    component.Note_Id = $"productComponent_{component.Id}";
                                                }));

                                                insideTasks.Add(Task.Run(() =>
                                                {
                                                    if (role == RoleName.CUSTOMER)
                                                    {
                                                        if (component.Components != null && component.Components.Any() && component.Components.Count > 0)
                                                        {
                                                            component.Components.RemoveAll(x => !componentIds.Contains(x.Id));
                                                        }
                                                    }
                                                    else if (component.Components != null && component.Components.Any())
                                                    {
                                                        component.Selected_Component_Id = component.Components.FirstOrDefault(x => componentIds.Contains(x.Id))?.Id;
                                                    }
                                                }));

                                                insideTasks.Add(Task.Run(async () =>
                                                {
                                                    var componentNote = productComponents.FirstOrDefault(x => component.Components.Select(c => c.Id).Contains(x.ComponentId));
                                                    if (componentNote != null && (!string.IsNullOrEmpty(componentNote.Note) || !string.IsNullOrEmpty(componentNote.NoteImage)))
                                                    {
                                                        component.NoteObject = new ComponentNoteVM();

                                                        if (!string.IsNullOrEmpty(componentNote.Note))
                                                        {
                                                            component.NoteObject.Note = componentNote.Note;
                                                        }

                                                        if (!string.IsNullOrEmpty(componentNote.NoteImage))
                                                        {
                                                            var listImageDTO = JsonConvert.DeserializeObject<List<string>>(componentNote.NoteImage);
                                                            if (listImageDTO != null && listImageDTO.Any())
                                                            {
                                                                var listImageUrl = new List<string>();
                                                                var insideTasks1 = new List<Task>();
                                                                foreach (var img in listImageDTO)
                                                                {
                                                                    insideTasks1.Add(Task.Run(() =>
                                                                    {
                                                                        listImageUrl.Add(Ultils.GetUrlImage(img));
                                                                    }));
                                                                }
                                                                await Task.WhenAll(insideTasks1);
                                                                component.NoteObject.NoteImage = JsonConvert.SerializeObject(listImageUrl);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        component.NoteObject = null;
                                                    }
                                                }));

                                                await Task.WhenAll(insideTasks);
                                            }));
                                        }
                                    }
                                }
                            }

                            await Task.WhenAll(tasks);

                            return Ok(productVM);
                        }

                        return NotFound(id);
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
                        IEnumerable<Product> products;

                        if (role == RoleName.CUSTOMER)
                        {
                            products = await productService.GetProductsByOrderIdOfCus(orderId, staffid);
                        }
                        else
                        {
                            products = await productService.GetProductsByOrderId(orderId);
                        }

                        List<ProductListOrderDetailVM> productVms = new List<ProductListOrderDetailVM>();

                        if (products != null && products.Any() && products.Count() > 0)
                        {
                            foreach (var product in products.ToList())
                            {
                                var productVM = mapper.Map<ProductListOrderDetailVM>(product);
                                if (product.ProductTemplate == null)
                                {
                                    product.ProductTemplate = await productTemplateService.GetById(product.ProductTemplateId);
                                }
                                if (!string.IsNullOrWhiteSpace(product.ProductTemplate.ThumbnailImage))
                                {
                                    productVM.TemplateThumnailImage = product.ProductTemplate.ThumbnailImage;
                                }
                                productVms.Add(productVM);
                            }
                        }
                        return Ok(productVms);
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

        [HttpGet("{productId}/order/{orderId}/bodySize")]
        public async Task<IActionResult> GetBodySizeOfProduct(string productId, string orderId)
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
                        var bodySizes = await productService.GetBodySizeOfProduct(productId, orderId, role == RoleName.CUSTOMER ? staffid : null);
                        return Ok(mapper.Map<List<ProductBodySizeTaskDetailVM>>(bodySizes));
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
