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

        public async Task<bool> CreateProfileBodyByStaff(string customerId, string staffId, string name, List<(string id, decimal? value)> bodySizeId)
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
                profileBody.CreatedTime = DateTime.UtcNow.AddHours(7);
                profileBody.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                profileBody.InactiveTime = null;
                profileBody.IsActive = true;
            });

            await Task.WhenAll(checkDuplicateId, setValue);

            var existBodySizeList = bodySizeRepository.GetAll(x => x.IsActive == true).Select(x => new { x.Id, x.MinValidValue, x.MaxValidValue }).ToList();
            var existBodyAttributeList = bodyAttributeRepository.GetAll(x => x.ProfileBodyId == profileBody.Id && x.IsActive == true).Select(x => x.BodySizeId).ToList();

            var bodyAttributeList = new List<BodyAttribute>();
            var tasks = new List<Task>();
            BodySize bodySizeObject = new BodySize();

            foreach (var id in bodySizeId)
            {
                bodySizeObject = bodySizeRepository.Get(id.id);
                var minValidValue = bodySizeObject.MinValidValue;
                var maxValidValue = bodySizeObject.MaxValidValue;
                tasks.Add(Task.Run(() =>
                {
                    if (existBodySizeList.Any(x => x.Id == id.id && id.value >= x.MinValidValue && id.value <= x.MaxValidValue))
                    {
                        if (id.value >= minValidValue && id.value <= maxValidValue)
                        {
                            if (!existBodyAttributeList.Contains(id.id))
                            {
                                bodyAttributeList.Add(new BodyAttribute()
                                {
                                    Id = Ultils.GenGuidString(),
                                    BodySizeId = id.id,
                                    ProfileBodyId = profileBody.Id,
                                    Value = id.value,
                                    CreatedTime = DateTime.UtcNow.AddHours(7),
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
                            throw new UserException("Giá trị số đo phải phù hợp");
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

        public async Task<bool> CreateProfileBodyByCustomer(string customerId, string name, List<(string id, decimal? value)> bodySizeId)
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
                profileBody.CreatedTime = DateTime.UtcNow.AddHours(7);
                profileBody.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                profileBody.InactiveTime = null;
                profileBody.IsActive = true;
            });

            await Task.WhenAll(checkDuplicateId, setValue);

            var existBodySizeList = bodySizeRepository.GetAll(x => x.IsActive == true).Select(x => new { x.Id, x.MinValidValue, x.MaxValidValue }).ToList();
            var existBodyAttributeList = bodyAttributeRepository.GetAll(x => x.ProfileBodyId == profileBody.Id && x.IsActive == true).Select(x => x.BodySizeId).ToList();

            var bodyAttributeList = new List<BodyAttribute>();
            var tasks = new List<Task>();
            BodySize bodySizeObject = new BodySize();
            foreach (var id in bodySizeId)
            {
                bodySizeObject = bodySizeRepository.Get(id.id);
                var minValidValue = bodySizeObject.MinValidValue;
                var maxValidValue = bodySizeObject.MaxValidValue;
                tasks.Add(Task.Run(() =>
                {
                    if (existBodySizeList.Any(x => x.Id == id.id))
                    {
                        if (id.value >= minValidValue && id.value <= maxValidValue)
                        {
                            if (!existBodyAttributeList.Contains(id.id))
                            {
                                bodyAttributeList.Add(new BodyAttribute()
                                {
                                    Id = Ultils.GenGuidString(),
                                    BodySizeId = id.id,
                                    ProfileBodyId = profileBody.Id,
                                    Value = id.value,
                                    CreatedTime = DateTime.UtcNow.AddHours(7),
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
                            throw new UserException("Giá trị số đo phải phù hợp");
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

        public async Task<bool> UpdateProfileBody(string customerId, string? staffId, string name, string profileBodyId, List<BodyAttribute>? bodyAttributes, ProfileBody profileBody)
        {
            var dbProfileBody = profileBodyRepository.Get(profileBodyId);
            if (dbProfileBody != null && dbProfileBody.IsActive == true && dbProfileBody.CustomerId == customerId)
            {
                if (dbProfileBody.CustomerId == customerId)
                {
                    dbProfileBody.Name = profileBody.Name;
                    dbProfileBody.StaffId = staffId;
                    dbProfileBody.CustomerId = customerId;
                    dbProfileBody.IsLocked = staffId != null;

                    dbProfileBody.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                    dbProfileBody.InactiveTime = null;
                    dbProfileBody.IsActive = true;
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

            if (profileBodyRepository.Update(dbProfileBody.Id, dbProfileBody))
            {
                var existBodySizeList = bodySizeRepository.GetAll(x => x.IsActive == true);
                if (existBodySizeList != null && existBodySizeList.Any())
                {
                    existBodySizeList = existBodySizeList.ToList();

                    var existBodyAttributeList = bodyAttributeRepository.GetAll(x => x.ProfileBodyId == profileBodyId && x.IsActive == true);
                    if (existBodyAttributeList != null && existBodyAttributeList.Any())
                    {
                        existBodyAttributeList = existBodyAttributeList.ToList();

                        var tasks = new List<Task>();
                        var addNewAttributes = new List<BodyAttribute>();
                        var updateAttributes = new List<BodyAttribute>();

                        foreach (var bodySize in existBodySizeList)
                        {
                            tasks.Add(Task.Run(() =>
                            {
                                var existBodyAttribute = existBodyAttributeList.FirstOrDefault(x => x.BodySizeId == bodySize.Id);
                                var newBodyAttribute = bodyAttributes?.FirstOrDefault(x => x.BodySizeId == bodySize.Id);
                                if (existBodyAttribute != null)
                                {
                                    if (newBodyAttribute != null)
                                    {
                                        if (newBodyAttribute.Value != null && newBodyAttribute.Value >= bodySize.MinValidValue && newBodyAttribute.Value <= bodySize.MaxValidValue)
                                        {
                                            if (newBodyAttribute.Value != existBodyAttribute.Value)
                                            {
                                                existBodyAttribute.Value = newBodyAttribute.Value;
                                                existBodyAttribute.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);

                                                updateAttributes.Add(existBodyAttribute);
                                            }
                                        }
                                        else if (newBodyAttribute.Value == null)
                                        {
                                            if (newBodyAttribute.Value != existBodyAttribute.Value)
                                            {
                                                existBodyAttribute.Value = newBodyAttribute.Value;
                                                existBodyAttribute.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);

                                                updateAttributes.Add(existBodyAttribute);
                                            }
                                        }
                                        else
                                        {
                                            throw new UserException("Giá trị số đo không phù hợp");
                                        }
                                    }
                                }
                                else
                                {
                                    if (newBodyAttribute != null)
                                    {
                                        if (newBodyAttribute.Value != null && newBodyAttribute.Value >= bodySize.MinValidValue && newBodyAttribute.Value <= bodySize.MaxValidValue)
                                        {
                                            addNewAttributes.Add(new BodyAttribute()
                                            {
                                                Id = Ultils.GenGuidString(),
                                                BodySizeId = bodySize.Id,
                                                CreatedTime = DateTime.UtcNow.AddHours(7),
                                                LastestUpdatedTime = DateTime.UtcNow.AddHours(7),
                                                InactiveTime = null,
                                                IsActive = true,
                                                ProfileBodyId = profileBodyId,
                                                Value = newBodyAttribute.Value
                                            });
                                        }
                                        else if (newBodyAttribute.Value == null)
                                        {

                                        }
                                        else
                                        {
                                            throw new UserException("Giá trị số đo không phù hợp");
                                        }
                                    }
                                }
                            }));
                        }

                        await Task.WhenAll(tasks);

                        var listCheck = new List<bool>();
                        if (addNewAttributes.Any())
                        {
                            foreach (var item in addNewAttributes)
                            {
                                listCheck.Add(bodyAttributeRepository.Create(item));
                            }
                        }

                        if (updateAttributes.Any())
                        {
                            foreach (var item in updateAttributes)
                            {
                                listCheck.Add(bodyAttributeRepository.Update(item.Id, item));
                            }
                        }

                        return !listCheck.Any(x => x == false);
                    }
                    else
                    {
                        var tasks = new List<Task>();
                        var addNewAttributes = new List<BodyAttribute>();
                        if (bodyAttributes != null && bodyAttributes.Any())
                        {

                            foreach (var bodySize in existBodySizeList)
                            {
                                tasks.Add(Task.Run(() =>
                                {
                                    var newBodyAttribute = bodyAttributes.FirstOrDefault(x => x.BodySizeId == bodySize.Id);
                                    if (newBodyAttribute != null)
                                    {
                                        if (newBodyAttribute.Value != null && newBodyAttribute.Value >= bodySize.MinValidValue && newBodyAttribute.Value <= bodySize.MaxValidValue)
                                        {
                                            addNewAttributes.Add(new BodyAttribute()
                                            {
                                                Id = Ultils.GenGuidString(),
                                                BodySizeId = bodySize.Id,
                                                ProfileBodyId = profileBodyId,
                                                Value = newBodyAttribute.Value,
                                                CreatedTime = DateTime.UtcNow.AddHours(7),
                                                LastestUpdatedTime = DateTime.UtcNow.AddHours(7),
                                                InactiveTime = null,
                                                IsActive = true
                                            });
                                        }
                                        else if (newBodyAttribute.Value == null)
                                        {

                                        }
                                        else
                                        {
                                            throw new UserException("Giá trị số đo không phù hợp");
                                        }
                                    }
                                }));
                            }

                            await Task.WhenAll(tasks);

                            if (addNewAttributes.Any())
                            {
                                var check = new List<bool>();
                                foreach (var item in addNewAttributes)
                                {
                                    check.Add(bodyAttributeRepository.Create(item));
                                }
                                return !check.Any(x => x == false);
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
                }
                else
                {
                    throw new UserException("Không có số đo nào trong hệ thống");
                }
            }
            else
            {
                throw new UserException("Cập nhật hồ sơ số đo không thành công");
            }
        }

        public async Task<bool> DeleteProfileBody(string id)
        {
            var dbProfileBody = profileBodyRepository.Get(id);
            if (dbProfileBody != null && dbProfileBody.IsActive == true)
            {
                dbProfileBody.CreatedTime = null;
                dbProfileBody.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                dbProfileBody.IsActive = false;
                dbProfileBody.InactiveTime = DateTime.UtcNow.AddHours(7);

                return profileBodyRepository.Update(dbProfileBody.Id, dbProfileBody);
            }
            else
            {
                throw new UserException("Không tìm thấy hồ sơ số đo");
            }
        }

        public async Task<ProfileBody> GetProfileBody(string id)
        {
            var profileBody = profileBodyRepository.Get(id);

            if (profileBody != null && profileBody.IsActive == true)
            {
                var bodySizes = bodySizeRepository.GetAll(x => x.IsActive == true);
                if (bodySizes != null && bodySizes.Any())
                {
                    bodySizes = bodySizes.OrderBy(x => x.BodyIndex).ToList();

                    var tasks = new List<Task>();

                    var bodyAttributeList = bodyAttributeRepository.GetAll(x => x.ProfileBodyId == id && x.IsActive == true);
                    if (bodyAttributeList != null && bodyAttributeList.Any())
                    {
                        bodyAttributeList = bodyAttributeList.ToList();

                        profileBody.BodyAttributes = new List<BodyAttribute>();

                        foreach (var bodySize in bodySizes)
                        {
                            tasks.Add(Task.Run(async () =>
                            {
                                if (!string.IsNullOrEmpty(bodySize.Image))
                                {
                                    bodySize.Image = Ultils.GetUrlImage(bodySize.Image);
                                }

                                var bodyAttribute = bodyAttributeList.FirstOrDefault(x => x.BodySizeId == bodySize.Id);

                                if (bodyAttribute != null)
                                {
                                    bodyAttribute.BodySize = bodySize;
                                    profileBody.BodyAttributes.Add(bodyAttribute);
                                }
                                else
                                {
                                    profileBody.BodyAttributes.Add(new BodyAttribute()
                                    {
                                        Id = Ultils.GenGuidString(),
                                        BodySizeId = bodySize.Id,
                                        CreatedTime = DateTime.UtcNow.AddHours(7),
                                        InactiveTime = null,
                                        IsActive = true,
                                        LastestUpdatedTime = DateTime.UtcNow.AddHours(7),
                                        ProfileBodyId = profileBody.Id,
                                        ProfileBody = profileBody,
                                        Value = null,
                                        BodySize = bodySize
                                    });
                                }
                            }));
                        }
                        await Task.WhenAll(tasks);
                    }
                    else
                    {
                        profileBody.BodyAttributes = new List<BodyAttribute>();
                        foreach (var bodySize in bodySizes)
                        {
                            tasks.Add(Task.Run(() =>
                            {
                                profileBody.BodyAttributes.Add(new BodyAttribute()
                                {
                                    Id = Ultils.GenGuidString(),
                                    BodySizeId = bodySize.Id,
                                    BodySize = bodySize,
                                    Value = null
                                });
                            }));
                        }
                        await Task.WhenAll(tasks);
                    }
                }
                if (profileBody.BodyAttributes != null && profileBody.BodyAttributes.Any())
                {
                    profileBody.BodyAttributes = profileBody.BodyAttributes.OrderBy(x => x.BodySize.BodyIndex).OrderBy(x => x.BodySize.Name).ToList();
                }

                return profileBody;
            }

            return null;
        }

        public IEnumerable<ProfileBody> GetProfileBodysByCustomerId(string customerId)
        {
            return profileBodyRepository.GetAll(x => x.CustomerId == customerId && x.IsActive == true)?.OrderByDescending(x => x.LastestUpdatedTime);
        }

        public IEnumerable<ProfileBody> GetProfileBodysByStaffId(string? search)
        {
            return profileBodyRepository.GetAll(x => ((search != null && x.StaffId != null && x.StaffId.Trim().ToLower().Contains(search.ToLower().Trim()))) && x.IsActive == true);
        }
    }
}
