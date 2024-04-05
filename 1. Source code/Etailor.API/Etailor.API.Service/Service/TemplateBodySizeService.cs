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
                                        if (!currentTemplateBodySizes.Select(x => x.BodySizeId).Contains(id))
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
                                            var currentTemplateBodySize = currentTemplateBodySizes.Where(x => x.BodySizeId == id);
                                            if (currentTemplateBodySize != null && currentTemplateBodySize.Any())
                                            {
                                                if (currentTemplateBodySize.Where(x => x.IsActive == true).Count() >= 1)
                                                {
                                                    for (int i = 0; i < currentTemplateBodySize.Count(); i++)
                                                    {
                                                        if (i == 0)
                                                        {
                                                            currentTemplateBodySize.ElementAt(i).IsActive = true;
                                                            currentTemplateBodySize.ElementAt(i).InactiveTime = null;
                                                            listUpdateBodySizeTemplates.Add(currentTemplateBodySize.ElementAt(i));
                                                        }
                                                        else
                                                        {
                                                            if (currentTemplateBodySize.ElementAt(i).IsActive == true)
                                                            {
                                                                currentTemplateBodySize.ElementAt(i).IsActive = false;
                                                                currentTemplateBodySize.ElementAt(i).InactiveTime = DateTime.UtcNow.AddHours(7);
                                                                listUpdateBodySizeTemplates.Add(currentTemplateBodySize.ElementAt(i));
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    currentTemplateBodySize.ElementAt(0).IsActive = true;
                                                    currentTemplateBodySize.ElementAt(0).InactiveTime = null;
                                                    listUpdateBodySizeTemplates.Add(currentTemplateBodySize.ElementAt(0));
                                                }
                                            }
                                            else
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
                                tasks.Add(Task.Run(() =>
                                {
                                    if (!ids.Contains(current.BodySizeId))
                                    {
                                        current.IsActive = false;
                                        current.InactiveTime = DateTime.UtcNow.AddHours(7);
                                        listUpdateBodySizeTemplates.Add(current);
                                    }
                                }));
                            }
                        }

                        await Task.WhenAll(tasks);

                        if (templateBodySizeRepository.CreateRange(listBodySizeTemplates))
                        {
                            if (templateBodySizeRepository.UpdateRange(listUpdateBodySizeTemplates))
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
                else
                {
                    throw new UserException("Không tìm thấy số đo nào trong hệ thống");
                }
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
                                        if (!currentTemplateBodySizes.Select(x => x.BodySizeId).Contains(id))
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
                                            var currentTemplateBodySize = currentTemplateBodySizes.Where(x => x.BodySizeId == id);
                                            if (currentTemplateBodySize != null && currentTemplateBodySize.Any())
                                            {
                                                if (currentTemplateBodySize.Where(x => x.IsActive == true).Count() >= 1)
                                                {
                                                    for (int i = 0; i < currentTemplateBodySize.Count(); i++)
                                                    {
                                                        if (i == 0)
                                                        {
                                                            currentTemplateBodySize.ElementAt(i).IsActive = true;
                                                            currentTemplateBodySize.ElementAt(i).InactiveTime = null;
                                                            listUpdateBodySizeTemplates.Add(currentTemplateBodySize.ElementAt(i));
                                                        }
                                                        else
                                                        {
                                                            if (currentTemplateBodySize.ElementAt(i).IsActive == true)
                                                            {
                                                                currentTemplateBodySize.ElementAt(i).IsActive = false;
                                                                currentTemplateBodySize.ElementAt(i).InactiveTime = DateTime.UtcNow.AddHours(7);
                                                                listUpdateBodySizeTemplates.Add(currentTemplateBodySize.ElementAt(i));
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    currentTemplateBodySize.ElementAt(0).IsActive = true;
                                                    currentTemplateBodySize.ElementAt(0).InactiveTime = null;
                                                    listUpdateBodySizeTemplates.Add(currentTemplateBodySize.ElementAt(0));
                                                }
                                            }
                                            else
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
                                tasks.Add(Task.Run(() =>
                                {
                                    if (!ids.Contains(current.BodySizeId))
                                    {
                                        current.IsActive = false;
                                        current.InactiveTime = DateTime.UtcNow.AddHours(7);
                                        listUpdateBodySizeTemplates.Add(current);
                                    }
                                }));
                            }
                        }

                        await Task.WhenAll(tasks);

                        if (templateBodySizeRepository.CreateRange(listBodySizeTemplates))
                        {
                            if (templateBodySizeRepository.UpdateRange(listUpdateBodySizeTemplates))
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
                else
                {
                    throw new UserException("Không tìm thấy số đo nào trong hệ thống");
                }
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
