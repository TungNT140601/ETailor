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

        public OrderService(IStaffRepository staffRepository, ICustomerRepository customerRepository, IOrderRepository orderRepository)
        {
            this.staffRepository = staffRepository;
            this.customerRepository = customerRepository;
            this.orderRepository = orderRepository;
        }

        public async Task<bool> CreateOrder(Order order)
        {
            var checkDuplicateId = Task.Run(() =>
            {
                if (orderRepository.GetAll(x => x.Id == order.Id && x.IsActive == true).Any())
                {
                    throw new UserException("Mã Id Order đã được sử dụng");
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

            await Task.WhenAll(checkDuplicateId, setValue);

            return orderRepository.Create(order);
        }

        public async Task<bool> UpdateOrder(Order order)
        {
            var dbOrder = orderRepository.Get(order.Id);
            if (dbOrder != null && dbOrder.IsActive == true)
            {
                var setValue = Task.Run(() =>
                {
                    //này lấy từ bảng product
                    dbOrder.TotalProduct = order.TotalProduct;
                    dbOrder.TotalPrice = order.TotalPrice;

                    //này lấy từ bảng discount
                    dbOrder.DiscountId = order.DiscountId;
                    dbOrder.DiscountCode = order.DiscountCode;
                    dbOrder.AfterDiscountPrice   = order.AfterDiscountPrice;

                    //lấy từ bảng transaction
                    dbOrder.PayDeposit = order.PayDeposit;
                    dbOrder.Deposit = order.Deposit;
                    dbOrder.PaidMoney = order.PaidMoney;
                    dbOrder.UnPaidMoney = order.UnPaidMoney;

                    dbOrder.Status = order.Status;
                    dbOrder.CancelTime = order.CancelTime;

                    dbOrder.CreatedTime = null;
                    dbOrder.LastestUpdatedTime = DateTime.Now;
                    dbOrder.InactiveTime = null ;
                    dbOrder.IsActive = true;
                });

                await Task.WhenAll(setValue);

                return orderRepository.Update(dbOrder.Id, dbOrder);
            }
            else
            {
                throw new UserException("Không tìm thấy danh mục sản phầm");
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
