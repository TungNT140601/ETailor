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
        Customer FindPhone(string phone);
        Customer FindUsername(string username);

        Customer FindById(string id);

        bool CreateCustomer(Customer customer);

        bool UpdatePersonalProfileCustomer(Customer customer);
        bool UpdateCustomerInfo(Customer customer);
        bool UpdateCustomerEmail(Customer customer);
        bool UpdateCustomerPhone(Customer customer);
        bool CheckOTP(string emailOrPhone, string otp);
        void Logout(string id);
        bool CheckSecerctKey(string id, string key);
        bool ChangePassword(string id, string oldPass, string newPass);
        bool ResetPassword(string email);
        bool CusRegis(Customer customer);
    }
}
