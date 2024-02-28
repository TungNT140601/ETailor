using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.Repository;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CustomException;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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

        public async Task<bool> CreateBodySize(BodySize bodySize, string wwwroot, IFormFile? image)
        {
            bodySize.Id = Ultils.GenGuidString();

            var setThumbnail = Task.Run(async () =>
            {
                if (image != null)
                {
                    bodySize.Image = await Ultils.UploadImage(wwwroot, "BodySize", image, null);
                }
                else
                {
                    bodySize.Image = string.Empty;
                    //blog.Thumbnail = await Ultils.GetUrlImage("https://drive.google.com/file/d/100YI-uovn5PdEhn4IeB1RYj4B41kcFIi/view");
                }
            });
            await Task.WhenAll(setThumbnail);

            
            bodySize.LastestUpdatedTime = DateTime.Now;
            bodySize.CreatedTime = DateTime.Now;
            bodySize.InactiveTime = null;
            bodySize.IsActive = true;
            return bodySizeRepository.Create(bodySize);
        }

        public async Task<bool> UpdateBodySize(BodySize bodySize, string wwwroot, IFormFile? image)
        {
            var existBodySize= bodySizeRepository.Get(bodySize.Id);
            if (existBodySize != null)
            {
                existBodySize.Name = bodySize.Name;
                if (image != null)
                {
                    bodySize.Image = await Ultils.UploadImage(wwwroot, "BodySize", image, bodySize.Image);
                }
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

        public async Task<BodySize> GetBodySize(string id)
        {
            var bodySize = bodySizeRepository.Get(id);

            var setThumbnail = Task.Run(async () =>
            {
                if (string.IsNullOrEmpty(bodySize.Image))
                {
                    bodySize.Image = "https://firebasestorage.googleapis.com/v0/b/etailor-21a50.appspot.com/o/Uploads%2FThumbnail%2Fstill-life-spring-wardrobe-switch.jpg?alt=media&token=7dc9a197-1b76-4525-8dc7-caa2238d8327";
                }
                else
                {
                    bodySize.Image = await Ultils.GetUrlImage(bodySize.Image);
                }
            });
            await Task.WhenAll(setThumbnail);

            return bodySize == null ? null : bodySize.IsActive == true ? bodySize : null;
        }

        public async Task<IEnumerable<BodySize>> GetBodySizes(string? search)
        {
            IEnumerable<BodySize> ListOfBodySize = bodySizeRepository.GetAll(x => (search == null || (search != null && x.Name.Trim().ToLower().Contains(search.Trim().ToLower()))) && x.IsActive == true);
            foreach (BodySize bodySize in ListOfBodySize)
            {
                var setThumbnail = Task.Run(async () =>
                {
                    if (string.IsNullOrEmpty(bodySize.Image))
                    {
                        bodySize.Image = "https://firebasestorage.googleapis.com/v0/b/etailor-21a50.appspot.com/o/Uploads%2FThumbnail%2Fstill-life-spring-wardrobe-switch.jpg?alt=media&token=7dc9a197-1b76-4525-8dc7-caa2238d8327";
                    }
                    else
                    {
                        bodySize.Image = await Ultils.GetUrlImage(bodySize.Image);
                    }
                });
                await Task.WhenAll(setThumbnail);
            };
            return ListOfBodySize;
        }
    }
}
