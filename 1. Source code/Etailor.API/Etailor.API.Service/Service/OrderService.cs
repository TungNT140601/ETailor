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

        public OrderService(IStaffRepository staffRepository, ICustomerRepository customerRepository, IOrderRepository orderRepository, IDiscountRepository discountRepository, IProductRepository productRepository, IPaymentRepository paymentRepository)
        {
            this.staffRepository = staffRepository;
            this.customerRepository = customerRepository;
            this.orderRepository = orderRepository;
            this.discountRepository = discountRepository;
            this.productRepository = productRepository;
            this.paymentRepository = paymentRepository;
        }

        public async Task<string> CreateOrder(Order order, string? role)
        {
            var tasks = new List<Task>();

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
                order.IsActive = true;
            }));

            tasks.Add(Task.Run(() =>
            {
                order.Status = 1;
            }));

            await Task.WhenAll(tasks);

            return orderRepository.Create(order) ? order.Id : null;
        }

        public async Task<string> UpdateOrder(Order order, string? role)
        {
            var dbOrder = orderRepository.Get(order.Id);
            if (dbOrder != null && dbOrder.IsActive == true)
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

                    var orderProducts = productRepository.GetAll(x => x.OrderId == dbOrder.Id && x.IsActive == true && x.Status != 0).ToList();

                    var orderPayments = paymentRepository.GetAll(x => x.OrderId == dbOrder.Id && x.Status == 0).ToList();

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
                        if (orderProducts.Any() && orderProducts.Count > 0)
                        {
                            dbOrder.TotalProduct = orderProducts.Count;

                            dbOrder.TotalPrice = orderProducts.Sum(x => x.Price);
                        }

                        if (!string.IsNullOrWhiteSpace(order.DiscountCode))
                        {
                            if (discount == null || discount.IsActive == false || discount.StartDate >= DateTime.Now && discount.EndDate <= DateTime.Now)
                            {
                                throw new UserException("Mã giảm giá không đúng hoặc hết hạn");
                            }
                            else if (usedDiscountCode.Any())
                            {
                                throw new UserException("Mã giảm giá đã được sử dụng");
                            }
                            else
                            {
                                dbOrder.DiscountId = discount.Id;
                                dbOrder.DiscountCode = discount.Code;
                                if (discount.ConditionProductMin != null && discount.ConditionProductMin != 0)
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
                                }
                                else if (discount.ConditionPriceMin != null && discount.ConditionPriceMin != 0 && discount.ConditionPriceMax == null)
                                {
                                    if (order.TotalPrice >= discount.ConditionPriceMin)
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
                                }
                                else if (discount.ConditionPriceMax != null && discount.ConditionPriceMax != 0 && discount.ConditionPriceMin == null)
                                {
                                    if (order.TotalPrice <= discount.ConditionPriceMax)
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
                                }
                                else if (discount.ConditionPriceMin != null && discount.ConditionPriceMin != 0 && discount.ConditionPriceMax != null && discount.ConditionPriceMax != 0)
                                {
                                    if (order.TotalPrice <= discount.ConditionPriceMax && order.TotalPrice >= discount.ConditionPriceMin)
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
                                }
                                else
                                {
                                    dbOrder.DiscountPrice = 0;
                                }
                            }
                        }


                        if (orderPayments.Any() && orderPayments.Count > 0)
                        {
                            dbOrder.PaidMoney = orderPayments.Where(x => x.Amount > 0).Sum(x => x.Amount);
                            if (dbOrder.DiscountPrice > 0)
                            {
                                if (dbOrder.DiscountPrice < dbOrder.TotalPrice)
                                {
                                    dbOrder.AfterDiscountPrice = dbOrder.TotalPrice - dbOrder.DiscountPrice;
                                }
                                else
                                {
                                    dbOrder.AfterDiscountPrice = 0;
                                }
                            }

                            if (dbOrder.AfterDiscountPrice != 0)
                            {
                                dbOrder.UnPaidMoney = dbOrder.AfterDiscountPrice - dbOrder.PaidMoney;
                            }
                            else
                            {
                                dbOrder.UnPaidMoney = dbOrder.TotalPrice - dbOrder.PaidMoney;
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

                    return orderRepository.Update(dbOrder.Id, dbOrder) ? dbOrder.Id : null;
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

        public bool PayDeposit(string orderId, decimal amount)
        {
            var dbOrder = orderRepository.Get(orderId);
            if (dbOrder != null && dbOrder.IsActive == true)
            {
                dbOrder.Deposit = amount;
                dbOrder.PayDeposit = true;

                if (orderRepository.Update(dbOrder.Id, dbOrder))
                {
                    return CheckOrderPaid(orderId);
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

        public bool CheckOrderPaid(string id)
        {
            var dbOrder = orderRepository.Get(id);
            if (dbOrder != null && dbOrder.IsActive == true)
            {
                var discount = string.IsNullOrEmpty(dbOrder.DiscountId) ? null : discountRepository.Get(dbOrder.DiscountId);

                var orderProducts = productRepository.GetAll(x => x.OrderId == dbOrder.Id && x.IsActive == true && x.Status != 0).ToList();

                var orderPayments = paymentRepository.GetAll(x => x.OrderId == dbOrder.Id && x.Status == 0).ToList();

                if (orderProducts.Any() && orderProducts.Count > 0)
                {
                    dbOrder.TotalProduct = orderProducts.Count;

                    dbOrder.TotalPrice = orderProducts.Sum(x => x.Price);
                }
                else
                {
                    dbOrder.TotalProduct = 0;

                    dbOrder.TotalPrice = 0;
                }

                if (discount != null)
                {

                    dbOrder.DiscountId = discount.Id;
                    dbOrder.DiscountCode = discount.Code;
                    if (discount.ConditionProductMin != null && discount.ConditionProductMin != 0)
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
                    }
                    else if (discount.ConditionPriceMin != null && discount.ConditionPriceMin != 0 && discount.ConditionPriceMax == null)
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
                    }
                    else if (discount.ConditionPriceMax != null && discount.ConditionPriceMax != 0 && discount.ConditionPriceMin == null)
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
                    }
                    else if (discount.ConditionPriceMin != null && discount.ConditionPriceMin != 0 && discount.ConditionPriceMax != null && discount.ConditionPriceMax != 0)
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
                    }
                    else
                    {
                        dbOrder.DiscountPrice = 0;
                    }
                }

                if (orderPayments.Any() && orderPayments.Count > 0)
                {
                    dbOrder.PaidMoney = orderPayments.Where(x => x.Amount > 0).Sum(x => x.Amount);
                    if (dbOrder.DiscountPrice.HasValue && dbOrder.DiscountPrice > 0)
                    {
                        if (dbOrder.DiscountPrice < dbOrder.TotalPrice)
                        {
                            dbOrder.AfterDiscountPrice = dbOrder.TotalPrice - dbOrder.DiscountPrice;
                        }
                        else
                        {
                            dbOrder.AfterDiscountPrice = 0;
                        }
                    }
                    else
                    {
                        dbOrder.AfterDiscountPrice = 0;
                        dbOrder.DiscountPrice = 0;
                    }

                    if (dbOrder.AfterDiscountPrice.HasValue && dbOrder.AfterDiscountPrice != 0)
                    {
                        dbOrder.UnPaidMoney = dbOrder.AfterDiscountPrice - dbOrder.PaidMoney;
                    }
                    else
                    {
                        dbOrder.UnPaidMoney = dbOrder.TotalPrice - dbOrder.PaidMoney;
                    }
                }

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
                    return orderRepository.Update(dbOrder.Id, dbOrder);
                }
            }
            else
            {
                throw new UserException("Không tìm thấy danh mục sản phầm");
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

                    return orderRepository.Update(dbOrder.Id, dbOrder);
                }
            }
            else
            {
                throw new UserException("Không tìm thấy danh mục sản phầm");
            }
        }

        public Order GetOrder(string id)
        {
            var order = orderRepository.Get(id);
            return order == null ? null : order.IsActive == true ? order : null;
        }

        public IEnumerable<Order> GetOrders()
        {
            return orderRepository.GetAll(x => x.IsActive == true);
        }

        public IEnumerable<Order> GetOrdersByCustomer(string cusId)
        {
            return orderRepository.GetAll(x => x.CustomerId == cusId && x.Status != 0 && x.IsActive == true);
        }
    }
}
