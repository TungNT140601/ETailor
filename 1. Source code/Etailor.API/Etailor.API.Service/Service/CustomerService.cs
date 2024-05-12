using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.StoreProcModels;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CustomException;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Etailor.API.Service.Service
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository customerRepository;
        public CustomerService(ICustomerRepository customerRepository)
        {
            this.customerRepository = customerRepository;
        }

        public Customer Login(string emailOrUsername, string password, string ip, string clientToken)
        {
            var customer = customerRepository.FirstOrDefault(x => ((x.Username != null && x.Username.Trim() == emailOrUsername.Trim()) || (x.Email != null && x.Email == emailOrUsername) && (x.EmailVerified != null && x.EmailVerified == true)) && x.IsActive == true);

            if (customer != null && !string.IsNullOrEmpty(customer.Password) && Ultils.VerifyPassword(password, customer.Password))
            {
                if (string.IsNullOrEmpty(customer.SecrectKeyLogin))
                {
                    customer.SecrectKeyLogin = Ultils.GenerateRandomString(20);
                    customerRepository.Update(customer.Id, customer);
                }

                return customer;
            }
            else
            {
                throw new UserException("Thông tin đăng nhập không chính xác");
            }
        }

        public Customer FindEmail(string email)
        {
            try
            {
                return customerRepository.FirstOrDefault(x => (x.Email != null && x.Email == email) && (x.IsActive != null && x.IsActive == true));
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message, nameof(CustomerService));
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(CustomerService));
            }
        }

        public Customer FindPhone(string phone)
        {
            try
            {
                return customerRepository.GetAll(x => x.Phone != null && x.Phone == phone && x.IsActive != null && x.IsActive == true)?.FirstOrDefault();
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message, nameof(CustomerService));
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(CustomerService));
            }
        }

        public IEnumerable<Customer> FindPhoneOrEmail(string? search)
        {
            try
            {
                return customerRepository.GetAll(x => (string.IsNullOrWhiteSpace(search) || (search != null && (x.Email != null && x.Email.Trim().ToLower().Contains(search.Trim().ToLower())) || (x.Phone != null && x.Phone.Trim().ToLower().Contains(search.Trim().ToLower()))) && x.Fullname != null && x.IsActive != null && x.IsActive == true));
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message, nameof(CustomerService));
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(CustomerService));
            }
        }

        public Customer FindUsername(string username)
        {
            try
            {
                return customerRepository.GetAll(x => x.Username != null && x.Username == username && x.IsActive != null && x.IsActive == true).FirstOrDefault();
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message, nameof(CustomerService));
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(CustomerService));
            }
        }

        public Customer FindById(string id)
        {
            try
            {
                return customerRepository.Get(id);
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message, nameof(CustomerService));
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(CustomerService));
            }
        }

        public async Task<bool> CreateCustomer(Customer customer)
        {
            try
            {
                var tasks = new List<Task>();

                tasks.Add(Task.Run(() =>
                {
                    customer.Id = Ultils.GenGuidString();
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (string.IsNullOrWhiteSpace(customer.Email) && string.IsNullOrWhiteSpace(customer.Phone))
                    {
                        throw new UserException("Vui lòng nhập email hoặc số điện thoại!!!");
                    }
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (!string.IsNullOrEmpty(customer.Password))
                    {
                        customer.Password = Ultils.HashPassword(customer.Password);
                    }
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (!string.IsNullOrWhiteSpace(customer.Phone) && !Ultils.IsValidVietnamesePhoneNumber(customer.Phone))
                    {
                        throw new UserException("Số điện thoại không đúng định dạng!!!");
                    }
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (!string.IsNullOrWhiteSpace(customer.Email) && !Ultils.IsValidEmail(customer.Email))
                    {
                        throw new UserException("Email không đúng định dạng!!!");
                    }
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (CheckEmailAndPhoneExist(null, customer.Email, customer.Phone))
                    {
                        throw new UserException("Email hoặc số điện thoại đã được sử dụng!!!");
                    }
                }));

                tasks.Add(Task.Run(() =>
                {
                    customer.PhoneVerified = false;
                    customer.EmailVerified = false;
                    customer.IsActive = true;
                }));

                tasks.Add(Task.Run(() =>
                {
                    customer.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                    customer.CreatedTime = DateTime.UtcNow.AddHours(7);
                }));

                await Task.WhenAll(tasks);

                return customerRepository.Create(customer);
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message, nameof(CustomerService));
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(CustomerService));
            }
        }

        public async Task<bool> UpdatePersonalProfileCustomer(Customer customer, IFormFile? avatar, string wwwroot)
        {
            var dbCustomer = customerRepository.Get(customer.Id);
            if (dbCustomer != null)
            {
                var checkFullname = Task.Run(() =>
                {
                    if (string.IsNullOrWhiteSpace(customer.Fullname))
                    {
                        throw new UserException("Vui lòng nhập tên");
                    }
                    else
                    {
                        dbCustomer.Fullname = customer.Fullname;
                    }
                });

                var checkAddress = Task.Run(() =>
                {
                    if (string.IsNullOrWhiteSpace(customer.Address))
                    {
                        throw new UserException("Vui lòng nhập địa chỉ");
                    }
                    else
                    {
                        dbCustomer.Address = customer.Address;
                    }
                });

                var checkEmail = Task.Run(() =>
                {
                    if (string.IsNullOrWhiteSpace(customer.Email))
                    {
                        throw new UserException("Vui lòng nhập địa chỉ email");
                    }
                    else
                    {
                        dbCustomer.Email = customer.Email;
                    }
                });

                var addAvatar = Task.Run(async () =>
                {
                    if (avatar != null)
                    {
                        dbCustomer.Avatar = await Ultils.UploadImage(wwwroot, "CustomerAvatar", avatar, dbCustomer.Avatar);
                    }
                });

                var setUpdateTime = Task.Run(() =>
                {
                    dbCustomer.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                });

                await Task.WhenAll(checkAddress, checkEmail, checkFullname, setUpdateTime, addAvatar);

                return customerRepository.Update(dbCustomer.Id, dbCustomer);
            }
            else
            {
                throw new UserException("Không tìm thấy người dùng");
            }
        }

        public bool UpdateCustomerEmail(Customer customer)
        {
            var dbCustomer = customerRepository.Get(customer.Id);
            if (dbCustomer != null)
            {
                dbCustomer.Email = customer.Email;
                dbCustomer.EmailVerified = customer.EmailVerified;

                dbCustomer.Otpnumber = customer.Otpnumber;
                dbCustomer.OtptimeLimit = customer.OtptimeLimit;
                dbCustomer.Otpused = customer.Otpused;

                dbCustomer.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);

                return customerRepository.Update(dbCustomer.Id, dbCustomer);
            }
            else
            {
                throw new UserException("Không tìm thấy người dùng");
            }
        }

        public bool UpdateCustomerPhone(Customer customer)
        {
            var dbCustomer = customerRepository.Get(customer.Id);
            if (dbCustomer != null)
            {
                dbCustomer.Phone = customer.Phone;
                dbCustomer.PhoneVerified = customer.PhoneVerified;

                dbCustomer.Otpnumber = customer.Otpnumber;
                dbCustomer.OtptimeLimit = customer.OtptimeLimit;
                dbCustomer.Otpused = customer.Otpused;

                dbCustomer.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);

                return customerRepository.Update(dbCustomer.Id, dbCustomer);
            }
            else
            {
                throw new UserException("Không tìm thấy người dùng");
            }
        }

        public bool CheckOTP(string emailOrPhone, string otp)
        {
            var customer = new Customer();
            if (Ultils.IsValidEmail(emailOrPhone))
            {
                customer = customerRepository.GetAll(x => (x.Email != null && x.Email == emailOrPhone) && x.Otpnumber == otp && x.OtptimeLimit > DateTime.UtcNow && x.IsActive != null && x.IsActive == true).FirstOrDefault();
            }
            else
            {
                customer = customerRepository.GetAll(x => (x.Phone != null && x.Phone == emailOrPhone) && x.Otpnumber == otp && x.OtptimeLimit > DateTime.UtcNow && x.IsActive != null && x.IsActive == true).FirstOrDefault();
            }
            if (customer == null)
            {
                throw new UserException("Mã xác thực không đúng hoặc hết hạn!!!");
            }
            else
            {
                if (customer.Otpused == true && customer.OtptimeLimit?.AddMinutes(-3) < DateTime.UtcNow.AddHours(7))
                {
                    throw new UserException($"Mã xác thực có thể gửi lại sau {customer.OtptimeLimit.Value.AddMinutes(-3).Minute - DateTime.UtcNow.AddHours(7).Minute} phút");
                }
                else if (customer.Otpused == false)
                {
                    customer.Otpused = true;
                    if (Ultils.IsValidEmail(emailOrPhone))
                    {
                        customer.EmailVerified = true;
                    }
                    else
                    {
                        customer.PhoneVerified = true;
                    }
                    return customerRepository.Update(customer.Id, customer);
                }
                else
                {
                    throw new UserException("Mã xác thực đã được sử dụng");
                }
            }
        }

        public void Logout(string id)
        {
            var customer = customerRepository.Get(id);
            if (customer != null)
            {
                customer.SecrectKeyLogin = null;
                customerRepository.Update(customer.Id, customer);
            }
        }

        public bool CheckSecerctKey(string id, string key)
        {
            var customer = customerRepository.Get(id);
            if (customer != null && customer.IsActive == true)
            {
                return customer.SecrectKeyLogin == key;
            }
            else
            {
                return false;
            }
        }

        public bool ChangePassword(string id, string oldPass, string newPass)
        {
            var customer = customerRepository.Get(id);
            if (customer != null)
            {
                if (!Ultils.VerifyPassword(oldPass, customer.Password))
                {
                    throw new UserException("Mật khẩu không chính xác!!!");
                }
                else
                {
                    customer.Password = Ultils.HashPassword(newPass);

                    return customerRepository.Update(id, customer);
                }
            }
            else
            {
                return false;
            }
        }

        public bool ResetPassword(string email)
        {
            if (!Ultils.IsValidEmail(email))
            {
                throw new UserException("Email sai định dạng!!!");
            }
            else
            {
                var customer = FindEmail(email);

                if (customer != null)
                {
                    var newPassword = Ultils.GenerateRandomString(8);

                    customer.Password = Ultils.HashPassword(newPassword);

                    if (customerRepository.Update(customer.Id, customer))
                    {
                        Ultils.SendResetPassMail(email, newPassword);
                        return true;
                    }
                    else
                    {
                        throw new SystemsException("Không thể gửi mail", nameof(CustomerService));
                    }
                }
                else
                {
                    throw new UserException("Email không tồn tại trong hệ thống");
                }
            }
        }

        public async Task<bool> CusRegis(Customer customer, IFormFile? avatar, string wwwroot)
        {
            try
            {
                var tasks = new List<Task>();

                var sqlParams = new List<SqlParameter>();

                tasks.Add(Task.Run(() =>
                {
                    if (string.IsNullOrWhiteSpace(customer.Username))
                    {
                        throw new UserException("Vui lòng nhập Tên tài khoản");
                    }
                    else
                    {
                        sqlParams.Add(new SqlParameter("@Username", customer.Username));
                    }
                }));
                tasks.Add(Task.Run(() =>
                {
                    if (string.IsNullOrWhiteSpace(customer.Email))
                    {
                        throw new UserException("Vui lòng nhập Email");
                    }
                    else if (!Ultils.IsValidEmail(customer.Email))
                    {
                        throw new UserException("Email sai định dạng!!!");
                    }
                    else
                    {
                        sqlParams.Add(new SqlParameter("@Email", customer.Email));
                    }
                }));
                tasks.Add(Task.Run(() =>
                {
                    if (string.IsNullOrWhiteSpace(customer.Fullname))
                    {
                        customer.Fullname = customer.Email?.Split('@')[0];
                    }
                    sqlParams.Add(new SqlParameter("@Fullname", customer.Fullname));
                }));
                tasks.Add(Task.Run(() =>
                {
                    if (!string.IsNullOrWhiteSpace(customer.Phone))
                    {
                        if (!Ultils.IsValidVietnamesePhoneNumber(customer.Phone))
                        {
                            throw new UserException("Số điện thoại không đúng định dạng!!!");
                        }
                        else
                        {
                            sqlParams.Add(new SqlParameter("@Phone", customer.Phone));
                        }
                    }
                    else
                    {
                        sqlParams.Add(new SqlParameter("@Phone", DBNull.Value));
                    }
                }));
                tasks.Add(Task.Run(() =>
                {
                    sqlParams.Add(new SqlParameter("@Address", string.IsNullOrWhiteSpace(customer.Address) ? DBNull.Value : customer.Address));
                }));
                tasks.Add(Task.Run(() =>
                {
                    if (string.IsNullOrEmpty(customer.Password))
                    {
                        throw new UserException("Vui lòng nhập mật khẩu");
                    }
                    else
                    {
                        sqlParams.Add(new SqlParameter("@Password", Ultils.HashPassword(customer.Password)));
                    }
                }));
                tasks.Add(Task.Run(async () =>
                {
                    if (avatar != null)
                    {
                        customer.Avatar = await Ultils.UploadImage(wwwroot, "CustomerAvatar", avatar, null);
                    }
                    else
                    {
                        customer.Avatar = string.Empty;
                    }

                    sqlParams.Add(new SqlParameter("@Avatar", string.IsNullOrWhiteSpace(customer.Avatar) ? DBNull.Value : customer.Avatar));
                }));

                await Task.WhenAll(tasks);

                var result = await customerRepository.GetStoreProcedureReturnInt(StoreProcName.Customer_Regis,
                    sqlParams.Find(x => x.ParameterName == "@Username"),
                    sqlParams.Find(x => x.ParameterName == "@Email"),
                    sqlParams.Find(x => x.ParameterName == "@Fullname"),
                    sqlParams.Find(x => x.ParameterName == "@Phone"),
                    sqlParams.Find(x => x.ParameterName == "@Address"),
                    sqlParams.Find(x => x.ParameterName == "@Password"),
                    sqlParams.Find(x => x.ParameterName == "@Avatar")
                    );

                switch (result)
                {
                    case -1:
                        throw new UserException("Tên tài khoản đã được sử dụng!!!");
                    case -2:
                        throw new UserException("Email đã được sử dụng!!!");
                    case -3:
                        throw new UserException("Số điện thoại đã được sử dụng!!!");
                    case -4:
                        throw new UserException("Email chưa xác thực!!!");
                    default:
                        return true;
                }
            }
            catch (SqlException ex)
            {
                if (!string.IsNullOrEmpty(customer.Avatar))
                {
                    Ultils.DeleteObject(JsonConvert.DeserializeObject<ImageFileDTO>(customer.Avatar).ObjectName);
                }
                throw new SystemsException(ex.Message, nameof(CustomerService.CusRegis));
            }
            catch (UserException ex)
            {
                if (!string.IsNullOrEmpty(customer.Avatar))
                {
                    Ultils.DeleteObject(JsonConvert.DeserializeObject<ImageFileDTO>(customer.Avatar).ObjectName);
                }
                throw new UserException(ex.Message);
            }
        }

        private bool CheckEmailAndPhoneExist(string? id, string? email, string? phone)
        {
            try
            {
                if (id == null)
                {
                    return customerRepository.GetAll(x =>
                    (
                        (x.Email != null && x.Email == email) ||
                        (x.Phone != null && x.Phone == phone)
                    )
                    && x.Password != null && x.IsActive == true
                    ).Any();
                }
                else
                {
                    return customerRepository.GetAll(
                        x => x.Id != id &&
                        (
                            (x.Email != null && x.Email == email) ||
                            (x.Phone != null && x.Phone == phone)
                        )
                        && x.Password != null && x.IsActive == true
                        ).Any();
                }
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message, nameof(CustomerService));
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(CustomerService));
            }
        }
    }
}
