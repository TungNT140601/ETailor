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
using Etailor.API.Repository.StoreProcModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

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
        private readonly INotificationRepository notificationRepository;
        private readonly ISignalRService signalRService;

        public OrderService(IStaffRepository staffRepository, ICustomerRepository customerRepository, IOrderRepository orderRepository,
            IDiscountRepository discountRepository, IProductRepository productRepository, IPaymentRepository paymentRepository,
            IProductTemplateRepository productTemplaTeRepository, IProductTemplateService productTemplateService,
            IProductStageRepository productStageRepository, IOrderMaterialRepository orderMaterialRepository,
            IMaterialRepository materialRepository, INotificationRepository notificationRepository, ISignalRService signalRService)
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
            this.notificationRepository = notificationRepository;
            this.signalRService = signalRService;
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
                    order.Id = Ultils.GenOrderId();
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
                        dbOrder.CusAddress = order.CusAddress;
                        dbOrder.CusEmail = order.CusEmail;
                        dbOrder.CusName = order.CusName;
                        dbOrder.CusPhone = order.CusPhone;
                    }));

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
            if (await CheckOrderPaid(orderId))
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
                            return await CheckOrderPaid(orderId);
                        }
                        else
                        {
                            throw new SystemsException("Lỗi trong quá trình cập nhật hóa đơn", nameof(OrderService.FinishOrder));
                        }
                    }
                }
                else
                {
                    throw new UserException("Hóa đơn không tồn tại");
                }
            }
            else
            {
                throw new SystemsException("Lỗi trong quá trình cập nhật hóa đơn", nameof(OrderService.CheckOrderPaid));
            }
        }

        public async Task<bool> ApplyDiscount(string orderId, string? code)
        {
            var dbOrder = orderRepository.Get(orderId);
            if (dbOrder != null && dbOrder.Status >= 1 && dbOrder.Status <= 2)
            {
                var tasks = new List<Task>();
                if (dbOrder.UnPaidMoney == 0)
                {
                    throw new UserException("Hóa đơn đã thanh toán xong. Không thể áp dụng thêm mã giảm giá");
                }

                if (dbOrder.Deposit > 0)
                {
                    throw new UserException("Hóa đơn đã đặt cọc. Không thể áp dụng thêm mã giảm giá");
                }

                if (!string.IsNullOrEmpty(code))
                {
                    var cus = customerRepository.Get(dbOrder.CustomerId);

                    var discount = discountRepository.Get(code);

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
                }));

                await Task.WhenAll(tasks);

                if (orderRepository.Update(dbOrder.Id, dbOrder))
                {
                    if (await CheckOrderDiscount(dbOrder.Id))
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

        private async Task<bool> CheckOrderDiscount(string id)
        {
            try
            {
                var result = await orderRepository.GetStoreProcedureReturnInt(StoreProcName.Check_Order_Discount,
                                       new SqlParameter
                                       {
                                           DbType = System.Data.DbType.String,
                                           Value = id,
                                           ParameterName = "@OrderId"
                                       });

                return result == 1;
            }
            catch (SqlException ex)
            {
                throw new UserException(ex.Message);
            }
        }

        public async Task<bool> CheckOrderPaid(string id)
        {
            try
            {
                var result = await orderRepository.GetStoreProcedureReturnInt(StoreProcName.Check_Order_Paid,
                                       new SqlParameter
                                       {
                                           DbType = System.Data.DbType.String,
                                           Value = id,
                                           ParameterName = "@OrderId"
                                       });

                return result == 1;
            }
            catch (SqlException ex)
            {
                throw new UserException(ex.Message);
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

        public async Task<bool> FinishOrderAndNofiToCus(string orderId)
        {
            var order = orderRepository.Get(orderId);
            if (order != null && order.IsActive == true)
            {
                switch (order.Status)
                {
                    case 0:
                        throw new UserException("Hóa đơn đã bị hủy. Không thể hoàn thành hóa đơn");
                    case 1:
                        throw new UserException("Hóa đơn chưa được duyệt. Không thể hoàn thành hóa đơn");
                    case 2:
                        throw new UserException("Hóa đơn đã được duyệt. Không thể hoàn thành hóa đơn");
                    case 3:
                        throw new UserException("Hóa đơn đang trong quá trình chờ thực hiện. Không thể hoàn thành hóa đơn");
                    case 4:
                        throw new UserException("Hóa đơn đang trong quá trình thực hiện. Không thể hoàn thành hóa đơn");
                    case 6:
                        throw new UserException("Hóa đơn trong quá trình chờ kiểm thử. Không thể hoàn thành hóa đơn");
                    case 7:
                        throw new UserException("Hóa đơn bị lỗi trong quá trình khách kiểm thử. Không thể hoàn thành hóa đơn");
                    case 8:
                        throw new UserException("Hóa đơn đã hoàn thành. Không thể hoàn thành hóa đơn");
                }

                var orderProducts = productRepository.GetAll(x => x.OrderId == order.Id && x.Status > 0 && x.IsActive == true);

                if (orderProducts != null && !orderProducts.Any(x => x.Status > 0 && x.Status < 5 && x.IsActive == true))
                {
                    order.Status = 6;
                    order.FinishTime = DateTime.UtcNow.AddHours(7);
                    order.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);

                    if (orderRepository.Update(orderId, order))
                    {
                        var notify = new Notification
                        {
                            Id = Ultils.GenGuidString(),
                            Title = "Hóa đơn đã hoàn thiện",
                            Content = $"Hóa đơn {order.Id} của bạn đã hoàn thiện. Vui lòng tới cửa hàng kiểm tra và đánh giá sản phẩm",
                            CustomerId = order.CustomerId,
                            SendTime = DateTime.UtcNow.AddHours(7),
                            ReadTime = null,
                            IsActive = true,
                            IsRead = false
                        };

                        if (notificationRepository.Create(notify))
                        {
                            await signalRService.SendNotificationToUser(notify.CustomerId, notify.Title);

                            return true;
                        }
                        else
                        {
                            throw new SystemsException("Lỗi trong quá trình gửi thông báo hoàn thành hóa đơn", nameof(OrderService.FinishOrderAndNofiToCus));
                        }
                    }
                    else
                    {
                        throw new SystemsException("Lỗi trong quá trình cập nhật hóa đơn", nameof(OrderService.FinishOrderAndNofiToCus));
                    }

                }
                else
                {
                    throw new UserException("Hóa đơn chưa hoàn thành sản phẩm. Không thể hoàn thành hóa đơn");
                }
            }
            else
            {
                throw new UserException("Không tìm thấy hóa đơn");
            }
        }

        public async Task<bool> DoneOrder(string orderId)
        {
            var order = orderRepository.Get(orderId);
            if (order != null && order.IsActive == true)
            {
                switch (order.Status)
                {
                    case 0:
                        throw new UserException("Hóa đơn đã bị hủy. Không thể hoàn thành hóa đơn");
                    case 1:
                        throw new UserException("Hóa đơn chưa được duyệt. Không thể hoàn thành hóa đơn");
                    case 2:
                        throw new UserException("Hóa đơn đã được duyệt. Không thể hoàn thành hóa đơn");
                    case 3:
                        throw new UserException("Hóa đơn đang trong quá trình chờ thực hiện. Không thể hoàn thành hóa đơn");
                    case 4:
                        throw new UserException("Hóa đơn đang trong quá trình thực hiện. Không thể hoàn thành hóa đơn");
                    case 5:
                        throw new UserException("Hóa đơn đã hoàn thiện và chờ kiểm duyệt. Không thể hoàn thành hóa đơn");
                    case 7:
                        throw new UserException("Hóa đơn bị lỗi trong quá trình khách kiểm thử. Không thể hoàn thành hóa đơn");
                    case 8:
                        throw new UserException("Hóa đơn đã hoàn thành. Không thể hoàn thành hóa đơn");
                }

                var orderProducts = productRepository.GetAll(x => x.OrderId == order.Id && x.Status > 0 && x.IsActive == true);

                if (orderProducts != null && !orderProducts.Any(x => x.Status > 0 && x.Status < 5 && x.IsActive == true))
                {
                    if (order.UnPaidMoney > 0)
                    {
                        throw new UserException("Hóa đơn chưa hoàn thành thanh toán. Không thể hoàn thành hóa đơn");
                    }

                    order.Status = 8;
                    order.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);

                    if (orderRepository.Update(orderId, order))
                    {
                        var notify = new Notification
                        {
                            Id = Ultils.GenGuidString(),
                            Title = "Hóa đơn đã hoàn thành",
                            Content = $"Hóa đơn {order.Id} của bạn đã hoàn thành. Xin cảm ơn!",
                            CustomerId = order.CustomerId,
                            SendTime = DateTime.UtcNow.AddHours(7),
                            ReadTime = null,
                            IsActive = true,
                            IsRead = false
                        };

                        if (notificationRepository.Create(notify))
                        {
                            await signalRService.SendNotificationToUser(notify.CustomerId, notify.Title);

                            return true;
                        }
                        else
                        {
                            throw new SystemsException("Lỗi trong quá trình gửi thông báo hoàn thành hóa đơn", nameof(OrderService.DoneOrder));
                        }
                    }
                    else
                    {
                        throw new SystemsException("Lỗi trong quá trình cập nhật hóa đơn", nameof(OrderService.DoneOrder));
                    }

                }
                else
                {
                    throw new UserException("Hóa đơn chưa hoàn thành sản phẩm. Không thể hoàn thành hóa đơn");
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
            if (dbOrder != null && dbOrder.IsActive == false)
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
                    try
                    {
                        var result = await orderRepository.GetStoreProcedureReturnInt(StoreProcName.Cancel_Order,
                            new SqlParameter
                            {
                                DbType = System.Data.DbType.String,
                                Value = id,
                                ParameterName = "@OrderId"
                            });
                        if (result == 1)
                        {
                            return true;
                        }
                        else
                        {
                            throw new SystemsException("Lỗi trong quá trình hủy hóa đơn", nameof(OrderService.DeleteOrder));
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new UserException(ex.Message);
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
            if (order != null && order.Status >= 0)
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
                                    orderMaterial.Material.Image = Ultils.GetUrlImage(orderMaterial.Material.Image);
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

        public async Task<IEnumerable<Order>> GetOrdersByCustomer(string cusId)
        {
            var orders = orderRepository.GetStoreProcedure(StoreProcName.Get_Active_Orders,
                new SqlParameter()
                {
                    SqlDbType = System.Data.SqlDbType.NVarChar,
                    Value = cusId,
                    ParameterName = "@CustomerId"
                });

            if (orders != null && orders.Any())
            {
                orders = orders.OrderByDescending(x => x.CreatedTime).ToList();
                var orderIds = string.Join(",", orders.Select(x => x.Id).ToList());
                var ordersProducts = productRepository.GetStoreProcedure(StoreProcName.Get_Active_Orders_Products, new Microsoft.Data.SqlClient.SqlParameter { DbType = System.Data.DbType.String, Value = orderIds, ParameterName = "@OrderIds" });
                // var ordersProducts = productRepository.GetAll(x => orderIds.Contains(x.OrderId) && x.IsActive == true && x.Status > 0);
                if (ordersProducts != null && ordersProducts.Any())
                {
                    ordersProducts = ordersProducts.ToList();
                    var templateIds = string.Join(",", ordersProducts.Select(x => x.ProductTemplateId).ToList());

                    var productTemplates = productTemplaTeRepository.GetAll(x => ordersProducts.Select(x => x.ProductTemplateId).Contains(x.Id));
                    if (productTemplates != null && productTemplates.Any())
                    {
                        productTemplates = productTemplates.ToList();
                        var tasks = new List<Task>();
                        foreach (var order in orders)
                        {
                            tasks.Add(Task.Run(() =>
                            {
                                order.Products = new List<Product>();
                                var orderProducts = ordersProducts.Where(x => x.OrderId == order.Id);
                                if (orderProducts != null && orderProducts.Any())
                                {
                                    orderProducts = orderProducts.ToList();

                                    var firstProduct = orderProducts.First();
                                    if (firstProduct.ProductTemplate == null)
                                    {
                                        firstProduct.ProductTemplate = productTemplates.FirstOrDefault(x => x.Id == firstProduct.ProductTemplateId);
                                    }

                                    if (firstProduct.ProductTemplate != null && !string.IsNullOrEmpty(firstProduct.ProductTemplate.ThumbnailImage))
                                    {
                                        firstProduct.ProductTemplate.ThumbnailImage = Ultils.GetUrlImage(firstProduct.ProductTemplate.ThumbnailImage);
                                    }

                                    order.Products.Add(firstProduct);
                                }
                            }));
                        }
                        await Task.WhenAll(tasks);

                        return orders;
                    }
                }
                return orders;
            }
            return new List<Order>();
        }

        public async Task<Order> GetOrderByCustomer(string cusId, string orderId)
        {
            var order = orderRepository.Get(orderId);
            if (order != null && order.IsActive == true && order.Status >= 0 && order.CustomerId == cusId)
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
                                    orderMaterial.Material.Image = Ultils.GetUrlImage(orderMaterial.Material.Image);
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

        public async Task<IEnumerable<Order>> GetOrders()
        {
            var orders = orderRepository.GetStoreProcedure(StoreProcName.Get_Active_Orders,
                new SqlParameter()
                {
                    SqlDbType = System.Data.SqlDbType.NVarChar,
                    Value = DBNull.Value,
                    ParameterName = "@CustomerId"
                });
            if (orders != null && orders.Any())
            {
                orders = orders.ToList();
                var orderIds = string.Join(",", orders.Select(x => x.Id).ToList());
                var ordersProducts = productRepository.GetStoreProcedure(StoreProcName.Get_Active_Orders_Products, new Microsoft.Data.SqlClient.SqlParameter { DbType = System.Data.DbType.String, Value = orderIds, ParameterName = "@OrderIds" });
                //var ordersProducts = productRepository.GetAll(x => orderIds.Contains(x.OrderId) && x.IsActive == true && x.Status > 0);
                if (ordersProducts != null && ordersProducts.Any())
                {
                    ordersProducts = ordersProducts.ToList();

                    var productTemplates = productTemplaTeRepository.GetAll(x =>
                        ordersProducts.Select(c => c.ProductTemplateId).Contains(x.Id));
                    if (productTemplates != null && productTemplates.Any())
                    {
                        productTemplates = productTemplates.ToList();
                        var tasks = new List<Task>();
                        foreach (var order in orders)
                        {
                            tasks.Add(Task.Run(() =>
                            {
                                order.Products = new List<Product>();
                                var orderProducts = ordersProducts.Where(x => x.OrderId == order.Id);
                                if (orderProducts != null && orderProducts.Any())
                                {
                                    orderProducts = orderProducts.ToList();

                                    var firstProduct = orderProducts.First();
                                    if (firstProduct.ProductTemplate == null)
                                    {
                                        firstProduct.ProductTemplate = productTemplates.FirstOrDefault(x => x.Id == firstProduct.ProductTemplateId);
                                    }

                                    if (firstProduct.ProductTemplate != null && !string.IsNullOrEmpty(firstProduct.ProductTemplate.ThumbnailImage))
                                    {
                                        firstProduct.ProductTemplate.ThumbnailImage = Ultils.GetUrlImage(firstProduct.ProductTemplate.ThumbnailImage);
                                    }

                                    order.Products.Add(firstProduct);
                                }
                            }));
                        }
                        await Task.WhenAll(tasks);

                        return orders;
                    }
                }
                return orders;
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
