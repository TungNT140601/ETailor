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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class TemplateBodySizeService : ITemplateBodySizeService
    {
        private readonly ITemplateBodySizeRepository templateBodySizeRepository;
        private readonly IProductTemplateRepository productTemplateRepository;
        private readonly IBodySizeRepository bodySizeRepository;

        public TemplateBodySizeService(ITemplateBodySizeRepository templateBodySizeRepository, IProductTemplateRepository productTemplateRepository, IBodySizeRepository bodySizeRepository)
        {
            this.templateBodySizeRepository = templateBodySizeRepository;
            this.productTemplateRepository = productTemplateRepository;
            this.bodySizeRepository = bodySizeRepository;
        }

        public async Task<bool> CreateTemplateBodySize(List<string> ids, string templateId)
        {
            var template = productTemplateRepository.Get(templateId);
            if (template == null || template.IsActive != false)
            {
                throw new UserException("Mẫu sản phẩm không tìm thấy");
            }
            else
            {
                var activeBodySizes = bodySizeRepository.GetAll(x => x.IsActive == true);
                if (activeBodySizes != null && activeBodySizes.Any())
                {
                    activeBodySizes = activeBodySizes.ToList();

                    var currentTemplateBodySizes = templateBodySizeRepository.GetAll(x => x.ProductTemplateId == templateId);
                    if (currentTemplateBodySizes != null && currentTemplateBodySizes.Any())
                    {
                        currentTemplateBodySizes = currentTemplateBodySizes.ToList();

                        var listBodySizeTemplates = new List<TemplateBodySize>();
                        var listUpdateBodySizeTemplates = new List<TemplateBodySize>();
                        var tasks = new List<Task>();
                        if (ids == null || ids.Count == 0)
                        {
                            throw new UserException("Danh sách số đo không được để trống");
                        }
                        else
                        {
                            foreach (string id in ids)
                            {
                                tasks.Add(Task.Run(() =>
                                {
                                    if (activeBodySizes.Select(x => x.Id).Contains(id))
                                    {
                                        if (!currentTemplateBodySizes.Select(x => x.Id).Contains(id))
                                        {
                                            listBodySizeTemplates.Add(new TemplateBodySize()
                                            {
                                                Id = Ultils.GenGuidString(),
                                                BodySizeId = id,
                                                InactiveTime = null,
                                                IsActive = true,
                                                ProductTemplateId = templateId
                                            });
                                        }
                                        else
                                        {
                                            var currentTemplateBodySize = currentTemplateBodySizes.SingleOrDefault(x => x.BodySizeId == id);
                                            if (currentTemplateBodySize != null)
                                            {
                                                currentTemplateBodySize.IsActive = true;
                                                currentTemplateBodySize.InactiveTime = null;
                                                listUpdateBodySizeTemplates.Add(currentTemplateBodySize);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        throw new UserException("Số đo không tồn tại trong hệ thống");
                                    }
                                }));
                            }

                            foreach (var current in currentTemplateBodySizes)
                            {
                                if (!ids.Contains(current.BodySizeId))
                                {
                                    tasks.Add(Task.Run(() =>
                                    {
                                        current.IsActive = false;
                                        current.InactiveTime = DateTime.Now;
                                        listUpdateBodySizeTemplates.Add(current);
                                    }));
                                }
                            }
                        }

                        await Task.WhenAll(tasks);

                        if (templateBodySizeRepository.CreateRange(listBodySizeTemplates))
                        {
                            return templateBodySizeRepository.UpdateRange(listUpdateBodySizeTemplates);
                        }
                    }
                    else
                    {
                        var listBodySizeTemplates = new List<TemplateBodySize>();
                        var tasks = new List<Task>();
                        if (ids == null || ids.Count == 0)
                        {
                            throw new UserException("Danh sách số đo không được để trống");
                        }
                        else
                        {
                            foreach (string id in ids)
                            {
                                tasks.Add(Task.Run(() =>
                                {
                                    if (activeBodySizes.Select(x => x.Id).Contains(id))
                                    {
                                        listBodySizeTemplates.Add(new TemplateBodySize()
                                        {
                                            Id = Ultils.GenGuidString(),
                                            BodySizeId = id,
                                            InactiveTime = null,
                                            IsActive = true,
                                            ProductTemplateId = templateId
                                        });
                                    }
                                    else
                                    {
                                        throw new UserException("Số đo không tồn tại trong hệ thống");
                                    }
                                }));
                            }
                        }

                        await Task.WhenAll(tasks);

                        return templateBodySizeRepository.CreateRange(listBodySizeTemplates);
                    }
                }

                return false;
            }
        }

        public async Task<bool> UpdateTemplateBodySize(List<string> ids, string templateId)
        {
            var template = productTemplateRepository.Get(templateId);
            if (template == null || template.IsActive != true)
            {
                throw new UserException("Mẫu sản phẩm không tìm thấy");
            }
            else
            {
                var activeBodySizes = bodySizeRepository.GetAll(x => x.IsActive == true);
                if (activeBodySizes != null && activeBodySizes.Any())
                {
                    activeBodySizes = activeBodySizes.ToList();

                    var currentTemplateBodySizes = templateBodySizeRepository.GetAll(x => x.ProductTemplateId == templateId);
                    if (currentTemplateBodySizes != null && currentTemplateBodySizes.Any())
                    {
                        currentTemplateBodySizes = currentTemplateBodySizes.ToList();

                        var addNewBodySizes = new List<TemplateBodySize>();
                        var updateOldBodySizes = new List<TemplateBodySize>();

                        var tasks = new List<Task>();

                        foreach (string id in ids)
                        {
                            tasks.Add(Task.Run(() =>
                            {
                                if (currentTemplateBodySizes.Select(c => c.Id).Contains(id)) // check body size already in template
                                {
                                    if (activeBodySizes.Select(c => c.Id).Contains(id)) // check id exist or active
                                    {
                                        var currentTemplateBodySize = currentTemplateBodySizes.Single(x => x.BodySizeId == id);
                                        if (currentTemplateBodySize.IsActive == false)
                                        {
                                            currentTemplateBodySize.InactiveTime = null;
                                            currentTemplateBodySize.IsActive = true;
                                        }
                                        updateOldBodySizes.Add(currentTemplateBodySize);
                                    }
                                    else
                                    {
                                        throw new UserException("Số đo không tồn tại trong hệ thống");
                                    }
                                }
                                else
                                {
                                    if (activeBodySizes.Select(c => c.Id).Contains(id)) // check id exist or active
                                    {
                                        addNewBodySizes.Add(new TemplateBodySize()
                                        {
                                            Id = Ultils.GenGuidString(),
                                            BodySizeId = id,
                                            InactiveTime = null,
                                            IsActive = true,
                                            ProductTemplateId = templateId
                                        });
                                    }
                                    else
                                    {
                                        throw new UserException("Số đo không tồn tại trong hệ thống");
                                    }
                                }
                            }));
                        }

                        await Task.WhenAll(tasks);

                        foreach (var id in currentTemplateBodySizes.Select(c => c.Id))
                        {
                            tasks.Add(Task.Run(() =>
                            {
                                if (!ids.Contains(id))
                                {
                                    var currentTemplateBodySize = currentTemplateBodySizes.Single(x => x.BodySizeId == id);
                                    if (currentTemplateBodySize.IsActive == true)
                                    {
                                        currentTemplateBodySize.InactiveTime = DateTime.Now;
                                        currentTemplateBodySize.IsActive = false;
                                        updateOldBodySizes.Add(currentTemplateBodySize);
                                    }
                                }
                            }));
                        }

                        await Task.WhenAll(tasks);

                        if (await templateBodySizeRepository.CreateRangeAsync(addNewBodySizes))
                        {
                            if (await templateBodySizeRepository.UpdateRangeAsync(updateOldBodySizes))
                            {
                                return true;
                            }
                            else
                            {
                                throw new UserException("Đã xảy ra lỗi trong quá trình cập nhật các số đo");
                            }
                        }
                        else
                        {
                            throw new UserException("Đã xảy ra lỗi trong quá trình thêm mới các số đo");
                        }
                    }
                }
                return false;
            }
        }

        public bool DeleteTemplateBodySize(string id)
        {
            var templateBodySize = templateBodySizeRepository.Get(id);
            if (templateBodySize == null || templateBodySize.IsActive == false)
            {
                throw new UserException("Số đo cần thiết của mẫu không tồn tại");
            }
            else
            {
                templateBodySize.IsActive = false;

                return templateBodySizeRepository.Update(id, templateBodySize);
            }
        }

        public IEnumerable<BodySize> GetTemplateBodySize(string templateId)
        {
            var template = productTemplateRepository.Get(templateId);
            if (template != null)
            {
                var templateBodySizes = templateBodySizeRepository.GetAll(x => x.ProductTemplateId == templateId && x.IsActive == true);
                if (templateBodySizes != null && templateBodySizes.Any())
                {
                    templateBodySizes = templateBodySizes.ToList();

                    return bodySizeRepository.GetAll(x => templateBodySizes.Select(c => c.BodySizeId).Contains(x.Id));
                }
                else
                {
                    return null;
                }
            }
            else
            {
                throw new UserException("Không tìm thấy mẫu sản phẩm");
            }
        }
    }
}
