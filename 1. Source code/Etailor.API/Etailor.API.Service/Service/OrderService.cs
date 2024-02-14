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

        public OrderService(IStaffRepository staffRepository, ICustomerRepository customerRepository, IOrderRepository orderRepository, IDiscountRepository discountRepository, IProductRepository productRepository)
        {
            this.staffRepository = staffRepository;
            this.customerRepository = customerRepository;
            this.orderRepository = orderRepository;
            this.discountRepository = discountRepository;
            this.productRepository = productRepository;
        }

        public async Task<string> CreateOrder(Order order, string? role)
        {
            if (string.IsNullOrWhiteSpace(order.CustomerId))
            {
                throw new UserException("Vui lòng chọn khách hàng");
            }

            var cus = customerRepository.Get(order.CustomerId);

            var checkCus = Task.Run(() =>
            {
                if (cus == null || cus.IsActive == false)
                {
                    throw new UserException("Không tìm thấy khách hàng");
                }
            });

            var setValue = Task.Run(() =>
            {
                order.Id = Ultils.GenGuidString();
                order.CreatedTime = DateTime.Now;
                order.LastestUpdatedTime = DateTime.Now;
                order.InactiveTime = null;
                order.IsActive = true;
            });

            var setValue2 = Task.Run(() =>
            {
                if (role != RoleName.MANAGER)
                {
                    order.Status = 1;
                }
                else
                {
                    order.Status = 2;
                }
            });
            await Task.WhenAll(checkCus, setValue);

            return orderRepository.Create(order) ? order.Id : null;
        }

        public async Task<string> UpdateOrder(Order order, string? role)
        {
            var dbOrder = orderRepository.Get(order.Id);
            if (dbOrder != null && dbOrder.IsActive == true)
            {
                if (dbOrder.Status < 2)
                {
                    if (string.IsNullOrWhiteSpace(order.CustomerId))
                    {
                        throw new UserException("Vui lòng chọn khách hàng");
                    }

                    var cus = customerRepository.Get(order.CustomerId);

                    var discount = discountRepository.GetAll(x => order.DiscountCode != null && x.Code != null && x.Code.Trim().ToLower() == order.DiscountCode.Trim().ToLower()).FirstOrDefault();

                    var usedDiscountCode = orderRepository.GetAll(x => x.Id != dbOrder.Id && x.CustomerId == cus.Id && x.DiscountCode == order.DiscountCode && x.IsActive == true && x.Status != 0);

                    var orderProducts = productRepository.GetAll(x => x.OrderId == dbOrder.Id && x.IsActive == true && x.Status != 0).ToList();

                    var checkCus = Task.Run(() =>
                    {
                        if (cus == null || cus.IsActive == false)
                        {
                            throw new UserException("Không tìm thấy khách hàng");
                        }
                        else
                        {
                            dbOrder.CustomerId = cus.Id;
                        }
                    });

                    var checkDiscount = Task.Run(() =>
                    {
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

                                if (discount.ConditionPriceMin != null && discount.ConditionPriceMin != 0 && discount.ConditionPriceMax == null)
                                {
                                    if (order.TotalPrice >= discount.ConditionPriceMin)
                                    {
                                        if (discount.DiscountPrice != null && discount.DiscountPrice != 0)
                                        {
                                            order.DiscountPrice = discount.DiscountPrice;
                                        }
                                        else if (discount.DiscountPercent != null && discount.DiscountPercent != 0)
                                        {
                                            //order.DiscountPrice = order.DiscountPrice * discount.DiscountPercent;
                                        }
                                    }
                                }
                                else if (discount.ConditionPriceMax != null && discount.ConditionPriceMax != 0 && discount.ConditionPriceMin == null)
                                {
                                    if (order.TotalPrice <= discount.ConditionPriceMax)
                                    {
                                        if (discount.DiscountPrice != null && discount.DiscountPrice != 0)
                                        {
                                            order.DiscountPrice = discount.DiscountPrice;
                                        }
                                        else if (discount.DiscountPercent != null && discount.DiscountPercent != 0)
                                        {
                                            //order.DiscountPrice = order.DiscountPrice * discount.DiscountPercent;
                                        }
                                    }
                                }
                                else if (discount.ConditionPriceMin != null && discount.ConditionPriceMin != 0 && discount.ConditionPriceMax != null && discount.ConditionPriceMax != 0)
                                {
                                    if (order.TotalPrice <= discount.ConditionPriceMax && order.TotalPrice >= discount.ConditionPriceMin)
                                    {
                                        if (discount.DiscountPrice != null && discount.DiscountPrice != 0)
                                        {
                                            order.DiscountPrice = discount.DiscountPrice;
                                        }
                                        else if (discount.DiscountPercent != null && discount.DiscountPercent != 0)
                                        {
                                            //order.DiscountPrice = order.DiscountPrice * discount.DiscountPercent;
                                        }
                                    }
                                }
                            }
                        }
                    });

                    var setValue1 = Task.Run(() =>
                    {
                        //này lấy từ bảng product
                        dbOrder.TotalProduct = order.TotalProduct;
                        dbOrder.TotalPrice = order.TotalPrice;

                        //này lấy từ bảng discount


                        dbOrder.DiscountId = order.DiscountId;
                        dbOrder.DiscountCode = order.DiscountCode;
                        dbOrder.AfterDiscountPrice = order.AfterDiscountPrice;

                        //lấy từ bảng transaction
                        dbOrder.PayDeposit = order.PayDeposit;
                        dbOrder.Deposit = order.Deposit;
                        dbOrder.PaidMoney = order.PaidMoney;
                        dbOrder.UnPaidMoney = order.UnPaidMoney;

                        dbOrder.Status = order.Status;
                        dbOrder.CancelTime = order.CancelTime;

                        dbOrder.LastestUpdatedTime = DateTime.Now;
                        dbOrder.InactiveTime = null;
                        dbOrder.IsActive = true;
                    });

                    await Task.WhenAll(setValue1);

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

        public async Task<bool> ApproveOrder(string id)
        {
            var dbOrder = orderRepository.Get(id);
            if (dbOrder != null && dbOrder.IsActive == true)
            {
                if (dbOrder.Status >= 2)
                {
                    throw new UserException("Hóa đơn đã được duyệt. Không thể duyệt lại hóa đơn");
                }

                return orderRepository.Update(dbOrder.Id, dbOrder);
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
