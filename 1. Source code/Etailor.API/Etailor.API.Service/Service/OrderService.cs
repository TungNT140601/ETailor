using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.Repository;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity.CustomException;
using Etailor.API.Ultity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Service.Service;
using System.Reflection.Metadata;
using Newtonsoft.Json;

namespace Etailor.API.Service.Service
{
    public class OrderService : IOrderService
    {
        private readonly IStaffRepository staffRepository;
        private readonly ICustomerRepository customerRepository;
        private readonly IOrderRepository orderRepository;
        private readonly IDiscountRepository discountRepository;
        private readonly IProductRepository productRepository;
        private readonly IPaymentRepository paymentRepository;
        private readonly IProductTemplateRepository productTemplaTeRepository;
        private readonly IProductTemplateService productTemplateService;
        private readonly IProductStageRepository productStageRepository;
        private readonly IOrderMaterialRepository orderMaterialRepository;
        private readonly IMaterialRepository materialRepository;

        public OrderService(IStaffRepository staffRepository, ICustomerRepository customerRepository, IOrderRepository orderRepository,
            IDiscountRepository discountRepository, IProductRepository productRepository, IPaymentRepository paymentRepository,
            IProductTemplateRepository productTemplaTeRepository, IProductTemplateService productTemplateService,
            IProductStageRepository productStageRepository, IOrderMaterialRepository orderMaterialRepository,
            IMaterialRepository materialRepository)
        {
            this.staffRepository = staffRepository;
            this.customerRepository = customerRepository;
            this.orderRepository = orderRepository;
            this.discountRepository = discountRepository;
            this.productRepository = productRepository;
            this.paymentRepository = paymentRepository;
            this.productTemplaTeRepository = productTemplaTeRepository;
            this.productTemplateService = productTemplateService;
            this.productStageRepository = productStageRepository;
            this.orderMaterialRepository = orderMaterialRepository;
            this.materialRepository = materialRepository;
        }

        public async Task<string> CreateOrder(Order order, string? role)
        {
            var tasks = new List<Task>();
            if (string.IsNullOrWhiteSpace(order.Id))
            {
                if (!string.IsNullOrWhiteSpace(order.CustomerId))
                {
                    var cus = customerRepository.Get(order.CustomerId);

                    tasks.Add(Task.Run(() =>
                    {
                        if (cus == null || cus.IsActive == false)
                        {
                            throw new UserException("Không tìm thấy khách hàng");
                        }
                    }));
                }

                tasks.Add(Task.Run(() =>
                {
                    order.Id = Ultils.GenGuidString();
                    order.CreatedTime = DateTime.UtcNow.AddHours(7);
                    order.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                    order.InactiveTime = null;
                    order.IsActive = false;
                }));

                tasks.Add(Task.Run(() =>
                {
                    order.Status = 1;
                }));

                await Task.WhenAll(tasks);

                return orderRepository.Create(order) ? order.Id : null;
            }
            else
            {
                var dbOrder = orderRepository.Get(order.Id);
                if (dbOrder != null && dbOrder.IsActive == false && dbOrder.Status <= 2 && dbOrder.InactiveTime == null)
                {
                    var diffCus = false;
                    if (!string.IsNullOrWhiteSpace(order.CustomerId))
                    {
                        var cus = customerRepository.Get(order.CustomerId);

                        tasks.Add(Task.Run(() =>
                        {
                            if (cus == null || cus.IsActive == false)
                            {
                                throw new UserException("Không tìm thấy khách hàng");
                            }
                            else
                            {
                                if (dbOrder.CustomerId != cus.Id)
                                {
                                    dbOrder.CustomerId = cus.Id;
                                    diffCus = true;
                                }
                                else
                                {
                                    diffCus = false;
                                }
                            }
                        }));
                    }

                    tasks.Add(Task.Run(() =>
                    {
                        dbOrder.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                        dbOrder.InactiveTime = null;
                        dbOrder.IsActive = false;
                    }));

                    tasks.Add(Task.Run(() =>
                    {
                        dbOrder.Status = 1;
                    }));

                    await Task.WhenAll(tasks);

                    if (diffCus)
                    {
                        await ClearOrder(dbOrder.Id);
                    }

                    return orderRepository.Update(dbOrder.Id, dbOrder) ? dbOrder.Id : null;
                }
                else
                {
                    throw new UserException("Id hóa đơn không tồn tại");
                }
            }
        }

        private async Task<bool> ClearOrder(string orderId)
        {
            var dbOrder = orderRepository.Get(orderId);
            if (dbOrder != null)
            {
                var orderProducts = productRepository.GetAll(x => x.OrderId == orderId && x.IsActive == true);
                if (orderProducts != null && orderProducts.Any())
                {
                    orderProducts = orderProducts.ToList();

                    var tasks = new List<Task>();
                    foreach (var product in orderProducts)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            product.IsActive = false;
                            product.InactiveTime = DateTime.UtcNow.AddHours(7);
                        }));
                    }
                    await Task.WhenAll(tasks);

                    var checks = new List<bool>();
                    foreach (var product in orderProducts)
                    {
                        checks.Add(productRepository.Update(product.Id, product));
                    }
                    return !checks.Any(x => x == false);
                }
            }
            return false;
        }

        public async Task<bool> FinishOrder(string orderId, string role)
        {
            var dbOrder = orderRepository.Get(orderId);
            if (dbOrder != null)
            {
                if (dbOrder.Status >= 2)
                {
                    throw new UserException("Đơn hàng đã duyệt. Không thể chỉnh sửa");
                }
                else if (dbOrder.Status == 0)
                {
                    throw new UserException("Hóa đơn đã bị hủy. Không thể cập nhật giá");
                }
                else
                {
                    dbOrder.Status = role == RoleName.STAFF ? 1 : role == RoleName.MANAGER ? 2 : 0;

                    if (dbOrder.TotalProduct == 0)
                    {
                        throw new UserException("Không thể hoàn thành hóa đơn không có sản phẩm");
                    }

                    if (dbOrder.PaidMoney == 0)
                    {
                        throw new UserException("Không thể hoàn thành hóa đơn chưa thanh toán hoặc chưa đặt cọc");
                    }

                    dbOrder.IsActive = true;

                    if (orderRepository.Update(dbOrder.Id, dbOrder))
                    {
                        return await CheckOrderPaid(dbOrder.Id);
                    }
                    else
                    {
                        throw new SystemsException("Lỗi trong quá trình cập nhật hóa đơn", nameof(OrderService));
                    }
                }
            }
            else
            {
                throw new UserException("Hóa đơn không tồn tại");
            }
        }

        public async Task<bool> ApplyDiscount(string orderId, string? code)
        {
            var dbOrder = orderRepository.Get(orderId);
            if (dbOrder != null && dbOrder.Status >= 1 && dbOrder.Status <= 2)
            {
                var tasks = new List<Task>();
                if (!string.IsNullOrEmpty(code))
                {
                    var cus = customerRepository.Get(dbOrder.CustomerId);

                    var discount = discountRepository.GetAll(x => code != null && x.Code != null && x.Code.Trim() == code.Trim()).FirstOrDefault();

                    var usedDiscountCode = orderRepository.GetAll(x => x.Id != dbOrder.Id && ((dbOrder.CustomerId != cus.Id && x.CustomerId == cus.Id) || (dbOrder.CustomerId == cus.Id && x.CustomerId == cus.Id)) && x.DiscountCode == code && x.IsActive == true && x.Status != 0);

                    tasks.Add(Task.Run(() =>
                    {
                        if (discount == null || discount.IsActive == false || discount.StartDate >= DateTime.UtcNow.AddHours(7) || discount.EndDate <= DateTime.UtcNow.AddHours(7))
                        {
                            dbOrder.DiscountCode = "";
                            dbOrder.DiscountId = null;
                            throw new UserException("Mã giảm giá không đúng hoặc hết hạn");
                        }
                        else if (usedDiscountCode.Any())
                        {
                            dbOrder.DiscountCode = "";
                            dbOrder.DiscountId = null;
                            throw new UserException("Mã giảm giá đã được sử dụng");
                        }
                        else
                        {
                            dbOrder.DiscountCode = discount.Code;
                            dbOrder.DiscountId = discount.Id;
                        }
                    }));
                }
                else
                {
                    tasks.Add(Task.Run(() =>
                    {
                        dbOrder.DiscountCode = "";
                        dbOrder.DiscountId = null;
                    }));
                }

                tasks.Add(Task.Run(() =>
                {
                    dbOrder.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                    dbOrder.CancelTime = null;
                    dbOrder.InactiveTime = null;
                    dbOrder.IsActive = true;
                }));

                await Task.WhenAll(tasks);

                if (orderRepository.Update(dbOrder.Id, dbOrder))
                {
                    return await CheckOrderPaid(dbOrder.Id);
                }
                else
                {
                    throw new SystemsException("Lỗi trong quá trình cập nhật hóa đơn", nameof(OrderService));
                }
            }
            else
            {
                throw new UserException("Không tìm thấy hóa đơn");
            }
        }

        public async Task<string> UpdateOrder(Order order, string? role)
        {
            var dbOrder = orderRepository.Get(order.Id);
            if (dbOrder != null)
            {
                if (dbOrder.Status < 2 && dbOrder.Status > 0)
                {
                    var tasks = new List<Task>();

                    if (string.IsNullOrWhiteSpace(order.CustomerId))
                    {
                        throw new UserException("Vui lòng chọn khách hàng");
                    }

                    var cus = customerRepository.Get(order.CustomerId);

                    var discount = discountRepository.GetAll(x => order.DiscountCode != null && x.Code != null && x.Code.Trim().ToLower() == order.DiscountCode.Trim().ToLower()).FirstOrDefault();

                    var usedDiscountCode = orderRepository.GetAll(x => x.Id != dbOrder.Id && ((dbOrder.CustomerId != cus.Id && x.CustomerId == cus.Id) || (dbOrder.CustomerId == cus.Id && x.CustomerId == cus.Id)) && x.DiscountCode == order.DiscountCode && x.IsActive == true && x.Status != 0);

                    tasks.Add(Task.Run(() =>
                    {
                        if (cus == null || cus.IsActive == false)
                        {
                            throw new UserException("Không tìm thấy khách hàng");
                        }
                        else
                        {
                            dbOrder.CustomerId = cus.Id;
                        }
                    }));

                    tasks.Add(Task.Run(() =>
                    {
                        if (!string.IsNullOrWhiteSpace(order.DiscountCode))
                        {
                            if (discount == null || discount.IsActive == false || discount.StartDate >= DateTime.UtcNow.AddHours(7) || discount.EndDate <= DateTime.UtcNow.AddHours(7))
                            {
                                throw new UserException("Mã giảm giá không đúng hoặc hết hạn");
                            }
                            else if (usedDiscountCode.Any())
                            {
                                throw new UserException("Mã giảm giá đã được sử dụng");
                            }
                        }
                    }));

                    tasks.Add(Task.Run(() =>
                    {
                        dbOrder.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                        dbOrder.CancelTime = null;
                        dbOrder.InactiveTime = null;
                        dbOrder.IsActive = true;
                    }));

                    await Task.WhenAll(tasks);

                    if (orderRepository.Update(dbOrder.Id, dbOrder))
                    {
                        return await CheckOrderPaid(dbOrder.Id) ? dbOrder.Id : null;
                    }
                    else
                    {
                        throw new SystemsException("Lỗi trong quá trình cập nhật hóa đơn", nameof(OrderService));
                    }
                }
                else
                {
                    throw new UserException("Hóa đơn đã được duyệt. Không thể cập nhập hóa đơn");
                }
            }
            else
            {
                throw new UserException("Không tìm thấy hóa đơn");
            }
        }

        public async Task<bool> PayDeposit(string orderId, decimal amount)
        {
            var dbOrder = orderRepository.Get(orderId);
            if (dbOrder != null)
            {
                dbOrder.Deposit = amount;
                dbOrder.PayDeposit = true;

                if (orderRepository.Update(dbOrder.Id, dbOrder))
                {
                    return await CheckOrderPaid(orderId);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                throw new UserException("Không tìm thấy hóa đơn");
            }
        }

        public async Task<bool> CheckOrderPaid(string id)
        {
            var dbOrder = orderRepository.Get(id);
            if (dbOrder != null)
            {
                var discount = string.IsNullOrEmpty(dbOrder.DiscountId) ? null : discountRepository.Get(dbOrder.DiscountId);

                var orderProducts = productRepository.GetAll(x => x.OrderId == dbOrder.Id && x.IsActive == true && x.Status != 0).ToList();

                var orderPayments = paymentRepository.GetAll(x => x.OrderId == dbOrder.Id && x.Status == 0).ToList();

                var tasks = new List<Task>();

                if (orderProducts.Any() && orderProducts.Count > 0)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        dbOrder.TotalProduct = orderProducts.Count;

                        dbOrder.TotalPrice = orderProducts.Sum(x => x.Price);
                    }));
                }
                else
                {
                    tasks.Add(Task.Run(() =>
                    {
                        dbOrder.TotalProduct = 0;

                        dbOrder.TotalPrice = 0;
                    }));
                }

                await Task.WhenAll(tasks);

                if (discount != null)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        dbOrder.DiscountId = discount.Id;
                        dbOrder.DiscountCode = discount.Code;
                    }));

                    if (discount.ConditionProductMin != null && discount.ConditionProductMin != 0)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            if (dbOrder.TotalProduct >= discount.ConditionProductMin)
                            {
                                if (discount.DiscountPrice != null && discount.DiscountPrice != 0)
                                {
                                    dbOrder.DiscountPrice = discount.DiscountPrice;
                                }
                                else if (discount.DiscountPercent != null && discount.DiscountPercent != 0)
                                {
                                    dbOrder.DiscountPrice = (decimal)((double)dbOrder.TotalPrice * (double)discount.DiscountPercent / 100);
                                }
                            }
                            else
                            {
                                dbOrder.DiscountPrice = 0;
                            }
                        }));
                    }
                    else if (discount.ConditionPriceMin != null && discount.ConditionPriceMin != 0 && discount.ConditionPriceMax == null)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            if (dbOrder.TotalPrice >= discount.ConditionPriceMin)
                            {
                                if (discount.DiscountPrice != null && discount.DiscountPrice != 0)
                                {
                                    dbOrder.DiscountPrice = discount.DiscountPrice;
                                }
                                else if (discount.DiscountPercent != null && discount.DiscountPercent != 0)
                                {
                                    dbOrder.DiscountPrice = (decimal)((double)dbOrder.TotalPrice * (double)discount.DiscountPercent / 100);
                                }
                            }
                            else
                            {
                                dbOrder.DiscountPrice = 0;
                            }
                        }));
                    }
                    else if (discount.ConditionPriceMax != null && discount.ConditionPriceMax != 0 && discount.ConditionPriceMin == null)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            if (dbOrder.TotalPrice <= discount.ConditionPriceMax)
                            {
                                if (discount.DiscountPrice != null && discount.DiscountPrice != 0)
                                {
                                    dbOrder.DiscountPrice = discount.DiscountPrice;
                                }
                                else if (discount.DiscountPercent != null && discount.DiscountPercent != 0)
                                {
                                    dbOrder.DiscountPrice = (decimal)((double)dbOrder.TotalPrice * (double)discount.DiscountPercent / 100);
                                }
                            }
                            else
                            {
                                dbOrder.DiscountPrice = 0;
                            }
                        }));
                    }
                    else if (discount.ConditionPriceMin != null && discount.ConditionPriceMin != 0 && discount.ConditionPriceMax != null && discount.ConditionPriceMax != 0)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            if (dbOrder.TotalPrice <= discount.ConditionPriceMax && dbOrder.TotalPrice >= discount.ConditionPriceMin)
                            {
                                if (discount.DiscountPrice != null && discount.DiscountPrice != 0)
                                {
                                    dbOrder.DiscountPrice = discount.DiscountPrice;
                                }
                                else if (discount.DiscountPercent != null && discount.DiscountPercent != 0)
                                {
                                    dbOrder.DiscountPrice = (decimal)((double)dbOrder.TotalPrice * (double)discount.DiscountPercent / 100);
                                }
                            }
                            else
                            {
                                dbOrder.DiscountPrice = 0;
                            }
                        }));
                    }
                    else
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            dbOrder.DiscountPrice = 0;
                        }));
                    }
                }
                else
                {
                    tasks.Add(Task.Run(() =>
                    {
                        dbOrder.DiscountId = null;
                        dbOrder.DiscountCode = "";
                        dbOrder.DiscountPrice = 0;
                    }));
                }

                await Task.WhenAll(tasks);


                if (dbOrder.DiscountPrice.HasValue && dbOrder.DiscountPrice > 0)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        if (dbOrder.DiscountPrice < dbOrder.TotalPrice)
                        {
                            dbOrder.AfterDiscountPrice = dbOrder.TotalPrice - dbOrder.DiscountPrice;
                        }
                        else
                        {
                            dbOrder.AfterDiscountPrice = 0;
                        }
                    }));
                }
                else
                {
                    tasks.Add(Task.Run(() =>
                    {
                        dbOrder.AfterDiscountPrice = dbOrder.TotalPrice;
                        dbOrder.DiscountPrice = 0;
                    }));
                }

                await Task.WhenAll(tasks);

                if (orderPayments.Any() && orderPayments.Count > 0)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        var paidMoney = orderPayments.Where(x => x.Amount > 0);
                        if (paidMoney.Any())
                        {
                            dbOrder.PaidMoney = paidMoney.Sum(x => x.Amount);
                        }
                        else
                        {
                            dbOrder.PaidMoney = 0;
                        }
                    }));
                }
                else
                {
                    dbOrder.PaidMoney = 0;
                }

                await Task.WhenAll(tasks);

                tasks.Add(Task.Run(() =>
                {
                    if (dbOrder.AfterDiscountPrice.HasValue && dbOrder.AfterDiscountPrice != 0)
                    {
                        dbOrder.UnPaidMoney = dbOrder.AfterDiscountPrice - dbOrder.PaidMoney;
                    }
                    else
                    {
                        dbOrder.UnPaidMoney = dbOrder.TotalPrice - dbOrder.PaidMoney;
                    }
                }));

                await Task.WhenAll(tasks);

                return orderRepository.Update(dbOrder.Id, dbOrder);
            }
            else
            {
                throw new UserException("Không tìm thấy hóa đơn");
            }
        }

        public async Task<bool> ApproveOrder(string id)
        {
            var dbOrder = orderRepository.Get(id);
            if (dbOrder != null && dbOrder.IsActive == true)
            {
                if (dbOrder.Status >= 2)
                {
                    throw new UserException("Hóa đơn đã được duyệt. Không thể duyệt lại hóa đơn");
                }
                else if (dbOrder.Status == 0)
                {
                    throw new UserException("Hóa đơn đã bị hủy. Không thể cập nhật giá");
                }
                else
                {
                    dbOrder.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                    dbOrder.Status = 2;
                    if (orderRepository.Update(dbOrder.Id, dbOrder))
                    {
                        return await CheckOrderPaid(dbOrder.Id);
                    }
                    else
                    {
                        throw new SystemsException("Lỗi trong quá trình cập nhật hóa đơn", nameof(OrderService));
                    }
                }
            }
            else
            {
                throw new UserException("Không tìm thấy hóa đơn");
            }
        }

        public async Task<bool> UpdateOrderPrice(string id, int price)
        {
            var dbOrder = orderRepository.Get(id);
            if (dbOrder != null && dbOrder.IsActive == true)
            {
                if (dbOrder.Status >= 2)
                {
                    throw new UserException("Hóa đơn đã được duyệt. Không thể cập nhật giá");
                }
                else if (dbOrder.Status == 0)
                {
                    throw new UserException("Hóa đơn đã bị hủy. Không thể cập nhật giá");
                }
                else
                {
                    dbOrder.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                    dbOrder.TotalPrice = price;
                    if (orderRepository.Update(dbOrder.Id, dbOrder))
                    {
                        return await CheckOrderPaid(dbOrder.Id);
                    }
                    else
                    {
                        throw new SystemsException("Lỗi trong quá trình cập nhật hóa đơn", nameof(OrderService));
                    }
                }
            }
            else
            {
                throw new UserException("Không tìm thấy hóa đơn");
            }
        }

        public async Task<bool> DeleteOrder(string id)
        {
            var dbOrder = orderRepository.Get(id);
            if (dbOrder != null && dbOrder.IsActive == true)
            {
                if (dbOrder.Status >= 4)
                {
                    throw new UserException("Không thể hủy hóa đơn: Hóa đơn đang trong giai đoạn thực hiện sản phẩm.");
                }
                else
                {
                    dbOrder.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                    dbOrder.Status = 0;
                    dbOrder.IsActive = false;

                    if (orderRepository.Update(dbOrder.Id, dbOrder))
                    {
                        var orderProducts = productRepository.GetAll(x => x.OrderId == dbOrder.Id && x.IsActive == true);
                        if (orderProducts != null && orderProducts.Any())
                        {
                            orderProducts = orderProducts.ToList();

                            var tasks = new List<Task>();

                            foreach (var product in orderProducts)
                            {
                                tasks.Add(Task.Run(() =>
                                {
                                    product.IsActive = false;
                                    product.InactiveTime = DateTime.UtcNow.AddHours(7);
                                    product.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                                    product.Status = 0;
                                }));
                            }

                            var orderProductsStages = productStageRepository.GetAll(x => orderProducts.Select(x => x.Id).Contains(x.ProductId) && x.IsActive == true);
                            if (orderProductsStages != null && orderProductsStages.Any())
                            {
                                orderProductsStages = orderProductsStages.ToList();

                                foreach (var productStage in orderProductsStages)
                                {
                                    tasks.Add(Task.Run(() =>
                                    {
                                        productStage.IsActive = false;
                                        productStage.InactiveTime = DateTime.UtcNow.AddHours(7);
                                        productStage.Status = 0;
                                    }));
                                }
                            }

                            await Task.WhenAll(tasks);

                            var checks = new List<bool>();

                            foreach (var product in orderProducts)
                            {
                                checks.Add(productRepository.Update(product.Id, product));
                            }
                            if (orderProductsStages != null && orderProductsStages.Any())
                            {
                                foreach (var productStage in orderProductsStages)
                                {
                                    checks.Add(productStageRepository.Update(productStage.Id, productStage));
                                }
                            }

                            return !checks.Any(x => x == false);
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        throw new SystemsException("Lỗi trong quá trình hủy hóa đơn", nameof(OrderService));
                    }
                }
            }
            else
            {
                throw new UserException("Không tìm thấy hóa đơn");
            }
        }

        public async Task<Order> GetOrder(string id)
        {
            var order = orderRepository.Get(id);
            if (order != null && order.Status >= 1)
            {
                var orderMaterials = orderMaterialRepository.GetAll(x => x.OrderId == order.Id && x.IsActive == true);
                if (orderMaterials != null && orderMaterials.Any())
                {
                    order.OrderMaterials = orderMaterials.ToList();
                    var materials = materialRepository.GetAll(x => order.OrderMaterials.Select(c => c.MaterialId).Contains(x.Id));
                    if (materials != null && materials.Any())
                    {
                        materials = materials.ToList();
                        var tasks = new List<Task>();
                        foreach (var orderMaterial in order.OrderMaterials)
                        {
                            tasks.Add(Task.Run(async () =>
                            {
                                orderMaterial.Material = materials.FirstOrDefault(x => x.Id == orderMaterial.MaterialId);
                                if (orderMaterial.Material != null)
                                {
                                    orderMaterial.Material.Image = await Ultils.GetUrlImage(orderMaterial.Material.Image);
                                }
                            }));
                        }
                        await Task.WhenAll(tasks);
                    }
                }
                else
                {
                    order.OrderMaterials = new List<OrderMaterial>();
                }

                return order;
            }
            return null;
        }

        public IEnumerable<Order> GetOrdersByCustomer(string cusId)
        {
            return orderRepository.GetAll(x => x.CustomerId == cusId && x.Status >= 1 && x.IsActive == true);
        }
        public async Task<Order> GetOrderByCustomer(string cusId, string orderId)
        {
            var order = orderRepository.Get(orderId);
            if (order != null && order.IsActive == true && order.Status >= 1 && order.CustomerId == cusId)
            {
                var orderMaterials = orderMaterialRepository.GetAll(x => x.OrderId == order.Id && x.IsActive == true);
                if (orderMaterials != null && orderMaterials.Any())
                {
                    order.OrderMaterials = orderMaterials.ToList();
                    var materials = materialRepository.GetAll(x => order.OrderMaterials.Select(c => c.MaterialId).Contains(x.Id));
                    if (materials != null && materials.Any())
                    {
                        materials = materials.ToList();
                        var tasks = new List<Task>();
                        foreach (var orderMaterial in order.OrderMaterials)
                        {
                            tasks.Add(Task.Run(async () =>
                            {
                                orderMaterial.Material = materials.FirstOrDefault(x => x.Id == orderMaterial.MaterialId);
                                if (orderMaterial.Material != null)
                                {
                                    orderMaterial.Material.Image = await Ultils.GetUrlImage(orderMaterial.Material.Image);
                                }
                            }));
                        }
                        await Task.WhenAll(tasks);
                    }
                }
                else
                {
                    order.OrderMaterials = new List<OrderMaterial>();
                }

                return order;
            }
            return null;
        }
        public IEnumerable<Order> GetOrders()
        {
            var orders = orderRepository.GetAll(x => x.Status >= 1 && x.IsActive == true);
            //var orders = orderRepository.GetStoreProcedure("GetActiveOrders");
            if (orders != null && orders.Any())
            {
                return orders.OrderByDescending(x => x.CreatedTime);
            }
            return new List<Order>();
        }
        public async Task<bool> UpdateOrderMaterial(List<OrderMaterial> orderMaterials)
        {
            var dbOrder = orderRepository.Get(orderMaterials.FirstOrDefault().OrderId);

            if (dbOrder != null && dbOrder.Status > 0)
            {
                switch (dbOrder.Status)
                {
                    case 0:
                        throw new UserException("Hóa đơn đã bị hủy. Không thể cập nhật nguyên liệu");
                    case 3:
                        throw new UserException("Hóa đơn đang trong quá trình chờ thực hiện. Không thể cập nhật nguyên liệu");
                    case 4:
                        throw new UserException("Hóa đơn đang trong quá trình thực hiện. Không thể cập nhật nguyên liệu");
                    case 5:
                        throw new UserException("Hóa đơn đã hoàn thiện. Không thể cập nhật nguyên liệu");
                    case 6:
                        throw new UserException("Hóa đơn trong quá trình chờ kiểm thử. Không thể cập nhật nguyên liệu");
                    case 7:
                        throw new UserException("Hóa đơn đã hoàn thành. Không thể cập nhật nguyên liệu");
                }

                var dbOrderMaterials = orderMaterialRepository.GetAll(x => x.OrderId == dbOrder.Id && x.IsActive == true);
                if (dbOrderMaterials != null && dbOrderMaterials.Any())
                {
                    dbOrderMaterials = dbOrderMaterials.ToList();

                    if (orderMaterials == null || dbOrderMaterials.Count() != orderMaterials.Count)
                    {
                        throw new UserException("Số lượng nguyên liệu không đúng");
                    }
                    else
                    {
                        var tasks = new List<Task>();
                        var updateOrderMaterials = new List<OrderMaterial>();
                        foreach (var dbOrderMaterial in dbOrderMaterials)
                        {
                            tasks.Add(Task.Run(async () =>
                            {
                                var orderMaterial = orderMaterials.SingleOrDefault(x => x.Id == dbOrderMaterial.Id);
                                if (orderMaterial != null)
                                {
                                    if (orderMaterial.IsCusMaterial.HasValue && orderMaterial.IsCusMaterial.Value)
                                    {
                                        var insideTasks = new List<Task>();

                                        insideTasks.Add(Task.Run(() =>
                                        {
                                            if (dbOrderMaterial.IsCusMaterial != orderMaterial.IsCusMaterial)
                                            {
                                                dbOrderMaterial.IsCusMaterial = orderMaterial.IsCusMaterial;
                                            }
                                        }));

                                        insideTasks.Add(Task.Run(() =>
                                        {
                                            if (string.IsNullOrEmpty(dbOrderMaterial.Image) && string.IsNullOrEmpty(orderMaterial.Image))
                                            {
                                                throw new UserException("Vui lòng chụp ảnh nguyên liệu của khách để xác nhận");
                                            }
                                            else if (string.IsNullOrEmpty(dbOrderMaterial.Image) && !string.IsNullOrEmpty(orderMaterial.Image))
                                            {
                                                dbOrderMaterial.Image = orderMaterial.Image;
                                            }
                                            else
                                            {
                                                Ultils.DeleteObject(dbOrderMaterial.Image);
                                                dbOrderMaterial.Image = orderMaterial.Image;
                                            }
                                        }));

                                        insideTasks.Add(Task.Run(() =>
                                        {
                                            if (!orderMaterial.Value.HasValue || orderMaterial.Value <= 0)
                                            {
                                                throw new UserException("Vui lòng nhập số lượng nguyên liệu nhận");
                                            }
                                            else
                                            {
                                                dbOrderMaterial.Value = orderMaterial.Value;
                                            }
                                            dbOrderMaterial.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                                        }));

                                        await Task.WhenAll(insideTasks);

                                        updateOrderMaterials.Add(dbOrderMaterial);
                                    }
                                }
                            }));
                        }

                        await Task.WhenAll(tasks);

                        return orderMaterialRepository.UpdateRange(updateOrderMaterials);
                    }
                }
                else
                {
                    throw new UserException("Không tìm thấy nguyên liệu hóa đơn");
                }
            }
            else
            {
                throw new UserException("Không tìm thấy hóa đơn");
            }

            throw new SystemsException("Lỗi trong quá trình cập nhật nguyên liệu hóa đơn", nameof(OrderService));
        }
    }
}
