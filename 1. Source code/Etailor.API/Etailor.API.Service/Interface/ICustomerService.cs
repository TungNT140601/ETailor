using Etailor.API.Repository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface ICustomerService
    {
        Customer LoginWithEmail(string email, string password);
        Customer LoginWithUsername(string username, string password);
        Customer FindEmail(string email);
        bool CreateCustomer(Customer customer);
        bool UpdateCustomerInfo(Customer customer);
        bool UpdateCustomerEmail(Customer customer);
        bool UpdateCustomerPhone(Customer customer);
    }
}
