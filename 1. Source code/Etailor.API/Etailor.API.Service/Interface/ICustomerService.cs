using Etailor.API.Repository.EntityModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface ICustomerService
    {
        Customer Login(string emailOrUsername, string password, string ip, string clientToken);
        Customer FindEmail(string email);
        Customer FindPhone(string phone);
        IEnumerable<Customer> FindPhoneOrEmail(string? search);
        Customer FindUsername(string username);
        Customer FindById(string id);
        Task<bool> CreateCustomer(Customer customer);
        Task<bool> UpdatePersonalProfileCustomer(Customer customer, IFormFile? avatar, string wwwroot);
        bool UpdateCustomerEmail(Customer customer);
        bool UpdateCustomerPhone(Customer customer);
        bool CheckOTP(string emailOrPhone, string otp);
        void Logout(string id);
        bool CheckSecerctKey(string id, string key);
        bool ChangePassword(string id, string oldPass, string newPass);
        bool ResetPassword(string email);
        Task<bool> CusRegis(Customer customer, IFormFile? avatar, string wwwroot);
    }
}
