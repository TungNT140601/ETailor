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

namespace Etailor.API.Service.Service
{
    public class ProfileBodyService : IProfileBodyService
    {
        private readonly ICustomerRepository customerRepository;
        private readonly IStaffRepository staffRepository;
        private readonly IProfileBodyRepository profileBodyRepository;
        private readonly IBodySizeRepository bodySizeRepository;
        private readonly IBodyAttributeRepository bodyAttributeRepository;

        public ProfileBodyService(ICustomerRepository customerRepository, IStaffRepository staffRepository, IProfileBodyRepository profileBodyRepository,
            IBodySizeRepository bodySizeRepository, IBodyAttributeRepository bodyAttributeRepository)
        {
            this.customerRepository = customerRepository;
            this.staffRepository = staffRepository;
            this.profileBodyRepository = profileBodyRepository;
            this.bodySizeRepository = bodySizeRepository;
            this.bodyAttributeRepository = bodyAttributeRepository;
        }

        public async Task<bool> AddProfileBody(ProfileBody ProfileBody)
        {
            var checkDuplicateId = Task.Run(() =>
            {
                if (profileBodyRepository.GetAll(x => x.Id == ProfileBody.Id && x.IsActive == true).Any())
                {
                    throw new UserException("Mã Id Profile Body đã được sử dụng");
                }
            });
            var setValue = Task.Run(() =>
            {
                ProfileBody.Id = Ultils.GenGuidString();
                ProfileBody.CreatedTime = DateTime.Now;
                ProfileBody.LastestUpdatedTime = DateTime.Now;
                ProfileBody.InactiveTime = null;
                ProfileBody.IsActive = true;
            });

            await Task.WhenAll(checkDuplicateId, setValue);

            return profileBodyRepository.Create(ProfileBody);
        }

        public async Task<bool> CreateProfileBody(string name, List<Tuple<string, decimal>> bodySizeId)
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
                profileBody.Name = name;
                profileBody.IsLocked = true;
                profileBody.CreatedTime = DateTime.Now;
                profileBody.LastestUpdatedTime = DateTime.Now;
                profileBody.InactiveTime = null;
                profileBody.IsActive = true;
            });

            await Task.WhenAll(checkDuplicateId, setValue);

            profileBodyRepository.Create(profileBody);

            var existBodySizeList = bodySizeRepository.GetAll(x => x.IsActive == true).Select(x => x.Id).ToList();
            var existBodyAttributeList = bodyAttributeRepository.GetAll(x => x.ProfileBodyId == profileBody.Id && x.IsActive == true).Select(x => x.BodySizeId).ToList();

            var bodyAttributeList = new List<BodyAttribute>();
            var tasks = new List<Task>();
            foreach (var id in bodySizeId)
            {
                tasks.Add(Task.Run(() =>
                {
                    if (existBodySizeList.Contains(id.Item1))
                    {
                        if (!existBodyAttributeList.Contains(id.Item1))
                        {
                            bodyAttributeList.Add(new BodyAttribute()
                            {
                                Id = Ultils.GenGuidString(),
                                BodySizeId = id.Item1,
                                ProfileBodyId = profileBody.Id,
                                Value = id.Item2,
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

            return bodyAttributeRepository.CreateRange(bodyAttributeList);
            
        }

        public async Task<bool> UpdateProfileBody(ProfileBody ProfileBody)
        {
            var dbProfileBody = profileBodyRepository.Get(ProfileBody.Id);
            if (dbProfileBody != null && dbProfileBody.IsActive == true)
            {
                var setValue = Task.Run(() =>
                {
                    dbProfileBody.Name = ProfileBody.Name;
                    dbProfileBody.StaffId = ProfileBody.StaffId;
                    dbProfileBody.CustomerId = ProfileBody.CustomerId;

                    dbProfileBody.CreatedTime = null;
                    dbProfileBody.LastestUpdatedTime = DateTime.Now;
                    dbProfileBody.InactiveTime = null;
                    dbProfileBody.IsActive = true;
                });

                await Task.WhenAll(setValue);

                return profileBodyRepository.Update(dbProfileBody.Id, dbProfileBody);
            }
            else
            {
                throw new UserException("Không tìm thấy profile body");
            }
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

        public ProfileBody GetProfileBody(string id)
        {
            var profile = profileBodyRepository.Get(id);
            return profile == null ? null : profile.IsActive == true ? profile : null;
        }

        public IEnumerable<ProfileBody> GetProfileBodysByCustomerId(string? search)
        {
            return profileBodyRepository.GetAll(x => ((search != null && x.CustomerId.Trim().ToLower().Contains(search.ToLower().Trim()))) && x.IsActive == true);
        }

        public IEnumerable<ProfileBody> GetProfileBodysByStaffId(string? search)
        {
            return profileBodyRepository.GetAll(x => ((search != null && x.StaffId.Trim().ToLower().Contains(search.ToLower().Trim()))) && x.IsActive == true);
        }
    }
}
