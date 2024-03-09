using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.Repository;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Ultity.CustomException;
using Microsoft.AspNetCore.Http;
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
        private readonly ICategoryRepository categoryRepository;
        private readonly IMasteryRepository masteryRepository;
        private IConfiguration configuration;

        public StaffService(IStaffRepository staffRepository, IConfiguration configuration, ICategoryRepository categoryRepository, IMasteryRepository masteryRepository)
        {
            this.staffRepository = staffRepository;
            this.configuration = configuration;
            this.categoryRepository = categoryRepository;
            this.masteryRepository = masteryRepository;
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
                throw new SystemsException(ex.Message, nameof(StaffService));
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(StaffService));
            }
        }

        public async Task<bool> AddNewStaff(Staff staff, string wwwroot, IFormFile? avatar, List<string>? masterySkills)
        {
            if (staffRepository.GetAll(x => x.Username == staff.Username && x.IsActive == true).Any())
            {
                throw new UserException("Tài khoản đã được sử dụng");
            }
            if (staff.Phone != null && CheckPhoneExist(staff.Phone))
            {
                throw new UserException("Số điện thoại đã được sử dụng");
            }
            var setAvatar = Task.Run(async () =>
            {
                if (avatar != null)
                {
                    staff.Avatar = await Ultils.UploadImage(wwwroot, "StaffAvatar", avatar, null);
                }
                else
                {
                    staff.Avatar = string.Empty;
                }
            });
            var setId = Task.Run(() =>
            {
                staff.Id = Ultils.GenGuidString();
            });
            var setRole = Task.Run(() =>
            {
                staff.Role = 2;
            });
            var setCreateTime = Task.Run(() =>
            {
                staff.CreatedTime = DateTime.Now;
            });
            var setIsActive = Task.Run(() =>
            {
                staff.IsActive = true;
            });
            var hashPassword = Task.Run(() =>
            {
                staff.Password = Ultils.HashPassword(staff.Password);
            });

            await Task.WhenAll(setAvatar, setId, setRole, setCreateTime, setIsActive, hashPassword);

            if (staffRepository.Create(staff))
            {
                var addMastery = new List<Task<bool>>();

                var duplicate = new List<string>();
                var notExist = new List<string>();
                if (masterySkills != null && masterySkills.Count > 0)
                {
                    foreach (var skill in masterySkills)
                    {
                        var category = categoryRepository.Get(skill);
                        if (category != null && category.IsActive == true)
                        {
                            if (masterySkills.Where(c => c == skill).Count() > 1)
                            {
                                duplicate.Add(category.Name);
                                masterySkills.Remove(skill);
                            }
                            else
                            {
                                addMastery.Add(Task.Run(() =>
                                {
                                    return masteryRepository.Create(new Mastery()
                                    {
                                        Id = Ultils.GenGuidString(),
                                        CategoryId = skill,
                                        StaffId = staff.Id
                                    });
                                }));
                            }
                        }
                        else
                        {
                            notExist.Add(skill);
                        }
                    }
                    var setMasterySkill = await Task.WhenAll(addMastery);
                    if (duplicate.Count > 0)
                    {
                        throw new UserException($"Chuyên môn không được trùng: {string.Join(", ", duplicate)}");
                    }
                    if (notExist.Count > 0)
                    {
                        throw new UserException($"Chuyên môn không tồn tại: {string.Join(", ", duplicate)}");
                    }
                    if (setMasterySkill.Any(c => c == false))
                    {
                        throw new UserException($"Lỗi khi thêm kỹ năng chuyên môn");
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                throw new UserException($"Lỗi khi thêm nhân viên");
            }
        }
        public async Task<bool> UpdateInfo(Staff staff, string wwwroot, IFormFile? avatar, List<string>? masterySkills, string role)
        {
            var bkStaff = new Staff();
            var bkMastery = new List<string>();
            var dbStaff = GetStaff(staff.Id);
            if (dbStaff != null)
            {
                bkMastery = masteryRepository.GetAll(x => x.StaffId == dbStaff.Id)?.Select(c => c.CategoryId).ToList();
                var phoneDuplicate = staffRepository.GetAll(x => x.Id != staff.Id && x.Phone != null && x.Phone == staff.Phone);
                var setBkStaff = Task.Run(() =>
                {
                    bkStaff = dbStaff;
                });
                var checkPhoneTask = Task.Run(() =>
                {
                    if (phoneDuplicate != null && phoneDuplicate.Any())
                    {
                        throw new UserException("Số điện thoại đã được sử dụng");
                    }
                    else
                    {
                        dbStaff.Phone = staff.Phone;
                    }
                });
                var checkAvatarTask = Task.Run(() =>
                {
                    if (avatar != null)
                    {
                        dbStaff.Avatar = Ultils.UploadImage(wwwroot, "StaffAvatar", avatar, dbStaff.Avatar).Result;
                    }
                });

                var updateFullnameTask = Task.Run(() =>
                {
                    dbStaff.Fullname = staff.Fullname;
                });

                var updateAddressTask = Task.Run(() =>
                {
                    dbStaff.Address = staff.Address;
                });

                var updateUpdateTimeTask = Task.Run(() =>
                {
                    dbStaff.LastestUpdatedTime = DateTime.Now;
                });

                await Task.WhenAll(setBkStaff, checkPhoneTask, checkAvatarTask, updateFullnameTask, updateAddressTask, updateUpdateTimeTask);

                if (!staffRepository.Update(dbStaff.Id, dbStaff))
                {
                    staffRepository.Update(bkStaff.Id, bkStaff);
                    return false;
                }
                else
                {
                    if (role == RoleName.MANAGER)
                    {
                        return await UpdateMastery(dbStaff.Id, masterySkills, bkMastery);
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else
            {
                throw new UserException("Nhân viên không tồn tại");
            }
        }

        private async Task<bool> UpdateMastery(string staffId, List<string>? masterySkills, List<string>? bkMastery)
        {
            var check = new List<bool>();
            var currentMastery = masteryRepository.GetAll(x => x.StaffId == staffId)?.Select(c => c.CategoryId).ToList();
            if (currentMastery == null)
            {
                currentMastery = new List<string>();
            }
            if (masterySkills == null)
            {
                masterySkills = new List<string>();
            }
            foreach (var mastery in currentMastery)
            {
                if (!masterySkills.Contains(mastery))
                {
                    check.Add(masteryRepository.Delete(mastery));
                }
            }
            foreach (var mastery in masterySkills)
            {
                if (!currentMastery.Contains(mastery))
                {
                    check.Add(
                    masteryRepository.Create(new Mastery()
                    {
                        Id = Ultils.GenGuidString(),
                        StaffId = staffId,
                        CategoryId = mastery
                    }));
                }
            }

            if (check.Any(c => c == false))
            {
                var masterys = masteryRepository.GetAll(c => c.StaffId == staffId).Select(c => c.CategoryId);

                if (masterys != null && masterys.Count() > 0)
                {
                    foreach (var mastery in masterys)
                    {
                        masteryRepository.Delete(mastery);
                    }
                }


                foreach (var mastery in bkMastery)
                {
                    masteryRepository.Create(new Mastery()
                    {
                        Id = Ultils.GenGuidString(),
                        StaffId = staffId,
                        CategoryId = mastery
                    });
                }

                return false;
            }
            else
            {
                return true;
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
                throw new SystemsException(ex.Message, nameof(StaffService));
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(StaffService));
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
                throw new SystemsException(ex.Message, nameof(StaffService));
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(StaffService));
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
                throw new SystemsException(ex.Message, nameof(StaffService));
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(StaffService));
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
                throw new SystemsException(ex.Message, nameof(StaffService));
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(StaffService));
            }
        }

        public bool CheckSecrectKey(string id, string key)
        {
            try
            {
                var staff = staffRepository.Get(id);
                if (staff != null && staff.IsActive == true)
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
                throw new SystemsException(ex.Message, nameof(StaffService));
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(StaffService));
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
                throw new SystemsException(ex.Message, nameof(StaffService));
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(StaffService));
            }
        }
    }
}
