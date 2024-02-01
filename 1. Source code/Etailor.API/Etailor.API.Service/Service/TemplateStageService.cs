using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
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
    public class TemplateStageService : ITemplateStageService
    {
        private readonly ITemplateStateRepository templateStateRepository;
        private readonly IProductTemplateRepository productTemplateRepository;
        private readonly IComponentTypeRepository componentTypeRepository;
        private readonly IComponentStageRepository componentStageRepository;
        public TemplateStageService(ITemplateStateRepository templateStateRepository, IProductTemplateRepository productTemplateRepository, IComponentTypeRepository componentTypeRepository, IComponentStageRepository componentStageRepository)
        {
            this.templateStateRepository = templateStateRepository;
            this.productTemplateRepository = productTemplateRepository;
            this.componentTypeRepository = componentTypeRepository;
            this.componentStageRepository = componentStageRepository;
        }

        public async Task<bool> CreateTemplateStages(string templateId, List<TemplateStage> templateStages)
        {
            var tasks = new List<Task>();
            var template = productTemplateRepository.Get(templateId);
            if (template == null || template.IsActive != false)
            {
                throw new UserException("Mẫu sản phẩm không tìm thấy");
            }
            else
            {
                var result = false;

                var activeComponentTypeIds = componentTypeRepository.GetAll(x => x.IsActive == true && x.CategoryId == template.CategoryId).Select(c => c.Id).ToList();

                var existStageNums = templateStateRepository.GetAll(x => x.ProductTemplateId == template.Id && x.IsActive == true).Select(c => new
                {
                    c.Id,
                    c.StageNum
                });
                var existStageNumIds = new List<string>();
                var componentOfStages = new List<ComponentStage>();
                foreach (var templateStage in templateStages)
                {
                    if (string.IsNullOrWhiteSpace(templateStage.Name))
                    {
                        throw new UserException("Tên giai đoạn không được để trống");
                    }
                    else
                    {
                        if (existStageNums.Select(c => c.StageNum).Contains(templateStage.StageNum))
                        {
                            existStageNumIds.Add(existStageNums.Where(c => c.StageNum == templateStage.StageNum).Select(c => c.Id).FirstOrDefault());
                        }

                        tasks.Add(Task.Run(() =>
                        {
                            templateStage.Id = Ultils.GenGuidString();
                        }));
                        tasks.Add(Task.Run(() =>
                        {
                            templateStage.IsActive = true;
                        }));
                        tasks.Add(Task.Run(() =>
                        {
                            templateStage.CreatedTime = DateTime.Now;
                        }));
                        tasks.Add(Task.Run(() =>
                        {
                            templateStage.LastestUpdatedTime = DateTime.Now;
                        }));
                        tasks.Add(Task.Run(() =>
                        {
                            templateStage.InactiveTime = null;
                        }));

                        foreach (var componentTypeStage in templateStage.ComponentStages)
                        {
                            if (!activeComponentTypeIds.Contains(componentTypeStage.ComponentTypeId))
                            {
                                throw new UserException("Không tìm thấy loại bộ phận");
                            }
                            else
                            {
                                tasks.Add(Task.Run(() =>
                                {
                                    componentOfStages.Add(new ComponentStage()
                                    {
                                        ComponentTypeId = componentTypeStage.ComponentTypeId,
                                        TemplateStageId = templateStage.Id,
                                        Id = Ultils.GenGuidString()
                                    });
                                }));
                            }
                        }
                        templateStage.ComponentStages = null;
                    }
                }
                await Task.WhenAll(tasks);

                if (templateStateRepository.CreateRange(templateStages) && componentStageRepository.CreateRange(componentOfStages))
                {
                    foreach (var id in existStageNumIds)
                    {
                        var existStage = templateStateRepository.Get(id);
                        if (existStage != null)
                        {
                            existStage.IsActive = false;
                            existStage.InactiveTime = DateTime.Now;

                            templateStateRepository.Update(id, existStage);
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

    }
}
