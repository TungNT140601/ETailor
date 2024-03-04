using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.Repository;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity.CustomException;
using Etailor.API.Ultity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Etailor.API.Service.Service
{
    public class ProfileBodyService : IProfileBodyService
    {
        private readonly ICustomerRepository customerRepository;
        private readonly IStaffRepository staffRepository;
        private readonly IProfileBodyRepository profileBodyRepository;
        private readonly IBodySizeRepository bodySizeRepository;
        private readonly IBodyAttributeRepository bodyAttributeRepository;
        private readonly IBodySizeService bodySizeService;
        private readonly IBodyAttributeService bodyAttributeService;
        public ProfileBodyService(ICustomerRepository customerRepository, IStaffRepository staffRepository, IProfileBodyRepository profileBodyRepository,
            IBodySizeRepository bodySizeRepository, IBodyAttributeRepository bodyAttributeRepository,
            IBodySizeService bodySizeService, IBodyAttributeService bodyAttributeService)
        {
            this.customerRepository = customerRepository;
            this.staffRepository = staffRepository;
            this.profileBodyRepository = profileBodyRepository;
            this.bodySizeRepository = bodySizeRepository;
            this.bodyAttributeRepository = bodyAttributeRepository;
            this.bodySizeService = bodySizeService;
            this.bodyAttributeService = bodyAttributeService;
        }

        public async Task<bool> CreateProfileBodyByStaff(string customerId, string staffId, string name, List<(string id, decimal value)> bodySizeId)
        {
            
            string profileBodyId = Ultils.GenGuidString();
            var checkDuplicateId = Task.Run(() =>
            {
                if (profileBodyRepository.GetAll(x => x.Id == profileBodyId && x.IsActive == true).Any())
                {
                    throw new UserException("Mã Id Profile Body đã được sử dụng");
                }
            });

            ProfileBody profileBody = new ProfileBody();

            var setValue = Task.Run(() =>
            {
                profileBody.Id = profileBodyId;
                profileBody.StaffId = staffId; 
                profileBody.CustomerId = customerId;
                profileBody.Name = name;
                profileBody.IsLocked = true;
                profileBody.CreatedTime = DateTime.Now;
                profileBody.LastestUpdatedTime = DateTime.Now;
                profileBody.InactiveTime = null;
                profileBody.IsActive = true;
            });

            await Task.WhenAll(checkDuplicateId, setValue);

            var existBodySizeList = bodySizeRepository.GetAll(x => x.IsActive == true).Select(x => new { x.Id , x.MinValidValue, x.MaxValidValue} ).ToList();
            var existBodyAttributeList = bodyAttributeRepository.GetAll(x => x.ProfileBodyId == profileBody.Id && x.IsActive == true).Select(x => x.BodySizeId).ToList();

            var bodyAttributeList = new List<BodyAttribute>();
            var tasks = new List<Task>();
            foreach (var id in bodySizeId)
            {
                tasks.Add(Task.Run(() =>
                {
                    if (existBodySizeList.Any(x => x.Id == id.id && id.value >= x.MinValidValue  && id.value <= x.MaxValidValue))
                    {
                        if (!existBodyAttributeList.Contains(id.id))
                        {
                            bodyAttributeList.Add(new BodyAttribute()
                            {
                                Id = Ultils.GenGuidString(),
                                BodySizeId = id.id,
                                ProfileBodyId = profileBody.Id,
                                Value = id.value,
                                CreatedTime = DateTime.Now,
                                LastestUpdatedTime = null,
                                InactiveTime = null,
                                IsActive = true
                            });
                        }
                        else
                        {
                            throw new UserException("Số đo đã tồn tại trong hệ thống");
                        }
                    }
                    else
                    {
                        throw new UserException("Số đo không tồn tại trong hệ thống");
                    }
                }
                ));
            }

            await Task.WhenAll(tasks);

            profileBodyRepository.Create(profileBody);
            return bodyAttributeRepository.CreateRange(bodyAttributeList);   
        }

        public async Task<bool> CreateProfileBodyByCustomer(string customerId, string name, List<(string id, decimal value)> bodySizeId)
        {
            string profileBodyId = Ultils.GenGuidString();
            var checkDuplicateId = Task.Run(() =>
            {
                if (profileBodyRepository.GetAll(x => x.Id == profileBodyId && x.IsActive == true).Any())
                {
                    throw new UserException("Mã Id Profile Body đã được sử dụng");
                }
            });

            ProfileBody profileBody = new ProfileBody();

            var setValue = Task.Run(() =>
            {
                profileBody.Id = profileBodyId;
                profileBody.StaffId = null;
                profileBody.CustomerId = customerId;
                profileBody.Name = name;
                profileBody.IsLocked = false;
                profileBody.CreatedTime = DateTime.Now;
                profileBody.LastestUpdatedTime = DateTime.Now;
                profileBody.InactiveTime = null;
                profileBody.IsActive = true;
            });

            await Task.WhenAll(checkDuplicateId, setValue);

            var existBodySizeList = bodySizeRepository.GetAll(x => x.IsActive == true).Select(x => new { x.Id, x.MinValidValue, x.MaxValidValue }).ToList();
            var existBodyAttributeList = bodyAttributeRepository.GetAll(x => x.ProfileBodyId == profileBody.Id && x.IsActive == true).Select(x => x.BodySizeId).ToList();

            var bodyAttributeList = new List<BodyAttribute>();
            var tasks = new List<Task>();
            foreach (var id in bodySizeId)
            {
                tasks.Add(Task.Run(() =>
                {
                    if (existBodySizeList.Any(x => x.Id == id.id && id.value >= x.MinValidValue && id.value <= x.MaxValidValue))
                    {
                        if (!existBodyAttributeList.Contains(id.id))
                        {
                            bodyAttributeList.Add(new BodyAttribute()
                            {
                                Id = Ultils.GenGuidString(),
                                BodySizeId = id.id,
                                ProfileBodyId = profileBody.Id,
                                Value = id.value,
                                CreatedTime = DateTime.Now,
                                LastestUpdatedTime = null,
                                InactiveTime = null,
                                IsActive = true
                            });
                        }
                        else
                        {
                            throw new UserException("Số đo đã tồn tại trong hệ thống");
                        }
                    }
                    else
                    {
                        throw new UserException("Số đo không tồn tại trong hệ thống");
                    }
                }
                ));
            }

            await Task.WhenAll(tasks);

            profileBodyRepository.Create(profileBody);
            return bodyAttributeRepository.CreateRange(bodyAttributeList);
        }

        //public async Task<bool> UpdateProfileBody(ProfileBody ProfileBody)
        //{
        //    var dbProfileBody = profileBodyRepository.Get(ProfileBody.Id);
        //    if (dbProfileBody != null && dbProfileBody.IsActive == true)
        //    {
        //        var setValue = Task.Run(() =>
        //        {
        //            dbProfileBody.Name = ProfileBody.Name;
        //            dbProfileBody.StaffId = ProfileBody.StaffId;
        //            dbProfileBody.CustomerId = ProfileBody.CustomerId;

        //            dbProfileBody.CreatedTime = null;
        //            dbProfileBody.LastestUpdatedTime = DateTime.Now;
        //            dbProfileBody.InactiveTime = null;
        //            dbProfileBody.IsActive = true;
        //        });

        //        await Task.WhenAll(setValue);

        //        return profileBodyRepository.Update(dbProfileBody.Id, dbProfileBody);
        //    }
        //    else
        //    {
        //        throw new UserException("Không tìm thấy profile body");
        //    }
        //}

        public async Task<bool> UpdateProfileBodyByStaff(string customerId, string staffId, string name, string profileBodyId, List<(string id, decimal value)> bodySizeId, ProfileBody profileBody)
        {
            var dbProfileBody = profileBodyRepository.Get(profileBodyId);
            var checkExistProfileBodyId = Task.Run(() =>
            {
                if (dbProfileBody != null && dbProfileBody.IsActive == true && dbProfileBody.CustomerId == customerId)
                {
                    if (dbProfileBody.CustomerId == customerId)
                    {
                        var setValue = Task.Run(() =>
                        {
                            dbProfileBody.Name = profileBody.Name;
                            dbProfileBody.StaffId = staffId;
                            dbProfileBody.CustomerId = customerId;
                            dbProfileBody.IsLocked = true;

                            dbProfileBody.LastestUpdatedTime = DateTime.Now;
                            dbProfileBody.InactiveTime = null;
                            dbProfileBody.IsActive = true;
                        });
                    } 
                    else
                    {
                        throw new UserException("Mã Khách hàng không khớp");
                    }               
                }
                else
                {
                    throw new UserException("Mã Id Profile Body không tồn tại trong hệ thống");
                }
            });

            var existBodyAttributeList = bodyAttributeRepository.GetAll(x => x.ProfileBodyId == profileBodyId && x.IsActive == true).ToList();
            var existBodySizeList = bodySizeRepository.GetAll(x => x.IsActive == true).Select(x => new { x.Id, x.MinValidValue, x.MaxValidValue }).ToList();
            
            var tasks = new List<Task>();

            foreach( var bodyAttribute in existBodyAttributeList)
            {
                tasks.Add(Task.Run(() =>
                {
                    foreach (var id in bodySizeId)
                    {
                        if (id.id == bodyAttribute.BodySizeId)
                        {
                            if (existBodySizeList.Any(x => x.Id == id.id && id.value >= x.MinValidValue && id.value <= x.MaxValidValue))
                            {
                                bodyAttribute.Value = id.value;
                                bodyAttribute.LastestUpdatedTime = DateTime.Now;
                            }
                            else
                            {
                                throw new UserException("Số đo không tồn tại trong hệ thống");
                            }
                        }
                        else
                        {
                            
                        }
                    }
                }
                ));
            }

            await Task.WhenAll(tasks);
            bodyAttributeRepository.UpdateRange(existBodyAttributeList);
            return profileBodyRepository.Update(profileBodyId, dbProfileBody);
        }

        public async Task<bool> UpdateProfileBodyByCustomer(string customerId, string name, string profileBodyId, List<(string id, decimal value)> bodySizeId, ProfileBody profileBody)
        {
            var dbProfileBody = profileBodyRepository.Get(profileBodyId);
            var checkExistProfileBodyId = Task.Run(() =>
            {
                if (dbProfileBody != null && dbProfileBody.IsActive == true && dbProfileBody.CustomerId == customerId)
                {
                    if (dbProfileBody.CustomerId == customerId)
                    {
                        if (dbProfileBody.IsLocked == false)
                        {
                            var setValue = Task.Run(() =>
                            {
                                dbProfileBody.Name = profileBody.Name;
                                dbProfileBody.StaffId = null;
                                dbProfileBody.CustomerId = customerId;
                                dbProfileBody.IsLocked = false;

                                dbProfileBody.LastestUpdatedTime = DateTime.Now;
                                dbProfileBody.InactiveTime = null;
                                dbProfileBody.IsActive = true;
                            });
                        }
                        else
                        {
                            throw new UserException("Khách hàng không thể tự cập nhật hồ sơ số đo do nhân viên thực hiện");
                        }
                    }
                    else
                    {
                        throw new UserException("Mã Khách hàng không khớp");
                    }
                }
                else
                {
                    throw new UserException("Mã Id Profile Body không tồn tại trong hệ thống");
                }
            });

            var existBodyAttributeList = bodyAttributeRepository.GetAll(x => x.ProfileBodyId == profileBodyId && x.IsActive == true).ToList();
            var existBodySizeList = bodySizeRepository.GetAll(x => x.IsActive == true).Select(x => new { x.Id, x.MinValidValue, x.MaxValidValue }).ToList();

            var tasks = new List<Task>();

            foreach (var bodyAttribute in existBodyAttributeList)
            {
                tasks.Add(Task.Run(() =>
                {
                    foreach (var id in bodySizeId)
                    {
                        if (id.id == bodyAttribute.BodySizeId)
                        {
                            if (existBodySizeList.Any(x => x.Id == id.id && id.value >= x.MinValidValue && id.value <= x.MaxValidValue))
                            {
                                bodyAttribute.Value = id.value;
                                bodyAttribute.LastestUpdatedTime = DateTime.Now;
                            }
                            else
                            {
                                throw new UserException("Số đo không tồn tại trong hệ thống");
                            }
                        }
                        else
                        {

                        }
                    }
                }
                ));
            }

            await Task.WhenAll(tasks);
            bodyAttributeRepository.UpdateRange(existBodyAttributeList);
            return profileBodyRepository.Update(profileBodyId, dbProfileBody);
        }

        public async Task<bool> DeleteProfileBody(string id)
        {
            var dbProfileBody = profileBodyRepository.Get(id);
            if (dbProfileBody != null && dbProfileBody.IsActive == true)
            {
                var checkChild = Task.Run(() =>
                {
                    //if (productTemplateRepository.GetAll(x => x.CategoryId == id && x.IsActive == true).Any() || componentTypeRepository.GetAll(x => x.CategoryId == id && x.IsActive == true).Any())
                    //{
                    //    throw new UserException("Không thể xóa danh mục sản phầm này do vẫn còn các mẫu sản phẩm và các loại thành phần sản phẩm vẫn còn thuộc danh mục này");
                    //}
                });
                var setValue = Task.Run(() =>
                {
                    dbProfileBody.CreatedTime = null;
                    dbProfileBody.LastestUpdatedTime = DateTime.Now;
                    dbProfileBody.IsActive = false;
                    dbProfileBody.InactiveTime = DateTime.Now;
                });

                await Task.WhenAll(checkChild, setValue);

                return profileBodyRepository.Update(dbProfileBody.Id, dbProfileBody);
            }
            else
            {
                throw new UserException("Không tìm thấy danh mục sản phầm");
            }
        }

        //public async Task<ProfileBody> GetProfileBody(string id)
        //{
        //    //string? search = null;
        //    //Task<BodySize> bodySize;
        //    var profileBody = profileBodyRepository.Get(id);
        //    //var bodyAttributeList = bodyAttributeService.GetBodyAttributesByProfileBodyId(id);
        //    //foreach( var bodyAttribute in bodyAttributeList )
        //    //{
        //    //    bodySize = bodySizeService.GetBodySize(bodyAttribute.BodySizeId);
        //    //}

        //    ////var bodySize = bodySizeService.GetBodySizes(search);


        //    return profileBody == null ? null : profileBody.IsActive == true ? profileBody : null;
        //}

        public ProfileBody GetProfileBody(string id)
        {
            var profileBody = profileBodyRepository.Get(id);

            return profileBody == null ? null : profileBody.IsActive == true ? profileBody : null;
        }

        public IEnumerable<ProfileBody> GetProfileBodysOfCustomerId(string? search)
        {

            return profileBodyRepository.GetAll(x => ((search != null && x.CustomerId != null && x.CustomerId.Trim().ToLower().Contains(search.ToLower().Trim()))) && x.IsActive == true);
        }

        public IEnumerable<ProfileBody> GetProfileBodysByCustomerId(string? search)
        {
            return profileBodyRepository.GetAll(x => ((search != null && x.CustomerId != null && x.CustomerId.Trim().ToLower().Contains(search.ToLower().Trim()))) && x.IsActive == true);
        }

        public IEnumerable<ProfileBody> GetProfileBodysByStaffId(string? search)
        {
            return profileBodyRepository.GetAll(x => ((search != null && x.StaffId != null && x.StaffId.Trim().ToLower().Contains(search.ToLower().Trim()))) && x.IsActive == true);
        }
    }
}
