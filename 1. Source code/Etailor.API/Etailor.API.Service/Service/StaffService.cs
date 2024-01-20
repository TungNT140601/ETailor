using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.Repository;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Ultity.CustomException;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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
                    if (string.IsNullOrEmpty(staff.SecrectKeyLogin))
                    {
                        staff.SecrectKeyLogin = Guid.NewGuid().ToString().Substring(0, 20);
                        staffRepository.Update(staff.Id, staff);
                    }
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
                if (CheckPhoneExist(staff.Phone))
                {
                    throw new UserException("Số điện thoại đã được sử dụng");
                }
                staff.Id = Ultils.GenGuidString();
                staff.Role = 2;
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
        public bool UpdateInfo(Staff staff)
        {
            try
            {
                var dbStaff = GetStaff(staff.Id);
                if (dbStaff != null)
                {
                    if (staffRepository.GetAll(x => x.Id != staff.Id && x.Phone != null && x.Phone == staff.Phone).Any())
                    {
                        throw new UserException("Số điện thoại đã được sử dụng");
                    }
                    else
                    {
                        dbStaff.Phone = staff.Phone;
                    }
                    dbStaff.Avatar = staff.Avatar;
                    dbStaff.Fullname = staff.Fullname;
                    dbStaff.Address = staff.Address;
                    dbStaff.Avatar = staff.Avatar;

                    dbStaff.LastestUpdatedTime = DateTime.Now;

                    return staffRepository.Update(dbStaff.Id, dbStaff);
                }
                else
                {
                    throw new UserException("Staff không tồn tại");
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

        public bool ChangePass(string id, string? oldPassword, string newPassword, string role)
        {
            try
            {
                var dbStaff = GetStaff(id);
                if (dbStaff != null)
                {
                    if (role == RoleName.MANAGER)
                    {
                        dbStaff.Password = Ultils.HashPassword(newPassword);
                    }
                    else if (role == RoleName.STAFF)
                    {
                        if (Ultils.VerifyPassword(oldPassword, dbStaff.Password))
                        {
                            dbStaff.Password = Ultils.HashPassword(newPassword);
                        }
                        else
                        {
                            throw new UserException("Mật khẩu không chính xác");
                        }
                    }
                    dbStaff.LastestUpdatedTime = DateTime.Now;

                    return staffRepository.Update(dbStaff.Id, dbStaff);
                }
                else
                {
                    throw new UserException("Staff không tồn tại");
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

        private bool CheckPhoneExist(string phone)
        {
            try
            {
                if (!Ultils.IsValidVietnamesePhoneNumber(phone))
                {
                    throw new UserException("Số điện thoại không đúng định dạng");
                }
                return staffRepository.GetAll(x => x.Phone == phone && x.IsActive == true).Any();
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
                var staff = staffRepository.Get(id);
                if (staff != null)
                {
                    staff.SecrectKeyLogin = null;
                    staffRepository.Update(staff.Id, staff);
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

        public Staff GetStaff(string id)
        {
            try
            {
                return staffRepository.Get(id);
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

        public bool CheckSecrectKey(string id, string key)
        {
            try
            {
                var staff = staffRepository.Get(id);
                if (staff != null)
                {
                    return staff.SecrectKeyLogin == key;
                }
                return false;
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

        public IEnumerable<Staff> GetAll(string? search)
        {
            try
            {
                if (string.IsNullOrEmpty(search))
                {
                    return staffRepository.GetAll(x => x.IsActive == true && (x.Role == 1 || x.Role == 2));
                }
                else
                {
                    return staffRepository.GetAll(x => ((x.Fullname != null && x.Fullname.Trim().ToLower().Contains(search.Trim().ToLower())) || (x.Phone != null && x.Phone.Trim().ToLower().Contains(search.Trim().ToLower()))) && x.IsActive == true && (x.Role == 1 || x.Role == 2));
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
