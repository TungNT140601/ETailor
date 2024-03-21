using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CustomException;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class AuthService : IAuthService
    {
        private readonly ICustomerRepository customerRepository;
        private readonly IStaffRepository staffRepository;

        public AuthService(IStaffRepository staffRepository, ICustomerRepository customerRepository)
        {
            this.customerRepository = customerRepository;
            this.staffRepository = staffRepository;
        }

        public async Task<bool> Regis(string? username, string? email, string? fullname, string? phone, string? address, string password, IFormFile? avatarFile)
        {
            var dupplicateStaffs = staffRepository.GetAll(x => (!string.IsNullOrWhiteSpace(username) && x.Username == username) && x.IsActive == true);
            var dupplicateCustomers = customerRepository.GetAll(x => (!string.IsNullOrWhiteSpace(username) && x.Username == username) || (!string.IsNullOrWhiteSpace(email) && x.Email == email) && x.IsActive == true);

            var tasks = new List<Task>();

            var customer = new Customer();

            tasks.Add(Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(email))
                {
                    throw new UserException("Vui lòng nhập tên tài khoản hoặc email");
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(username))
                    {
                        customer.Username = username.Trim();
                    }
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        customer.Email = email.Trim();
                        customer.EmailVerified = true;
                    }
                }
            }));
            tasks.Add(Task.Run(() =>
            {
                if (!string.IsNullOrWhiteSpace(email) && Ultils.IsValidEmail(email))
                {
                    throw new UserException("Email không đúng định dạng");
                }
            }));
            tasks.Add(Task.Run(() =>
            {
                if ((dupplicateCustomers != null && dupplicateCustomers.Any()) || (dupplicateStaffs != null && dupplicateStaffs.Any()))
                {
                    throw new UserException("Tên tài khoản hoặc email đã được sử dụng");
                }
            }));
            tasks.Add(Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(fullname))
                {
                    throw new UserException("Vui lòng nhập họ và tên");
                }
                else
                {
                    customer.Fullname = fullname.Trim();
                }
            }));
            tasks.Add(Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(phone))
                {
                    throw new UserException("Vui lòng nhập số điện thoại");
                }
                else if (Ultils.IsValidVietnamesePhoneNumber(phone))
                {
                    throw new UserException("Số điện thoại không đúng định dạng");
                }
                else
                {
                    customer.Phone = phone.Trim();
                    customer.PhoneVerified = true;
                }
            }));
            tasks.Add(Task.Run(() =>
            {
                if (!string.IsNullOrWhiteSpace(address))
                {
                    customer.Address = address;
                }
            }));
            tasks.Add(Task.Run(() =>
            {
                if (string.IsNullOrEmpty(password))
                {
                    throw new UserException("Vui lòng nhập mật khẩu");
                }
                else
                {
                    customer.Password = Ultils.HashPassword(password);
                }
            }));
            tasks.Add(Task.Run(async () =>
            {
                if (avatarFile != null)
                {
                    customer.Avatar = await Ultils.UploadImage("./wwwroot", "CustomerAvatar", avatarFile, null);
                }
            }));
            tasks.Add(Task.Run(() =>
            {
                customer.Id = Ultils.GenGuidString();
                customer.CreatedTime = DateTime.UtcNow.AddHours(7);
                customer.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                customer.InactiveTime = null;
                customer.IsActive = true;
            }));

            await Task.WhenAll(tasks);

            return customerRepository.Create(customer);
        }

        public Customer CheckLoginCus(string? usernameOrEmail, string? password)
        {
            if (usernameOrEmail.Contains("@"))
            {
                var customer = customerRepository.FirstOrDefault(x => x.Email == usernameOrEmail && x.IsActive == true);

                if (customer != null)
                {
                    if (!Ultils.VerifyPassword(password, customer.Password))
                    {
                        return null;
                    }
                    else if (string.IsNullOrEmpty(customer.SecrectKeyLogin))
                    {
                        customer.SecrectKeyLogin = Guid.NewGuid().ToString().Substring(0, 20);
                        customerRepository.Update(customer.Id, customer);
                    }
                }

                return customer;
            }
            else
            {
                var customer = customerRepository.FirstOrDefault(x => x.Username == usernameOrEmail && x.IsActive == true);

                if (customer != null)
                {
                    if (!Ultils.VerifyPassword(password, customer.Password))
                    {
                        return null;
                    }
                    else if (string.IsNullOrEmpty(customer.SecrectKeyLogin))
                    {
                        customer.SecrectKeyLogin = Guid.NewGuid().ToString().Substring(0, 20);
                        customerRepository.Update(customer.Id, customer);
                    }
                }

                return customer;
            }
        }

        public Staff CheckLoginStaff(string? usernameOrEmail, string? password)
        {
            var staff = staffRepository.FirstOrDefault(x => x.Username == usernameOrEmail && x.IsActive == true);

            if (staff != null)
            {
                if (!Ultils.VerifyPassword(password, staff.Password))
                {
                    return null;
                }
                else if (string.IsNullOrEmpty(staff.SecrectKeyLogin))
                {
                    staff.SecrectKeyLogin = Guid.NewGuid().ToString().Substring(0, 20);
                    staffRepository.Update(staff.Id, staff);
                }
            }

            return staff;
        }
    }
}
