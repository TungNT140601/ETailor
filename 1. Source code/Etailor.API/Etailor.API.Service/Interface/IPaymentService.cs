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
        string CreatePayment(string orderId, int? percent, int payType, string platform, string ip);
        bool UpdatePayment(string paymentId, int status);
        IEnumerable<Payment> GetAllPayments(string? orderId);
    }
}
