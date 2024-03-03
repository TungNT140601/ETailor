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

        public OrderService(IStaffRepository staffRepository, ICustomerRepository customerRepository, IOrderRepository orderRepository,
            IDiscountRepository discountRepository, IProductRepository productRepository, IPaymentRepository paymentRepository,
            IProductTemplateRepository productTemplaTeRepository, IProductTemplateService productTemplateService)
        {
            this.staffRepository = staffRepository;
            this.customerRepository = customerRepository;
            this.orderRepository = orderRepository;
            this.discountRepository = discountRepository;
            this.productRepository = productRepository;
            this.paymentRepository = paymentRepository;
            this.productTemplaTeRepository = productTemplaTeRepository;
            this.productTemplateService = productTemplateService;
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
                    order.CreatedTime = DateTime.Now;
                    order.LastestUpdatedTime = DateTime.Now;
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
                        dbOrder.LastestUpdatedTime = DateTime.Now;
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
                    var tasks = new List<Task>();
                    foreach (var product in orderProducts.ToList())
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            product.IsActive = false;
                            product.InactiveTime = DateTime.Now;
                        }));
                    }
                    await Task.WhenAll(tasks);

                    var checks = new List<bool>();
                    foreach (var product in orderProducts.ToList())
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
                else
                {
                    dbOrder.Status = role == RoleName.STAFF ? 1 : role == RoleName.MANAGER ? 2 : 0;

                    dbOrder.IsActive = true;

                    if (orderRepository.Update(dbOrder.Id, dbOrder))
                    {
                        return await CheckOrderPaid(dbOrder.Id);
                    }
                    else
                    {
                        throw new SystemsException("Lỗi trong quá trình cập nhật hóa đơn");
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
                        if (discount == null || discount.IsActive == false || discount.StartDate >= DateTime.Now || discount.EndDate <= DateTime.Now)
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
                    dbOrder.LastestUpdatedTime = DateTime.Now;
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
                    throw new SystemsException("Lỗi trong quá trình cập nhật hóa đơn");
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
                if (dbOrder.Status < 2)
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
                            if (discount == null || discount.IsActive == false || discount.StartDate >= DateTime.Now || discount.EndDate <= DateTime.Now)
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
                        dbOrder.LastestUpdatedTime = DateTime.Now;
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
                        throw new SystemsException("Lỗi trong quá trình cập nhật hóa đơn");
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
                else
                {
                    dbOrder.LastestUpdatedTime = DateTime.Now;
                    dbOrder.Status = 2;
                    if (orderRepository.Update(dbOrder.Id, dbOrder))
                    {
                        return await CheckOrderPaid(dbOrder.Id);
                    }
                    else
                    {
                        throw new SystemsException("Lỗi trong quá trình cập nhật hóa đơn");
                    }
                }
            }
            else
            {
                throw new UserException("Không tìm thấy hóa đơn");
            }
        }

        public bool DeleteOrder(string id)
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
                    dbOrder.LastestUpdatedTime = DateTime.Now;
                    dbOrder.Status = 0;
                    dbOrder.IsActive = false;

                    return orderRepository.Update(dbOrder.Id, dbOrder);
                }
            }
            else
            {
                throw new UserException("Không tìm thấy hóa đơn");
            }
        }

        public Order GetOrder(string id)
        {
            var order = orderRepository.Get(id);
            return order == null ? null : order.Status >= 1 ? order : null;
        }

        public IEnumerable<Order> GetOrdersByCustomer(string cusId)
        {
            return orderRepository.GetAll(x => x.CustomerId == cusId && x.Status >= 1 && x.IsActive == true);
        }
        public Order GetOrderByCustomer(string cusId, string orderId)
        {
            return orderRepository.GetAll(x => x.Id == orderId && x.CustomerId == cusId && x.Status >= 1 && x.IsActive == true).FirstOrDefault();
        }

        public IEnumerable<Order> GetOrders()
        {
            var orders = orderRepository.GetAll(x => x.Status >= 1);
            if (orders != null && orders.Any())
            {
                return orders.OrderByDescending(x => x.CreatedTime);
            }
            return new List<Order>();
        }
    }
}
