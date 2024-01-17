using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.Repository;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CustomException;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class StaffService : IStaffService
    {
        private readonly IStaffRepository staffRepository;
        private IConfiguration configuration;

        public StaffService(IStaffRepository staffRepository, IConfiguration configuration)
        {
            this.staffRepository = staffRepository;
            this.configuration = configuration;
        }

        public Staff CheckLogin(string username, string password)
        {
            try
            {
                var staff = staffRepository.GetAll(x => x.Username == username && Ultils.VerifyPassword(password, x.Password) && x.IsActive == true).FirstOrDefault();
                if (staff == null)
                {
                    throw new UserException("Mật khẩu của bạn không chính xác");
                }
                else
                {
                    return staff;
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

        public bool AddNewStaff(Staff staff)
        {
            try
            {
                if (staffRepository.GetAll(x => x.Username == staff.Username && x.IsActive == true).Any())
                {
                    throw new UserException("Tài khoản đã được sử dụng");
                }
                staff.Id = Ultils.GenGuidString();
                staff.CreatedTime = DateTime.Now;
                staff.IsActive = true;
                staff.Password = Ultils.HashPassword(staff.Password);

                return staffRepository.Create(staff);
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
