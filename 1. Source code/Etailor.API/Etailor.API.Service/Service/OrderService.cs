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

namespace Etailor.API.Service.Service
{
    public class OrderService : IOrderService
    {
        private readonly IStaffRepository staffRepository;
        private readonly ICustomerRepository customerRepository;
        private readonly IOrderRepository orderRepository;
        private readonly IDiscountRepository discountRepository;

        public OrderService(IStaffRepository staffRepository, ICustomerRepository customerRepository, IOrderRepository orderRepository, IDiscountRepository discountRepository)
        {
            this.staffRepository = staffRepository;
            this.customerRepository = customerRepository;
            this.orderRepository = orderRepository;
            this.discountRepository = discountRepository;
        }

        public async Task<bool> CreateOrder(Order order)
        {
            var discount = discountRepository.GetAll(x => x.Code == order.DiscountCode).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(order.CustomerId))
            {
                throw new UserException("Vui lòng chọn khách hàng");
            }

            var cus = customerRepository.Get(order.CustomerId);

            var checkDuplicateId = Task.Run(() =>
            {
                //if (orderRepository.GetAll(x => x.Id == order.Id && x.IsActive == true).Any())
                //{
                //    throw new UserException("Mã Id Order đã được sử dụng");
                //}
            });

            var checkCus = Task.Run(() =>
            {
                if (cus == null || cus.IsActive == false)
                {
                    throw new UserException("Không tìm thấy khách hàng");
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
                    else
                    {
                        order.DiscountId = discount.Id;
                        order.DiscountCode = discount.Code;
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
                                    order.DiscountPrice = order.DiscountPrice * discount.DiscountPercent;
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
                                    order.DiscountPrice = order.DiscountPrice * discount.DiscountPercent;
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
                                    order.DiscountPrice = order.DiscountPrice * discount.DiscountPercent;
                                }
                            }
                        }
                    }
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

            await Task.WhenAll(checkDuplicateId, checkCus, checkDiscount, setValue);

            return orderRepository.Create(order);
        }

        public async Task<bool> UpdateOrder(Order order)
        {
            var dbOrder = orderRepository.Get(order.Id);
            if (dbOrder != null && dbOrder.IsActive == true)
            {
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

                return orderRepository.Update(dbOrder.Id, dbOrder);
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
                var checkChild = Task.Run(() =>
                {
                    //if (productTemplateRepository.GetAll(x => x.CategoryId == id && x.IsActive == true).Any() || componentTypeRepository.GetAll(x => x.CategoryId == id && x.IsActive == true).Any())
                    //{
                    //    throw new UserException("Không thể xóa danh mục sản phầm này do vẫn còn các mẫu sản phẩm và các loại thành phần sản phẩm vẫn còn thuộc danh mục này");
                    //}
                });
                var setValue = Task.Run(() =>
                {
                    dbOrder.CreatedTime = null;
                    dbOrder.LastestUpdatedTime = DateTime.Now;
                    dbOrder.IsActive = false;
                    dbOrder.InactiveTime = DateTime.Now;
                });

                await Task.WhenAll(checkChild, setValue);

                return orderRepository.Update(dbOrder.Id, dbOrder);
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

        public IEnumerable<Order> GetOrders(string? search)
        {
            return orderRepository.GetAll(x => (search == null || (search != null && x.Id.Trim().ToLower().Contains(search.ToLower().Trim()))) && x.IsActive == true);
        }
    }
}
