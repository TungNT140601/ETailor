using AutoMapper;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Repository;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Ultity.CustomException;
using Etailor.API.WebAPI.ViewModels;
using Google.Apis.Util;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IStaffService staffService;
        private readonly ICustomerService customerService;
        private readonly IOrderService orderService;
        private readonly IProductService productService;
        private readonly IProductTemplateService productTemplateService;
        private readonly IDiscountService discountService;
        private readonly IMapper mapper;

        public OrderController(IStaffService staffService, ICustomerService customerService,
            IOrderService orderService, IMapper mapper, IProductService productService, IProductTemplateService productTemplateService,
            IDiscountService discountService)
        {
            this.mapper = mapper;
            this.staffService = staffService;
            this.customerService = customerService;
            this.orderService = orderService;
            this.productService = productService;
            this.productTemplateService = productTemplateService;
            this.discountService = discountService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateVM orderVM)
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
                        var order = mapper.Map<Order>(orderVM);

                        order.CreaterId = id;

                        var orderId = await orderService.CreateOrder(order, role);
                        return orderId != null ? Ok(orderId) : BadRequest("Tạo mới hóa đơn thất bại");
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
        public async Task<IActionResult> UpdateOrder(string id, [FromBody] OrderCreateVM orderVM)
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
                        var order = mapper.Map<Order>(orderVM);

                        order.CreaterId = id;

                        var orderId = await orderService.UpdateOrder(order, role);

                        return orderId != null ? Ok(orderId) : BadRequest("Cập nhật hóa đơn thất bại");
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

        [HttpDelete("cancel/{id}")]
        public async Task<IActionResult> DeleteOrder(string id)
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
                        return await orderService.DeleteOrder(id) ? Ok("Hủy hóa đơn thành công") : BadRequest("Hủy hóa đơn thất bại");
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
        public async Task<IActionResult> GetOrder(string id)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (role == RoleName.ADMIN)
                {
                    return Unauthorized("Không có quyền truy cập");
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
                        var order = new OrderDetailVM();

                        if (role == RoleName.CUSTOMER)
                        {
                            order = mapper.Map<OrderDetailVM>(await orderService.GetOrderByCustomer(staffid, id));
                        }
                        else
                        {
                            order = mapper.Map<OrderDetailVM>(await orderService.GetOrder(id));
                        }

                        if (order != null)
                        {
                            if (!string.IsNullOrWhiteSpace(order.DiscountId))
                            {
                                order.Discount = mapper.Map<DiscountOrderDetailVM>(discountService.GetDiscount(order.DiscountId));
                            }
                            if (!string.IsNullOrWhiteSpace(order.CustomerId))
                            {
                                order.Customer = mapper.Map<CustomerAllVM>(customerService.FindById(order.CustomerId));

                                order.Customer.Avatar = Ultils.GetUrlImage(order.Customer.Avatar);
                            }

                            var listProducts = await productService.GetProductsByOrderId(order.Id);

                            if (listProducts != null && listProducts.Any() && listProducts.Count() > 0)
                            {
                                order.Products = new List<ProductListOrderDetailVM>();
                                foreach (var product in listProducts)
                                {
                                    var productVM = mapper.Map<ProductListOrderDetailVM>(product);

                                    if (product.ProductTemplate == null)
                                    {
                                        product.ProductTemplate = await productTemplateService.GetById(product.ProductTemplateId);
                                    }
                                    productVM.TemplateThumnailImage = product.ProductTemplate.ThumbnailImage;
                                    productVM.TemplateName = product.ProductTemplate.Name;

                                    order.Products.Add(productVM);
                                }
                            }
                            return Ok(order);
                        }
                        return NotFound();
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

        [HttpGet]
        public async Task<IActionResult> GetOrders()
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
                    else if (role == RoleName.ADMIN)
                    {
                        return Unauthorized("Không có quyền truy cập");
                    }
                    else
                    {
                        IEnumerable<Order> orders;

                        if (role == RoleName.CUSTOMER)
                        {
                            orders = orderService.GetOrdersByCustomer(staffid);
                        }
                        else
                        {
                            orders = orderService.GetOrders();
                        }

                        var getOrderVMs = new List<GetOrderVM>();

                        if (orders != null && orders.Any() && orders.Count() > 0)
                        {
                            orders = orders.ToList();

                            var listProducts = await productService.GetProductsByOrderIds(orders.Select(x => x.Id).ToList());
                            if (listProducts != null && listProducts.Any())
                            {
                                listProducts = listProducts.ToList();
                                var tasks = new List<Task>();
                                foreach (var order in orders.ToList())
                                {
                                    tasks.Add(Task.Run(async () =>
                                    {
                                        var realOrder = mapper.Map<GetOrderVM>(order);
                                        var firstProductOrder = listProducts.FirstOrDefault(x => x.OrderId == order.Id);
                                        if (firstProductOrder != null)
                                        {
                                            if (firstProductOrder.ProductTemplate == null)
                                            {
                                                firstProductOrder.ProductTemplate = await productTemplateService.GetById(listProducts.First().ProductTemplateId);
                                            }

                                            realOrder.ThumbnailImage = firstProductOrder.ProductTemplate.ThumbnailImage;
                                        }
                                        realOrder.CreatedTime = order.CreatedTime;

                                        getOrderVMs.Add(realOrder);
                                    }));
                                }
                                await Task.WhenAll(tasks);
                            }
                        }
                        return Ok(getOrderVMs);
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

        [HttpPatch("finish/{orderId}")]
        public async Task<IActionResult> FinishOrder(string orderId)
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
                        return await orderService.FinishOrder(orderId, role) ? Ok("Tạo hóa đơn thành công") : BadRequest("Tạo hóa đơn thất bại");
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

        [HttpPatch("approve/{orderId}")]
        public async Task<IActionResult> ApproveOrder(string orderId)
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
                        return await orderService.ApproveOrder(orderId) ? Ok("Tạo hóa đơn thành công") : BadRequest("Tạo hóa đơn thất bại");
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

        [HttpPatch("{orderId}/price")]
        public async Task<IActionResult> ChangeOrderPrice(string orderId, double? price)
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
                        if (price is null || price == 0)
                        {
                            return BadRequest("Giá không được để trống");
                        }
                        else if (price < 0)
                        {
                            return BadRequest("Giá không được nhỏ hơn 0");
                        }
                        else
                        {
                            int.TryParse(price.ToString(), out int priceInt);

                            return await orderService.UpdateOrderPrice(orderId, priceInt) ? Ok("Cập nhật giá hóa đơn thành công") : BadRequest("Cập nhật giá hóa đơn thất bại");
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
