using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
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
                var activeBodySizeIds = bodySizeRepository.GetAll(x => x.IsActive == true).Select(x => x.Id).ToList();
                var listBodySizeTemplates = new List<TemplateBodySize>();
                var tasks = new List<Task>();
                foreach (string id in ids)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        if (activeBodySizeIds.Contains(id))
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

                await Task.WhenAll(tasks);

                return templateBodySizeRepository.CreateRange(listBodySizeTemplates);
            }
        }

        public async Task<string> UpdateDraftTemplateBodySize(string wwwroot, List<string> ids, string templateId)
        {
            var template = productTemplateRepository.Get(templateId);
            if (template == null || template.IsActive != false)
            {
                throw new UserException("Mẫu sản phẩm không tìm thấy");
            }
            else
            {
                var fileDraft = Path.Combine(wwwroot, "UpdateDraft", $"{templateId}.json");

                var draft = new ProductTemplate();

                if (File.Exists(fileDraft))
                {
                    string jsonString = File.ReadAllText(fileDraft);

                    draft = JsonSerializer.Deserialize<ProductTemplate>(jsonString);

                    var activeBodySizeIds = bodySizeRepository.GetAll(x => x.IsActive == true).Select(x => x.Id).ToList();

                    var currentTemplateBodySizes = templateBodySizeRepository.GetAll(x => x.ProductTemplateId == templateId).ToList();
                    var currentTemplateBodySizeIds = currentTemplateBodySizes.Select(x => x.BodySizeId).ToList();

                    draft.TemplateBodySizes = new List<TemplateBodySize>();

                    var tasks = new List<Task>();

                    foreach (string id in ids)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            if (currentTemplateBodySizeIds.Contains(id)) // check body size already in template
                            {
                                if (activeBodySizeIds.Contains(id)) // check id exist or active
                                {
                                    var currentTemplateBodySize = currentTemplateBodySizes.Single(x => x.BodySizeId == id);
                                    if (currentTemplateBodySize.IsActive == false)
                                    {
                                        currentTemplateBodySize.InactiveTime = null;
                                        currentTemplateBodySize.IsActive = true;
                                        draft.TemplateBodySizes.Add(currentTemplateBodySize);
                                    }
                                }
                                else
                                {
                                    throw new UserException("Số đo không tồn tại trong hệ thống");
                                }
                            }
                            else
                            {
                                if (activeBodySizeIds.Contains(id)) // check id exist or active
                                {
                                    draft.TemplateBodySizes.Add(new TemplateBodySize()
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

                    foreach (var id in currentTemplateBodySizeIds)
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
                                    draft.TemplateBodySizes.Add(currentTemplateBodySize);
                                }
                            }
                        }));
                    }

                    await Task.WhenAll(tasks);

                    var options = new JsonSerializerOptions
                    {
                        ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    };

                    var saveDraftString = JsonSerializer.Serialize(draft, options);

                    File.WriteAllText(fileDraft, saveDraftString);

                    return templateId;
                }
                else
                {
                    throw new UserException("Mẫu sản phẩm không tìm thấy");
                }
            }
        }
    }
}
