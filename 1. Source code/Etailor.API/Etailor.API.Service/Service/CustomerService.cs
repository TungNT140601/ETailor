using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CustomException;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public Customer LoginWithEmail(string email, string password)
        {
            try
            {
                var customer = customerRepository.GetAll(x => (x.Email != null && x.Email == email) && (x.EmailVerified != null && x.EmailVerified == true) && Ultils.VerifyPassword(password, x.Password) == true).FirstOrDefault();
                if (customer == null)
                {
                    throw new UserException("Mật khẩu của bạn không chính xác");
                }
                else
                {
                    if (string.IsNullOrEmpty(customer.SecrectKeyLogin))
                    {
                        customer.SecrectKeyLogin = Ultils.GenerateRandomString(20);
                        customerRepository.Update(customer.Id, customer);
                    }
                    return customer;
                }
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }

        public Customer LoginWithUsername(string username, string password)
        {
            try
            {
                var customer = customerRepository.GetAll(x => (x.Username != null && x.Username.Trim() == username.Trim()) && Ultils.VerifyPassword(password, x.Password) && x.IsActive == true).FirstOrDefault();
                if (customer == null)
                {
                    throw new UserException("Mật khẩu của bạn không chính xác");
                }
                else
                {
                    if (string.IsNullOrEmpty(customer.SecrectKeyLogin))
                    {
                        customer.SecrectKeyLogin = Ultils.GenerateRandomString(20);
                        customerRepository.Update(customer.Id, customer);
                    }
                    return customer;
                }
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }

        public Customer FindEmail(string email)
        {
            try
            {
                var cuss = customerRepository.GetAll(x => (x.Email != null && x.Email == email) && (x.IsActive != null && x.IsActive == true)).FirstOrDefault();
                return cuss;
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }

        public Customer FindPhone(string phone)
        {
            try
            {
                return customerRepository.GetAll(x => x.Phone != null && x.Phone == phone && x.IsActive != null && x.IsActive == true).FirstOrDefault();
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
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
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
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
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }

        public bool CreateCustomer(Customer customer)
        {
            try
            {
                customer.Id = Ultils.GenGuidString();
                customer.Password = Ultils.HashPassword(customer.Password);
                customer.Phone = null;
                customer.PhoneVerified = false;
                customer.IsActive = true;

                customer.LastestUpdatedTime = DateTime.Now;
                customer.CreatedTime = null;

                return customerRepository.Create(customer);
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }

        public bool UpdatePersonalProfileCustomer(Customer customer)
        {
            try
            {
                var dbCustomer = customerRepository.Get(customer.Id);

                dbCustomer.Avatar = customer.Avatar;
                dbCustomer.Fullname = customer.Fullname;
                dbCustomer.Address = customer.Address;
                dbCustomer.Username = customer.Username;

                dbCustomer.LastestUpdatedTime = DateTime.Now;

                return customerRepository.Update(dbCustomer.Id, dbCustomer);
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }

        public bool UpdateCustomerInfo(Customer customer)
        {
            try
            {
                var dbCustomer = customerRepository.Get(customer.Id);

                dbCustomer.Avatar = customer.Avatar;
                dbCustomer.Fullname = customer.Fullname;
                dbCustomer.Address = customer.Address;
                dbCustomer.Username = customer.Username;

                dbCustomer.LastestUpdatedTime = DateTime.Now;

                return customerRepository.Update(dbCustomer.Id, dbCustomer);
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }

        public bool UpdateCustomerEmail(Customer customer)
        {
            try
            {
                var dbCustomer = customerRepository.Get(customer.Id);

                dbCustomer.Email = customer.Email;
                dbCustomer.EmailVerified = customer.EmailVerified;

                dbCustomer.Otpnumber = customer.Otpnumber;
                dbCustomer.OtptimeLimit = customer.OtptimeLimit;
                dbCustomer.Otpused = customer.Otpused;

                dbCustomer.LastestUpdatedTime = DateTime.Now;

                return customerRepository.Update(dbCustomer.Id, dbCustomer);
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }

        public bool UpdateCustomerPhone(Customer customer)
        {
            try
            {
                var dbCustomer = customerRepository.Get(customer.Id);

                dbCustomer.Phone = customer.Phone;
                dbCustomer.PhoneVerified = customer.PhoneVerified;

                dbCustomer.Otpnumber = customer.Otpnumber;
                dbCustomer.OtptimeLimit = customer.OtptimeLimit;
                dbCustomer.Otpused = customer.Otpused;

                dbCustomer.LastestUpdatedTime = DateTime.Now;

                return customerRepository.Update(dbCustomer.Id, dbCustomer);
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }

        public bool CheckOTP(string emailOrPhone, string otp)
        {
            try
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
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }

        public void Logout(string id)
        {
            try
            {
                var customer = customerRepository.Get(id);
                if (customer != null)
                {
                    customer.SecrectKeyLogin = null;
                    customerRepository.Update(customer.Id, customer);
                }
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }

        public bool CheckSecerctKey(string id, string key)
        {
            try
            {
                var customer = customerRepository.Get(id);
                if (customer != null)
                {
                    return customer.SecrectKeyLogin == key;
                }
                else
                {
                    return false;
                }
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }

        public bool ChangePassword(string id, string oldPass, string newPass)
        {
            try
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
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }

        public bool ResetPassword(string email)
        {
            try
            {
                if (!Ultils.IsValidEmail(email))
                {
                    throw new UserException("Email sai định dạng!!!");
                }

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
                        throw new SystemsException("Không thể gửi mail");
                    }
                }
                else
                {
                    throw new UserException("Email không tồn tại trong hệ thống");
                }
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }

        public bool CusRegis(Customer customer)
        {
            try
            {
                if (customer.Email != null)
                {
                    if (!Ultils.IsValidEmail(customer.Email))
                    {
                        throw new UserException("Email sai định dạng!!!");
                    }
                    else
                    {
                        var existCus = FindEmail(customer.Email);
                        if (existCus != null)
                        {
                            if (existCus.EmailVerified == false)
                            {
                                throw new UserException("Email chưa được xác thực!!!");
                            }
                            else
                            {
                                if (existCus.CreatedTime == null)
                                {
                                    existCus.Password = Ultils.HashPassword(customer.Password);
                                    existCus.Avatar = customer.Avatar;
                                    existCus.Address = customer.Address;
                                    if (customerRepository.GetAll(c => c.Id != existCus.Id && c.Username == existCus.Username).Any())
                                    {
                                        throw new UserException("Tên tài khoản đã được sử dụng!!!");
                                    }
                                    else
                                    {
                                        existCus.Username = customer.Username;
                                    }
                                    existCus.IsActive = true;
                                    existCus.CreatedTime = DateTime.Now;
                                    existCus.LastestUpdatedTime = DateTime.Now;

                                    return customerRepository.Update(existCus.Id, existCus);
                                }
                                else
                                {
                                    throw new UserException("Email đã được sử dụng!!!");
                                }
                            }
                        }
                        else
                        {
                            throw new UserException("Email không tồn tại trong hệ thống");
                        }
                    }
                }
                else
                {
                    if (FindUsername(customer.Username) != null)
                    {
                        throw new UserException("Tên tài khoản đã được sử dụng!!!");
                    }
                    else
                    {
                        customer.Id = Ultils.GenGuidString();
                        customer.Password = Ultils.HashPassword(customer.Password);
                        customer.CreatedTime = DateTime.Now;
                        customer.EmailVerified = false;
                        customer.Email = null;
                        customer.Phone = null;
                        customer.PhoneVerified = false;
                        customer.IsActive = true;
                        customer.LastestUpdatedTime = DateTime.Now;

                        return customerRepository.Create(customer);
                    }
                }
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }

        private bool CheckEmailAndPhoneExist(string? id, string email, string phone)
        {
            try
            {
                if (id == null)
                {
                    return customerRepository.GetAll(x => ((x.Email != null && x.Email == email) || (x.Phone != null && x.Phone == phone)) && x.IsActive != null && x.IsActive == true).Any();
                }
                else
                {
                    return customerRepository.GetAll(x => x.Id != id && ((x.Email != null && x.Email == email) || (x.Phone != null && x.Phone == phone)) && x.IsActive != null && x.IsActive == true).Any();
                }
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }
    }
}
