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
                foreach (var templateStage in templateStages)
                {
                    var createTask = new List<Task>();
                    if (string.IsNullOrWhiteSpace(templateStage.Name))
                    {
                        throw new UserException("Tên giai đoạn không được để trống");
                    }
                    else
                    {
                        if (existStageNums.Select(c => c.StageNum).Contains(templateStage.StageNum))
                        {
                            var existStage = templateStateRepository.Get(existStageNums.Where(c => c.StageNum == templateStage.StageNum).Select(c => c.Id).FirstOrDefault());
                            if (existStage != null)
                            {
                                existStage.IsActive = false;
                                existStage.InactiveTime = DateTime.Now;
                                templateStateRepository.Update(existStage.Id, existStage);
                            }
                        }

                        createTask.Add(Task.Run(() =>
                        {
                            templateStage.Id = Ultils.GenGuidString();
                        }));
                        createTask.Add(Task.Run(() =>
                        {
                            templateStage.IsActive = true;
                        }));
                        createTask.Add(Task.Run(() =>
                        {
                            templateStage.CreatedTime = DateTime.Now;
                        }));
                        createTask.Add(Task.Run(() =>
                        {
                            templateStage.LastestUpdatedTime = DateTime.Now;
                        }));
                        createTask.Add(Task.Run(() =>
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
                                createTask.Add(Task.Run(() =>
                                {
                                    componentTypeStage.Id = Ultils.GenGuidString();
                                    componentTypeStage.TemplateStageId = templateStage.Id;
                                }));
                            }
                        }

                        await Task.WhenAll(createTask);

                        result = templateStateRepository.Create(templateStage);
                    }
                }
                await Task.WhenAll(tasks);

                return result;
            }
        }

        //public Task<bool> UpdateTemplateStage(string templateId, List<TemplateStage> templateStages)
        //{
        //    var tasks = new List<Task<bool>>();
        //    int stageNum = 1;
        //    var template = productTemplateRepository.Get(templateId);
        //    if (template == null || template.IsActive == true)
        //    {
        //        throw new UserException("Mẫu sản phẩm không tìm thấy");
        //    }
        //    else
        //    {
        //        var existStages = templateStateRepository.GetAll(x => x.ProductTemplateId == templateId).ToList();
        //        if (existStages is null)
        //        {
        //            existStages = new List<TemplateStage>();
        //            for (int i = 0; i < templateStages.Count; i++)
        //            {
        //                existStages.Add(new TemplateStage());
        //            }
        //        }
        //        else if (existStages.Any() && existStages.Count < templateStages.Count)
        //        {
        //            var indexDiff = templateStages.Count - existStages.Count;

        //            for (int i = 0; i < indexDiff; i++)
        //            {
        //                existStages.Add(new TemplateStage());
        //            }
        //        }

        //        for (int i = 0; i < existStages.Count; i++)
        //        {
        //            if (i < templateStages.Count)
        //            {
        //                if ((existStages[i].Name != templateStages[i].Name) || )
        //                {

        //                }
        //            }
        //            else
        //            {

        //            }
        //        }
        //    }
        //}
    }
}
