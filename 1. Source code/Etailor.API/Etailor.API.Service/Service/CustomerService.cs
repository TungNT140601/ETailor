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
                var customer = customerRepository.GetAll(x => x.Email == email && x.EmailVerified == true && Ultils.VerifyPassword(password, x.Password)).FirstOrDefault();
                if (customer == null)
                {
                    throw new UserException("Mật khẩu của bạn không chính xác");
                }
                else
                {
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
                var customer = customerRepository.GetAll(x => x.Username.Trim() == username.Trim() && Ultils.VerifyPassword(password, x.Password)).FirstOrDefault();
                if (customer == null)
                {
                    throw new UserException("Mật khẩu của bạn không chính xác");
                }
                else
                {
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
                return customerRepository.GetAll(x => x.Email == email).FirstOrDefault();
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

                customer.CreatedTime = DateTime.Now;
                customer.LastestUpdatedTime = DateTime.Now;
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

                return customerRepository.Update(customer.Id, customer);
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

                dbCustomer.Otp = customer.Otp;
                dbCustomer.OtpexpireTime = customer.OtpexpireTime;
                dbCustomer.Otpused = customer.Otpused;

                dbCustomer.LastestUpdatedTime = DateTime.Now;

                return customerRepository.Update(customer.Id, customer);
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

                dbCustomer.Otp = customer.Otp;
                dbCustomer.OtpexpireTime = customer.OtpexpireTime;
                dbCustomer.Otpused = customer.Otpused;

                dbCustomer.LastestUpdatedTime = DateTime.Now;

                return customerRepository.Update(customer.Id, customer);
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
                var customer = customerRepository.GetAll(x => (x.Email == emailOrPhone || x.Phone == emailOrPhone) && x.Otp == otp && x.OtpexpireTime > DateTime.Now).FirstOrDefault();
                if (customer == null)
                {
                    throw new UserException("Mã xác thực không đúng hoặc hết hạn!!!");
                }
                else
                {
                    customer.Otpused = true;

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

    }
}
