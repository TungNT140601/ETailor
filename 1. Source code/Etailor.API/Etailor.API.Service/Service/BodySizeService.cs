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

            var dupplicateName = bodySizeRepository.GetAll(x => !string.IsNullOrWhiteSpace(bodySize.Name) && x.Name.Trim().ToLower() == bodySize.Name.Trim().ToLower() && x.IsActive == true);
            if (dupplicateName != null && dupplicateName.Any())
            {
                dupplicateName = dupplicateName.ToList();
            }
            else
            {
                dupplicateName = null;
            }
            var tasks = new List<Task>();

            tasks.Add(Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(bodySize.Name))
                {
                    throw new UserException("Tên thuật ngữ số đo cơ thể không được để trống.");
                }
                else
                {
                    if (dupplicateName != null)
                    {
                        throw new UserException("Tên thuật ngữ số đo cơ thể này đã tồn tại.");
                    }
                }
            }));

            tasks.Add(Task.Run(async () =>
            {
                if (image != null)
                {
                    bodySize.Image = await Ultils.UploadImage(wwwroot, "BodySize", image, null);
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                if (!bodySize.MinValidValue.HasValue)
                {
                    throw new UserException("Giá trị tối thiếu không được để trống.");
                }
                else if (bodySize.MinValidValue < 0)
                {
                    throw new UserException("Giá trị tối thiếu không hợp lệ.");
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                if (!bodySize.MaxValidValue.HasValue)
                {
                    throw new UserException("Giá trị tối thiếu không được để trống.");
                }
                else if (bodySize.MaxValidValue < 0)
                {
                    throw new UserException("Giá trị tối thiếu không hợp lệ.");
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                if (bodySize.MinValidValue > bodySize.MaxValidValue)
                {
                    throw new UserException("Giá trị tối thiếu không được lớn hơn giá trị tối đa.");
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                bodySize.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                bodySize.InactiveTime = null;
                bodySize.IsActive = true;
            }));

            await Task.WhenAll(tasks);

            return bodySizeRepository.Create(bodySize);
        }

        public async Task<bool> UpdateBodySize(BodySize bodySize, string wwwroot, IFormFile? image)
        {
            var existBodySize = bodySizeRepository.Get(bodySize.Id);
            if (existBodySize != null)
            {
                var dupplicateName = bodySizeRepository.GetAll(x => !string.IsNullOrWhiteSpace(bodySize.Name) && x.Id != existBodySize.Id && x.Name.Trim().ToLower() == bodySize.Name.Trim().ToLower() && x.IsActive == true);
                if (dupplicateName != null && dupplicateName.Any())
                {
                    dupplicateName = dupplicateName.ToList();
                }
                else
                {
                    dupplicateName = null;
                }
                var tasks = new List<Task>();

                tasks.Add(Task.Run(() =>
                {
                    if (string.IsNullOrWhiteSpace(bodySize.Name))
                    {
                        throw new UserException("Tên thuật ngữ số đo cơ thể không được để trống.");
                    }
                    else
                    {
                        if (dupplicateName != null)
                        {
                            throw new UserException("Tên thuật ngữ số đo cơ thể này đã tồn tại.");
                        }
                        else
                        {
                            existBodySize.Name = bodySize.Name;
                        }
                    }
                }));

                tasks.Add(Task.Run(async () =>
                {
                    if (image != null)
                    {
                        existBodySize.Image = await Ultils.UploadImage(wwwroot, "BodySize", image, existBodySize.Image);
                    }
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (!string.IsNullOrWhiteSpace(bodySize.GuideVideoLink))
                    {
                        existBodySize.GuideVideoLink = bodySize.GuideVideoLink;
                    }
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (!bodySize.MinValidValue.HasValue)
                    {
                        throw new UserException("Giá trị tối thiếu không được để trống.");
                    }
                    else if (bodySize.MinValidValue < 0)
                    {
                        throw new UserException("Giá trị tối thiếu không hợp lệ.");
                    }
                    else
                    {
                        if (bodySize.MinValidValue > existBodySize.MinValidValue)
                        {
                            throw new UserException("Giá trị tối thiếu mới không được lớn hơn giá trị tối thiểu hiện tại.");
                        }
                        else
                        {
                            existBodySize.MinValidValue = bodySize.MinValidValue;
                        }
                    }
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (!bodySize.MaxValidValue.HasValue)
                    {
                        throw new UserException("Giá trị tối thiếu không được để trống.");
                    }
                    else if (bodySize.MaxValidValue < 0)
                    {
                        throw new UserException("Giá trị tối thiếu không hợp lệ.");
                    }
                    else
                    {
                        if (bodySize.MaxValidValue < existBodySize.MaxValidValue)
                        {
                            throw new UserException("Giá trị tối đa mới không được nhỏ hơn giá trị tối đa hiện tại.");
                        }
                        else
                        {
                            existBodySize.MaxValidValue = bodySize.MaxValidValue;
                        }
                    }
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (bodySize.MinValidValue > bodySize.MaxValidValue)
                    {
                        throw new UserException("Giá trị tối thiếu không được lớn hơn giá trị tối đa.");
                    }
                }));

                tasks.Add(Task.Run(() =>
                {
                    existBodySize.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                    existBodySize.InactiveTime = null;
                    existBodySize.IsActive = true;
                }));

                await Task.WhenAll(tasks);

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
            if (existBodySize != null && existBodySize.IsActive == true)
            {
                existBodySize.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                existBodySize.InactiveTime = DateTime.UtcNow.AddHours(7);
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

            if (string.IsNullOrEmpty(bodySize.Image))
            {
                bodySize.Image = "https://firebasestorage.googleapis.com/v0/b/etailor-21a50.appspot.com/o/Uploads%2FThumbnail%2Fstill-life-spring-wardrobe-switch.jpg?alt=media&token=7dc9a197-1b76-4525-8dc7-caa2238d8327";
            }
            else
            {
                bodySize.Image = Ultils.GetUrlImage(bodySize.Image);

            }

            return bodySize == null ? null : bodySize.IsActive == true ? bodySize : null;
        }

        public async Task<IEnumerable<BodySize>> GetBodySizes(string? search)
        {
            var listOfBodySize = bodySizeRepository.GetAll(x => (search == null || (search != null && x.Name.Trim().ToLower().Contains(search.Trim().ToLower()))) && x.IsActive == true);

            if (listOfBodySize != null && listOfBodySize.Any())
            {
                listOfBodySize = listOfBodySize.OrderBy(x => x.BodyIndex).OrderBy(x => x.Name).ToList();

                var tasks = new List<Task>();
                foreach (var bodySize in listOfBodySize)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        if (string.IsNullOrEmpty(bodySize.Image))
                        {
                            bodySize.Image = "https://firebasestorage.googleapis.com/v0/b/etailor-21a50.appspot.com/o/Uploads%2FThumbnail%2Fstill-life-spring-wardrobe-switch.jpg?alt=media&token=7dc9a197-1b76-4525-8dc7-caa2238d8327";
                        }
                        else
                        {
                            bodySize.Image = Ultils.GetUrlImage(bodySize.Image);

                        }
                    }));
                };
                await Task.WhenAll(tasks);
            }

            return listOfBodySize;
        }
    }
}
