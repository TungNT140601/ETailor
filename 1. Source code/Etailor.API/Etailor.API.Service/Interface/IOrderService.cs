using Etailor.API.Repository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IOrderService
    {
        Task<string> CreateOrder(Order order, string? role);
        Task<string> UpdateOrder(Order order, string? role);
        bool DeleteOrder(string id);
        Order GetOrder(string id);
        IEnumerable<Order> GetOrders();
        Order GetOrderByCustomer(string cusId, string orderId);
        IEnumerable<Order> GetOrdersByCustomer(string cusId);
        bool PayDeposit(string orderId, decimal amount);
        bool CheckOrderPaid(string id);
        bool FinishOrder(string orderId, string role);
    }
}
