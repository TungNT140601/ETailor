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

        public ProfileBodyService(ICustomerRepository customerRepository, IStaffRepository staffRepository, IProfileBodyRepository profileBodyRepository)
        {
            this.customerRepository = customerRepository;
            this.staffRepository = staffRepository;
            this.profileBodyRepository = profileBodyRepository;
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

        public async Task<bool> UpdateProfileBody(ProfileBody ProfileBody)
        {
            var dbProduct = profileBodyRepository.Get(ProfileBody.Id);
            if (dbProduct != null && dbProduct.IsActive == true)
            {
                var setValue = Task.Run(() =>
                {
                    dbProduct.Name = ProfileBody.Name;
                    dbProduct.StaffId = ProfileBody.StaffId;

                    dbProduct.CreatedTime = null;
                    dbProduct.LastestUpdatedTime = DateTime.Now;
                    dbProduct.InactiveTime = null;
                    dbProduct.IsActive = true;
                });

                await Task.WhenAll(setValue);

                return profileBodyRepository.Update(dbProduct.Id, dbProduct);
            }
            else
            {
                throw new UserException("Không tìm thấy profile body");
            }
        }

        public async Task<bool> DeleteProfileBody(string id)
        {
            var dbProduct = profileBodyRepository.Get(id);
            if (dbProduct != null && dbProduct.IsActive == true)
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
                    dbProduct.CreatedTime = null;
                    dbProduct.LastestUpdatedTime = DateTime.Now;
                    dbProduct.IsActive = false;
                    dbProduct.InactiveTime = DateTime.Now;
                });

                await Task.WhenAll(checkChild, setValue);

                return profileBodyRepository.Update(dbProduct.Id, dbProduct);
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
