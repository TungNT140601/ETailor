using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
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
    public class BodyAttributeService : IBodyAttributeService
    {
        private readonly IBodyAttributeRepository bodyAttributeRepository;
        private readonly IProfileBodyRepository profileBodyRepository;
        private readonly IBodySizeRepository bodySizeRepository;

        public BodyAttributeService(IBodyAttributeRepository bodyAttributeRepository,
            IProfileBodyRepository profileBodyRepository, IBodySizeRepository bodySizeRepository)
        {
            this.bodyAttributeRepository = bodyAttributeRepository;
            this.profileBodyRepository = profileBodyRepository;
            this.bodySizeRepository = bodySizeRepository;
        }

        public async Task<bool> AddBodyAttribute(BodyAttribute bodyAttribute)
        {
            var checkDuplicateId = Task.Run(() =>
            {
                if (profileBodyRepository.GetAll(x => x.Id == bodyAttribute.Id && x.IsActive == true).Any())
                {
                    throw new UserException("Mã Id số đo này đã được sử dụng");
                }
            });
            var setValue = Task.Run(() =>
            {
                bodyAttribute.Id = Ultils.GenGuidString();
                bodyAttribute.CreatedTime = DateTime.UtcNow.AddHours(7);
                bodyAttribute.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                bodyAttribute.InactiveTime = null;
                bodyAttribute.IsActive = true;
            });

            await Task.WhenAll(checkDuplicateId, setValue);

            return bodyAttributeRepository.Create(bodyAttribute);
        }

        public async Task<bool> UpdateBodyAttribute(BodyAttribute bodyAttribute)
        {
            var dbBodyAttribute = bodyAttributeRepository.Get(bodyAttribute.Id);
            if (dbBodyAttribute != null && dbBodyAttribute.IsActive == true)
            {
                var setValue = Task.Run(() =>
                {
                    dbBodyAttribute.Value = bodyAttribute.Value;

                    dbBodyAttribute.CreatedTime = null;
                    dbBodyAttribute.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                    dbBodyAttribute.InactiveTime = null;
                    dbBodyAttribute.IsActive = true;
                });

                await Task.WhenAll(setValue);

                return bodyAttributeRepository.Update(dbBodyAttribute.Id, dbBodyAttribute);
            }
            else
            {
                throw new UserException("Không tìm thấy số đo này");
            }
        }

        public async Task<bool> DeleteBodyAttribute(string id)
        {
            var dbBodyAttribute = bodyAttributeRepository.Get(id);
            if (dbBodyAttribute != null && dbBodyAttribute.IsActive == true)
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
                    dbBodyAttribute.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                    dbBodyAttribute.IsActive = false;
                    dbBodyAttribute.InactiveTime = DateTime.UtcNow.AddHours(7);
                });

                await Task.WhenAll(checkChild, setValue);

                return bodyAttributeRepository.Update(dbBodyAttribute.Id, dbBodyAttribute);
            }
            else
            {
                throw new UserException("Không tìm thấy số đo");
            }
        }

        public BodyAttribute GetBodyAttribute(string id)
        {
            var bodyAttribute = bodyAttributeRepository.Get(id);
            return bodyAttribute == null ? null : bodyAttribute.IsActive == true ? bodyAttribute : null;
        }

        public IEnumerable<BodyAttribute> GetBodyAttributesByProfileBodyId(string? search)
        {
            return bodyAttributeRepository.GetAll(x => ((search != null && x.ProfileBodyId.Trim().ToLower().Contains(search.ToLower().Trim()))) && x.IsActive == true);
        }

        public IEnumerable<BodyAttribute> GetBodyAttributesByBodySizeId(string? search)
        {
            return bodyAttributeRepository.GetAll(x => ((search != null && x.BodySizeId.Trim().ToLower().Contains(search.ToLower().Trim()))) && x.IsActive == true);
        }
    }
}
