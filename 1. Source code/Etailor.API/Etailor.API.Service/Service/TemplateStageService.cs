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

        public List<TemplateStage> GetAll(string templateId, string? search)
        {
            var template = productTemplateRepository.Get(templateId);
            if (template == null || template.IsActive != true)
            {
                throw new UserException("Mẫu sản phẩm không tìm thấy");
            }
            else
            {
                var stages = templateStateRepository.GetAll(x => x.ProductTemplateId == templateId && (search == null || (search != null && x.Name.ToLower().Trim().Contains(search.Trim().ToLower()))) && x.IsActive == true).OrderBy(x => x.StageNum).ToList();
                if (stages != null && stages.Count > 0)
                {
                    var stageIds = stages.Select(x => x.Id);
                    var componentStages = componentStageRepository.GetAll(x => stageIds.Contains(x.TemplateStageId)).ToList();
                    var componentTypeGroups = componentStages.GroupBy(c => c.ComponentTypeId).ToList();
                    var componentTypeIds = componentTypeGroups.Select(c => c.Key).ToList();
                    var componentTypes = componentTypeRepository.GetAll(x => componentTypeIds.Contains(x.Id)).ToList();
                }

                return stages;
            }
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

                if (templateStages != null && templateStages.Count > 0 && templateStages.Any(c => c.ComponentStages.Any(x => x.ComponentTypeId != null && !activeComponentTypeIds.Contains(x.ComponentTypeId))))
                {

                }

                var existStageNums = templateStateRepository.GetAll(x => x.ProductTemplateId == template.Id && x.IsActive == true).Select(c => new
                {
                    c.Id,
                    c.StageNum
                }).ToList();

                if (existStageNums != null && existStageNums.Count > 0)
                {
                    var stageIds = existStageNums.Select(x => x.Id);
                    var componentStages = componentStageRepository.GetAll(x => stageIds.Contains(x.TemplateStageId)).ToList();
                }
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
        public async Task<bool> UpdateTemplateStages(string templateId, List<TemplateStage> inputStages)
        {

            var tasks = new List<Task>();
            var template = productTemplateRepository.Get(templateId);
            if (template == null || template.IsActive != true)
            {
                throw new UserException("Mẫu sản phẩm không tìm thấy");
            }
            else
            {
                #region GetDataFromDB
                var activeComponent = componentTypeRepository.GetAll(x => x.IsActive == true && x.CategoryId == template.CategoryId).Select(c => c.Id).ToList();
                var currentStages = templateStateRepository.GetAll(x => x.ProductTemplateId == templateId && x.IsActive == true).ToList();
                if (currentStages != null && currentStages.Count > 0)
                {
                    var stageIds = currentStages.Select(x => x.Id).ToList();
                    //var componentStages = componentStageRepository.GetAll(x => stageIds.Contains(x.TemplateStageId)).ToList();
                }
                #endregion

                #region InitVariable
                var maxStageNum = 0;
                var updateStages = new List<TemplateStage>();
                var inactiveStages = new List<TemplateStage>();
                var updateComponentStages = new List<ComponentStage>();
                #endregion


                #region CountNewTotalStage
                if (inputStages.Any() && inputStages.Count > 0)
                {
                    if (inputStages.Any(c => c.ComponentStages.Any(x => !activeComponent.Contains(x.ComponentTypeId))))
                    {
                        throw new UserException("Không tìm thấy loại bộ phận");
                    }
                    if (currentStages.Any() && currentStages.Count > 0)
                    {
                        maxStageNum = inputStages.Count >= currentStages.Count ? inputStages.Count : currentStages.Count;
                    }
                    else
                    {
                        maxStageNum = inputStages.Count;
                        currentStages = new List<TemplateStage>();
                    }
                }
                else
                {
                    if (currentStages.Any() && currentStages.Count > 0)
                    {
                        maxStageNum = currentStages.Count;
                    }
                    else
                    {
                        maxStageNum = 0;
                        inputStages = new List<TemplateStage>();
                    }
                }
                #endregion


                #region CheckDiffBetweenNewOldData
                for (int i = 0; i < maxStageNum; i++)
                {
                    var checkDiff = false; // Flag to check if 2 Step is diff

                    var id = Ultils.GenGuidString(); // Template Stage New Id

                    var updateCurrentComponentStages = new List<ComponentStage>();

                    // Select 2 stage has same StageNum from db and update
                    var currentStage = currentStages.Where(x => x.StageNum == i + 1).FirstOrDefault();
                    var inputStage = inputStages.Where(x => x.StageNum == i + 1).FirstOrDefault();

                    // Check diff
                    if (currentStage == null && inputStage != null) // if db dont have stage num
                    {
                        updateStages.Add(inputStage); // add new stage to list for add it
                    }
                    else if (currentStage != null && inputStage == null) // if new dont have stage num
                    {
                        inactiveStages.Add(currentStage); // add old stage to list for remove it
                    }
                    else if (currentStage != null && inputStage != null) // if both old and new have stage num
                    {
                        var currentComponents = currentStage.ComponentStages.ToList();// get list component of old stage
                        var inputComponents = inputStage.ComponentStages.ToList();// get list component of new stage

                        if (inputStage.Name != currentStage.Name)// if it has diff name
                        {
                            checkDiff = true; // flag is true
                            if (currentComponents != null && currentComponents.Count > 0)
                            {
                                updateCurrentComponentStages.AddRange(currentComponents); // add list component to list for add
                            }
                        }
                        else if (currentComponents == null && inputComponents != null) // if new list component not null and old list null
                        {
                            checkDiff = true; // flag is true
                            inputStage = new TemplateStage()
                            {
                                Id = id,
                                Name = currentStage.Name,
                                ProductTemplateId = currentStage.ProductTemplateId,
                                TemplateStageId = currentStage.TemplateStageId,
                                StageNum = currentStage.StageNum
                            }; // duplicate old stage
                            templateStateRepository.Detach(currentStage.Id);
                            if (inputComponents != null && inputComponents.Count > 0)
                            {
                                updateCurrentComponentStages.AddRange(inputComponents);// add list component to list for add
                            }
                        }
                        else if (currentComponents != null && inputComponents == null)// if new list component is null and old list not null
                        {
                            checkDiff = true;
                            inputStage = new TemplateStage()
                            {
                                Id = id,
                                Name = currentStage.Name,
                                ProductTemplateId = currentStage.ProductTemplateId,
                                TemplateStageId = currentStage.TemplateStageId,
                                StageNum = currentStage.StageNum
                            }; // duplicate old stage
                            templateStateRepository.Detach(currentStage.Id);
                        }
                        else if (currentComponents != null && inputComponents != null)// if both list not null
                        {
                            var currentComponentsIds = currentComponents.Select(x => x.ComponentTypeId);
                            var inputComponentsIds = inputComponents.Select(x => x.ComponentTypeId);
                            var idDiffs = currentComponentsIds.Except(inputComponentsIds); // check diff between two lists
                            if (idDiffs.Any()) // if has any diff
                            {
                                checkDiff = true;
                                inputStage = new TemplateStage()
                                {
                                    Id = id,
                                    Name = currentStage.Name,
                                    ProductTemplateId = currentStage.ProductTemplateId,
                                    TemplateStageId = currentStage.TemplateStageId,
                                    StageNum = currentStage.StageNum
                                }; // duplicate old stage
                                templateStateRepository.Detach(currentStage.Id);
                                if (inputComponents != null && inputComponents.Count > 0)
                                {
                                    updateCurrentComponentStages.AddRange(inputComponents);
                                }
                            }
                        }
                    }

                    if (checkDiff)
                    {
                        if (updateCurrentComponentStages != null)
                        {
                            foreach (var updateCurrentComponentStage in updateCurrentComponentStages)
                            {
                                updateCurrentComponentStage.Id = Ultils.GenGuidString();
                                updateCurrentComponentStage.TemplateStageId = id;
                            }
                            updateComponentStages.AddRange(updateCurrentComponentStages);
                        }
                        if (inputStage != null)
                        {
                            updateStages.Add(new TemplateStage()
                            {
                                Id = id,
                                Name = inputStage.Name,
                                ProductTemplateId = inputStage.ProductTemplateId,
                                TemplateStageId = inputStage.TemplateStageId,
                                StageNum = inputStage.StageNum,
                                LastestUpdatedTime = DateTime.Now,
                                CreatedTime = DateTime.Now,
                                InactiveTime = null,
                                IsActive = true,
                                ComponentStages = null,
                                ProductStages = null,
                                ProductTemplate = null,
                                TemplateStageNavigation = null
                            }); // duplicate old stage
                        }
                        if (currentStage != null)
                        {
                            currentStage.InactiveTime = DateTime.Now;
                            currentStage.IsActive = false;

                            inactiveStages.Add(currentStage);
                        }
                    }
                }
                #endregion


                var check = new List<bool>();

                #region ApplyDiffDataToDB
                if (updateStages != null && updateStages.Any())
                {
                    //foreach (var updateStage in updateStages)
                    //{
                    //    updateStage.ComponentStages = null;
                    //    check.Add(templateStateRepository.Create(updateStage));
                    //}
                    check.Add(templateStateRepository.CreateRange(updateStages));
                }
                if (inactiveStages != null && inactiveStages.Any())
                {
                    foreach (var inactiveStage in inactiveStages)
                    {
                        check.Add(templateStateRepository.Update(inactiveStage.Id, inactiveStage));
                    }
                }
                if (updateComponentStages != null && updateComponentStages.Any())
                {
                    check.Add(componentStageRepository.CreateRange(updateComponentStages));
                }
                #endregion

                return !check.Any(c => c == false);
            }
        }
    }
}
