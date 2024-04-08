using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CustomException;
using Microsoft.AspNetCore.Http;
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

        public async Task<Customer> Login(string emailOrUsername, string password, string ip, string clientToken)
        {
            try
            {
                var customer = await Task.Run(() =>
                {
                    if (Ultils.IsValidEmail(emailOrUsername))
                    {
                        return customerRepository.GetAll(x => (x.Email != null && x.Email == emailOrUsername) && (x.EmailVerified != null && x.EmailVerified == true) && Ultils.VerifyPassword(password, x.Password) == true).FirstOrDefault();
                    }
                    else
                    {
                        return customerRepository.GetAll(x => (x.Username != null && x.Username.Trim() == emailOrUsername.Trim()) && Ultils.VerifyPassword(password, x.Password) && x.IsActive == true).FirstOrDefault();
                    }
                });

                if (customer == null)
                {
                    throw new UserException("Mật khẩu của bạn không chính xác");
                }
                else
                {
                    var updateSecretKeyTask = Task.Run(() =>
                    {
                        //Thread1
                        if (string.IsNullOrEmpty(customer.SecrectKeyLogin))
                        {
                            customer.SecrectKeyLogin = Guid.NewGuid().ToString().Substring(0, 20);
                            customerRepository.Update(customer.Id, customer);
                        }
                    });

                    await Task.WhenAll(updateSecretKeyTask);

                    return customer;
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

        public Customer FindEmail(string email)
        {
            try
            {
                var cuss = customerRepository.GetAll(x => (x.Email != null && x.Email == email) && (x.IsActive != null && x.IsActive == true))?.FirstOrDefault();
                return cuss;
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
                return customerRepository.GetAll(x => (string.IsNullOrWhiteSpace(search) || (search != null && (x.Email != null && x.Email.Trim().ToLower().Contains(search.Trim().ToLower())) || (x.Phone != null && x.Phone.Trim().ToLower().Contains(search.Trim().ToLower()))) && x.IsActive != null && x.IsActive == true));
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
                if (customer.Otpused == false && customer.OtptimeLimit?.AddMinutes(-3) < DateTime.UtcNow.AddHours(7))
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
                                var hashPass = Task.Run(() =>
                                {
                                    if (string.IsNullOrEmpty(customer.Password))
                                    {
                                        throw new UserException("Vui lòng nhập mật khẩu");
                                    }
                                    existCus.Password = Ultils.HashPassword(customer.Password);
                                });
                                var uploadAvatar = Task.Run(async () =>
                                {
                                    if (avatar != null)
                                    {
                                        existCus.Avatar = await Ultils.UploadImage(wwwroot, "CustomerAvatar", avatar, null);
                                    }
                                });
                                var setAddress = Task.Run(() =>
                                {
                                    existCus.Address = customer.Address;
                                });
                                var checkUsername = Task.Run(() =>
                                {
                                    if (customer.Username != null)
                                    {
                                        if (customerRepository.GetAll(c => c.Id != existCus.Id && c.Username != null && c.Username == customer.Username).Any())
                                        {
                                            throw new UserException("Tên tài khoản đã được sử dụng!!!");
                                        }
                                        else
                                        {
                                            existCus.Username = customer.Username;
                                        }
                                    }
                                });
                                var setValue = Task.Run(() =>
                                {
                                    existCus.IsActive = true;
                                    existCus.CreatedTime = DateTime.UtcNow.AddHours(7);
                                    existCus.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                                });

                                await Task.WhenAll(hashPass, uploadAvatar, setAddress, checkUsername, setValue);

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
                    var hashPassword = Task.Run(() =>
                    {
                        customer.Password = Ultils.HashPassword(customer.Password);
                    });
                    var setId = Task.Run(() =>
                    {
                        customer.Id = Ultils.GenGuidString();
                    });
                    var setValue = Task.Run(() =>
                    {
                        customer.EmailVerified = false;
                        customer.Email = null;
                        customer.Phone = null;
                        customer.PhoneVerified = false;
                        customer.IsActive = true;
                        customer.CreatedTime = DateTime.UtcNow.AddHours(7);
                        customer.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                    });

                    return customerRepository.Create(customer);
                }
            }
        }

        private bool CheckEmailAndPhoneExist(string? id, string? email, string? phone)
        {
            try
            {
                if (id == null)
                {
                    return customerRepository.GetAll(x => (x.Email == null || (x.Email != null && x.Email == email)) || (x.Phone == null || (x.Phone != null && x.Phone == phone)) && x.IsActive == true).Any();
                }
                else
                {
                    return customerRepository.GetAll(x => x.Id != id && ((x.Email == null || (x.Email != null && x.Email == email)) || (x.Phone == null || (x.Phone != null && x.Phone == phone)) && x.IsActive == true)).Any();
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
