using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.Repository;
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
    public class BodySizeService : IBodySizeService
    {
        private readonly IBodySizeRepository bodySizeRepository;
        private readonly IBodyAttributeRepository bodyAttributeRepository;
        private readonly ITemplateBodySizeRepository templateBodySizeRepository;

        public BodySizeService(IBodySizeRepository bodySizeRepository, 
            IBodyAttributeRepository bodyAttributeRepository, ITemplateBodySizeRepository templateBodySizeRepository)
        {
            this.bodySizeRepository = bodySizeRepository;
            this.bodyAttributeRepository = bodyAttributeRepository;
            this.templateBodySizeRepository = templateBodySizeRepository;
        }

        public bool CreateBodySize(BodySize bodySize)
        {
            bodySize.Id = Ultils.GenGuidString();
            bodySize.LastestUpdatedTime = DateTime.Now;
            bodySize.CreatedTime = DateTime.Now;
            bodySize.InactiveTime = null;
            bodySize.IsActive = true;
            return bodySizeRepository.Create(bodySize);
        }

        public bool UpdateBodySize(BodySize bodySize)
        {
            var existBodySize= bodySizeRepository.Get(bodySize.Id);
            if (existBodySize != null)
            {
                existBodySize.Name = bodySize.Name;
                existBodySize.Image = bodySize.Image;
                existBodySize.GuideVideoLink = bodySize.GuideVideoLink;
                existBodySize.MinValidValue = bodySize.MinValidValue;
                existBodySize.MaxValidValue = bodySize.MaxValidValue;

                existBodySize.LastestUpdatedTime = DateTime.Now;
                existBodySize.InactiveTime = null;
                existBodySize.IsActive = true;

                return bodySizeRepository.Update(existBodySize.Id, existBodySize);
            }
            else
            {
                throw new UserException("Không tìm thấy thuật ngữ số đo cơ thể này.");
            }
        }

        public bool DeleteBodySize(string id)
        {
            var existBodySize = bodySizeRepository.Get(id);
            if (existBodySize != null)
            {
                existBodySize.LastestUpdatedTime = DateTime.Now;
                existBodySize.InactiveTime = DateTime.Now;
                existBodySize.IsActive = false;
                return bodySizeRepository.Update(existBodySize.Id, existBodySize);
            }
            else
            {
                throw new UserException("Không tìm thấy thuật ngữ số đo cơ thể này.");
            }
        }

        public BodySize GetBodySize(string id)
        {
            var bodySize = bodySizeRepository.Get(id);
            return bodySize == null ? null : bodySize.IsActive == true ? bodySize : null;
        }

        public IEnumerable<BodySize> GetBodySizes(string? search)
        {
            return bodySizeRepository.GetAll(x => (search == null || (search != null && x.Name.Trim().ToLower().Contains(search.Trim().ToLower()))) && x.IsActive == true);
        }
    }
}
