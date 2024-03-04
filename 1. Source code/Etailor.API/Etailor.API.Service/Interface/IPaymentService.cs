using Etailor.API.Repository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IPaymentService
    {
        Task<string> CreatePayment(string orderId, decimal? amount, int payType, string platform, string ip);
        Task<bool> UpdatePayment(string paymentId, int status);
        IEnumerable<Payment> GetAllPayments();
        IEnumerable<Payment> GetAllOrderPayments(string? orderId);
        Task<bool> RefundMoneyVNPay(string paymentId, int transactionType, decimal? amount);
    }
}
