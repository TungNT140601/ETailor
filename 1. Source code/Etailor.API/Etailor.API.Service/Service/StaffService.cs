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
        private readonly IProductRepository productRepository;
        private IConfiguration configuration;

        public StaffService(IStaffRepository staffRepository, IConfiguration configuration, ICategoryRepository categoryRepository
            , IMasteryRepository masteryRepository, IProductRepository productRepository)
        {
            this.staffRepository = staffRepository;
            this.configuration = configuration;
            this.categoryRepository = categoryRepository;
            this.masteryRepository = masteryRepository;
            this.productRepository = productRepository;
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
            var masteries = categoryRepository.GetAll(x => masterySkills != null && string.Join(",", masterySkills).Contains(x.Id) && x.IsActive == true);
            if (masteries != null && masteries.Any())
            {
                masteries = masteries.ToList();
            }
            else
            {
                masteries = null;
            }
            var dupplicateUsername = staffRepository.GetAll(x => x.Username == staff.Username && x.IsActive == true);
            if (dupplicateUsername != null && dupplicateUsername.Any())
            {
                throw new UserException("Tài khoản đã được sử dụng");
            }
            else
            {
                dupplicateUsername = null;
            }
            var dupplicatePhone = staffRepository.GetAll(x => !string.IsNullOrWhiteSpace(staff.Phone) && x.Phone == staff.Phone && x.IsActive == true);
            if (dupplicatePhone != null && dupplicatePhone.Any())
            {
                throw new UserException("Số điện thoại đã được sử dụng");
            }
            else
            {
                dupplicatePhone = null;
            }
            var tasks = new List<Task>();
            tasks.Add(Task.Run(async () =>
            {
                if (avatar != null)
                {
                    staff.Avatar = await Ultils.UploadImage(wwwroot, "StaffAvatar", avatar, null);
                }
                else
                {
                    staff.Avatar = string.Empty;
                }
            }));
            tasks.Add(Task.Run(() =>
            {
                staff.Id = Ultils.GenGuidString();
            }));
            tasks.Add(Task.Run(() =>
            {
                staff.Role = 2;
            }));
            tasks.Add(Task.Run(() =>
            {
                staff.CreatedTime = DateTime.UtcNow.AddHours(7);
            }));
            tasks.Add(Task.Run(() =>
            {
                staff.IsActive = true;
            }));
            tasks.Add(Task.Run(() =>
            {
                staff.Password = Ultils.HashPassword(staff.Password);
            }));
            tasks.Add(Task.Run(async () =>
            {
                staff.Masteries = new List<Mastery>();

                var duplicate = new List<string>();

                if (masterySkills != null && masterySkills.Count > 0)
                {
                    if (masteries != null && masteries.Any())
                    {
                        var insideTasks = new List<Task>();
                        foreach (var skill in masterySkills)
                        {
                            insideTasks.Add(Task.Run(() =>
                            {
                                var category = masteries.FirstOrDefault(x => x.Id == skill);
                                if (category != null)
                                {
                                    if (masterySkills.Where(c => c == skill).Count() > 1)
                                    {
                                        duplicate.Add(category.Name);
                                        masterySkills.Remove(skill);
                                    }
                                    else
                                    {
                                        staff.Masteries.Add(new Mastery()
                                        {
                                            Id = Ultils.GenGuidString(),
                                            StaffId = staff.Id,
                                            CategoryId = skill
                                        });
                                    }
                                }
                            }));
                        }
                        await Task.WhenAll(insideTasks);
                    }
                    else
                    {
                        throw new UserException($"Chuyên môn không tồn tại");
                    }

                    if (duplicate.Count > 0)
                    {
                        throw new UserException($"Chuyên môn không được trùng");
                    }
                }
            }));

            await Task.WhenAll(tasks);

            if (staffRepository.Create(staff))
            {
                return true;
            }
            else
            {
                throw new UserException($"Lỗi khi thêm nhân viên");
            }
        }
        public async Task<bool> UpdateInfo(Staff staff, string wwwroot, IFormFile? avatar, List<string>? masterySkills, string role)
        {
            var dbStaff = staffRepository.Get(staff.Id);
            if (dbStaff != null && dbStaff.IsActive == true)
            {
                var masteries = categoryRepository.GetAll(x => masterySkills != null && string.Join(",", masterySkills).Contains(x.Id) && x.IsActive == true);
                if (masteries != null && masteries.Any())
                {
                    masteries = masteries.ToList();
                }
                else
                {
                    masteries = null;
                }

                var dupplicatePhone = staffRepository.GetAll(x => x.Id != dbStaff.Id && !string.IsNullOrWhiteSpace(staff.Phone) && x.Phone == staff.Phone && x.IsActive == true);
                if (dupplicatePhone != null && dupplicatePhone.Any())
                {
                    throw new UserException("Số điện thoại đã được sử dụng");
                }
                else
                {
                    dupplicatePhone = null;
                    dbStaff.Phone = staff.Phone;
                }

                var dbStaffMastery = masteryRepository.GetAll(x => x.StaffId == dbStaff.Id);
                if (dbStaffMastery != null && dbStaffMastery.Any())
                {
                    dbStaffMastery = dbStaffMastery.ToList();
                }

                var addMastery = new List<Mastery>();
                var removeMastery = new List<Mastery>();

                var tasks = new List<Task>();
                tasks.Add(Task.Run(async () =>
                {
                    if (avatar != null)
                    {
                        dbStaff.Avatar = await Ultils.UploadImage(wwwroot, "StaffAvatar", avatar, dbStaff.Avatar);
                    }
                }));

                tasks.Add(Task.Run(() =>
                {
                    dbStaff.Fullname = staff.Fullname;
                }));

                tasks.Add(Task.Run(() =>
                {
                    dbStaff.Address = staff.Address;
                }));

                tasks.Add(Task.Run(() =>
                {
                    dbStaff.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (role == RoleName.MANAGER)
                    {
                        if (dbStaffMastery != null && dbStaffMastery.Any())
                        {
                            foreach (var mastery in dbStaffMastery)
                            {
                                if (masterySkills != null && masterySkills.Any())
                                {
                                    if (!masterySkills.Contains(mastery.CategoryId))
                                    {
                                        removeMastery.Add(mastery);
                                    }
                                }
                                else
                                {
                                    removeMastery.Add(mastery);
                                }
                            }

                            if (masterySkills != null && masterySkills.Any())
                            {
                                foreach (var skill in masterySkills)
                                {
                                    if (!dbStaffMastery.Any(x => x.CategoryId == skill))
                                    {
                                        addMastery.Add(new Mastery()
                                        {
                                            Id = Ultils.GenGuidString(),
                                            StaffId = dbStaff.Id,
                                            CategoryId = skill
                                        });
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (masterySkills != null && masterySkills.Any())
                            {
                                foreach (var skill in masterySkills)
                                {
                                    addMastery.Add(new Mastery()
                                    {
                                        Id = Ultils.GenGuidString(),
                                        StaffId = dbStaff.Id,
                                        CategoryId = skill
                                    });
                                }
                            }
                        }
                    }
                }));

                await Task.WhenAll(tasks);

                if (staffRepository.Update(dbStaff.Id, dbStaff))
                {
                    var check = new List<bool>();
                    if (addMastery != null && addMastery.Any())
                    {
                        foreach (var mastery in addMastery)
                        {
                            check.Add(masteryRepository.Create(mastery));
                        }
                    }
                    if (removeMastery != null && removeMastery.Any())
                    {
                        foreach (var mastery in removeMastery)
                        {
                            check.Add(masteryRepository.Delete(mastery.Id));
                        }
                    }

                    if (check.Any(c => c == false))
                    {
                        throw new UserException("Lỗi khi cập nhật thông tin nhân viên");
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    throw new UserException("Lỗi khi cập nhật thông tin nhân viên");
                }
            }
            else
            {
                throw new UserException("Nhân viên không tồn tại");
            }
        }

        public async Task<bool> ChangePass(string id, string? oldPassword, string newPassword, string role)
        {
            try
            {
                var dbStaff = staffRepository.Get(id);
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
                    dbStaff.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);

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

        public async Task<Staff> GetStaff(string id)
        {
            try
            {
                var staff = staffRepository.Get(id);
                if (staff != null)
                {
                    if (!string.IsNullOrEmpty(staff.Avatar))
                    {
                        staff.Avatar = Ultils.GetUrlImage(staff.Avatar);
                    }
                    staff.Masteries = masteryRepository.GetAll(x => x.StaffId == staff.Id)?.ToList();
                }
                return staff;
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

        public async Task<Staff> GetStaffInfo(string id)
        {
            try
            {
                var staff = staffRepository.Get(id);
                var setAvatar = Task.Run(async () =>
                {
                    if (string.IsNullOrEmpty(staff.Avatar))
                    {
                        staff.Avatar = "https://firebasestorage.googleapis.com/v0/b/etailor-21a50.appspot.com/o/Uploads%2FThumbnail%2Fstill-life-spring-wardrobe-switch.jpg?alt=media&token=7dc9a197-1b76-4525-8dc7-caa2238d8327";
                    }
                    else
                    {
                        staff.Avatar = Ultils.GetUrlImage(staff.Avatar);
                    }
                });
                await Task.WhenAll(setAvatar);
                return staff;
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
        public IEnumerable<Staff> GetAllWithPagination(string? search, int? pageIndex, int? itemPerPage)
        {
            try
            {
                if (string.IsNullOrEmpty(search))
                {
                    var totalData = staffRepository.Count(x => x.IsActive == true && (x.Role == 1 || x.Role == 2));
                    if (totalData == 0)
                    {
                        return new List<Staff>();
                    }
                    else
                    {
                        if (itemPerPage == null)
                        {
                            itemPerPage = 10;
                        }
                        if (pageIndex == null || pageIndex == 0)
                        {
                            pageIndex = 1;
                        }
                        var totalPage = (int)Math.Ceiling((double)totalData / (double)itemPerPage);
                        if (pageIndex > totalPage || pageIndex < 1)
                        {
                            pageIndex = 1;
                        }
                        return staffRepository.GetAllPagination(x => x.IsActive == true && (x.Role == 1 || x.Role == 2), pageIndex, itemPerPage);
                    }
                }
                else
                {
                    var totalData = staffRepository.Count(x => ((x.Fullname != null && x.Fullname.Trim().ToLower().Contains(search.Trim().ToLower())) || (x.Phone != null && x.Phone.Trim().ToLower().Contains(search.Trim().ToLower()))) && x.IsActive == true && (x.Role == 1 || x.Role == 2));
                    if (totalData == 0)
                    {
                        return new List<Staff>();
                    }
                    else
                    {
                        if (itemPerPage == null)
                        {
                            itemPerPage = 10;
                        }
                        if (pageIndex == null || pageIndex == 0)
                        {
                            pageIndex = 1;
                        }
                        var totalPage = (int)Math.Ceiling((double)totalData / (double)itemPerPage);
                        if (pageIndex > totalPage || pageIndex < 1)
                        {
                            pageIndex = 1;
                        }
                        return staffRepository.GetAllPagination(x => ((x.Fullname != null && x.Fullname.Trim().ToLower().Contains(search.Trim().ToLower())) || (x.Phone != null && x.Phone.Trim().ToLower().Contains(search.Trim().ToLower()))) && x.IsActive == true && (x.Role == 1 || x.Role == 2), pageIndex, itemPerPage);
                    }
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

        public bool RemoveStaff(string staffId)
        {
            var staff = staffRepository.Get(staffId);
            if (staff != null && staff.IsActive == true)
            {
                var staffTasks = productRepository.GetAll(x => x.StaffMakerId == staffId && x.Status > 0 && x.Status < 4 && x.IsActive == true);
                if (staffTasks != null && staffTasks.Any())
                {
                    throw new UserException("Nhân viên đang có công việc chưa hoàn thành. Vui lòng bàn giao hết các công việc của nhân viên sang nhân viên khác.");
                }
                else
                {
                    staff.InactiveTime = DateTime.UtcNow.AddHours(7);
                    staff.IsActive = false;
                    return staffRepository.Update(staffId, staff);
                }
            }
            else
            {
                throw new UserException("Nhân viên không tồn tại");
            }
        }
    }
}
