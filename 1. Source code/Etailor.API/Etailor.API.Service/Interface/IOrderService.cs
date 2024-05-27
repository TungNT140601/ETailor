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
        Task<bool> ApplyDiscount(string orderId, string code);
        Task<bool> DeleteOrder(string id);
        Task<Order> GetOrder(string id);
        Task<IEnumerable<Order>> GetOrders();
        Task<Order> GetOrderByCustomer(string cusId, string orderId);
        Task<IEnumerable<Order>> GetOrdersByCustomer(string cusId);
        Task<bool> PayDeposit(string orderId, decimal amount);
        Task<bool> CheckOrderPaid(string id);
        Task<bool> FinishOrder(string orderId, string role);
        Task<bool> ApproveOrder(string id);
        Task<bool> FinishOrderAndNofiToCus(string orderId);
        Task<bool> DoneOrder(string orderId);
        Task<bool> UpdateOrderPrice(string id, int price);
        Task<bool> UpdateOrderMaterial(List<OrderMaterial> orderMaterials);
    }
}
