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
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class ComponentService : IComponentService
    {
        private readonly IProductTemplateRepository productTemplateRepository;
        private readonly IComponentRepository componentRepository;
        private readonly IComponentTypeRepository componentTypeRepository;
        public ComponentService(IProductTemplateRepository productTemplateRepository, IComponentRepository componentRepository, IComponentTypeRepository componentTypeRepository)
        {
            this.productTemplateRepository = productTemplateRepository;
            this.componentRepository = componentRepository;
            this.componentTypeRepository = componentTypeRepository;
        }

        public async Task<Component> GetComponent(string id)
        {
            var component = componentRepository.Get(id);

            var setImage = Task.Run(async () =>
            {
                if (string.IsNullOrEmpty(component.Image))
                {
                    component.Image = "https://firebasestorage.googleapis.com/v0/b/etailor-21a50.appspot.com/o/Uploads%2FThumbnail%2Fstill-life-spring-wardrobe-switch.jpg?alt=media&token=7dc9a197-1b76-4525-8dc7-caa2238d8327";
                }
                else
                {
                    component.Image = await Ultils.GetUrlImage(component.Image);
                }
            });
            await Task.WhenAll(setImage);

            return component == null ? null : component.IsActive == true ? component : null;
        }

        public async Task<IEnumerable<Component>> GetComponents()
        {
            IEnumerable<Component> ListOfComponents = componentRepository.GetAll(x => x.IsActive == true);
            foreach (var component in ListOfComponents)
            {
                var setImage = Task.Run(async () =>
                {
                    if (string.IsNullOrEmpty(component.Image))
                    {
                        component.Image = "https://firebasestorage.googleapis.com/v0/b/etailor-21a50.appspot.com/o/Uploads%2FThumbnail%2Fstill-life-spring-wardrobe-switch.jpg?alt=media&token=7dc9a197-1b76-4525-8dc7-caa2238d8327";
                    }
                    else
                    {
                        component.Image = await Ultils.GetUrlImage(component.Image);
                    }
                });
                await Task.WhenAll(setImage);
            };
            return ListOfComponents;
        }

        public async Task<string> AddComponent(Component component, IFormFile? image, string wwwroot)
        {
            var componentType = componentTypeRepository.Get(component.ComponentTypeId);
            var template = productTemplateRepository.Get(component.ProductTemplateId);
            var componentNames = componentRepository.GetAll(x => x.ProductTemplateId == component.ProductTemplateId && x.Name == component.Name && x.ComponentTypeId == component.ComponentTypeId && x.IsActive == true).ToList();
            var templateComponents = componentRepository.GetAll(x => x.ProductTemplateId == component.ProductTemplateId && x.ComponentTypeId == component.ComponentTypeId && x.IsActive == true).ToList();

            var changeDefaultComponent = new Component();

            var tasks = new List<Task>();

            tasks.Add(Task.Run(() =>
            {
                component.Id = Ultils.GenGuidString();
            }));

            tasks.Add(Task.Run(() =>
            {
                if (string.IsNullOrEmpty(component.ComponentTypeId))
                {
                    throw new UserException("Kiểu bộ phận không tồn tại");
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                if (componentType == null || componentType.IsActive != true)
                {
                    throw new UserException("Kiểu bộ phận không tồn tại");
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                if (string.IsNullOrEmpty(component.ProductTemplateId))
                {
                    throw new UserException("Mẫu sản phẩm không tồn tại");
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                if (template == null)
                {
                    throw new UserException("Mẫu sản phẩm không tồn tại");
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(component.Name))
                {
                    throw new UserException("Tên bộ phận không được để trống");
                }
                else
                {
                    if (componentNames.Any())
                    {
                        throw new UserException($"Tên bộ phận đã được sử dụng");
                    }
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                if (component.Default.HasValue)
                {
                    if (component.Default.Value)
                    {
                        if (templateComponents.Any(c => c.Default == true))
                        {
                            changeDefaultComponent = templateComponents.Single(x => x.Default.Value == true);
                            changeDefaultComponent.Default = false;
                        }
                    }
                }
                else
                {
                    if (templateComponents.Any(c => c.Default == true))
                    {
                        component.Default = false;
                    }
                    else
                    {
                        component.Default = true;
                    }
                }
            }));

            tasks.Add(Task.Run(async () =>
            {
                if (image != null)
                {
                    component.Image = await Ultils.UploadImage(wwwroot, "Component", image, null);
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                component.CreatedTime = DateTime.UtcNow.AddHours(7);
                component.InactiveTime = null;
                component.IsActive = true;
            }));

            await Task.WhenAll(tasks);

            return componentRepository.Create(component) ? component.Id : null;
        }

        public async Task<string> UpdateComponent(Component component, IFormFile? newImage, string wwwroot)
        {
            var dbComponent = componentRepository.Get(component.Id);

            if (dbComponent != null)
            {
                var tasks = new List<Task>();

                tasks.Add(Task.Run(() =>
                {
                    dbComponent.IsActive = false;
                    dbComponent.InactiveTime = DateTime.UtcNow.AddHours(7);
                    componentRepository.Update(dbComponent.Id, dbComponent);
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (string.IsNullOrWhiteSpace(component.Name))
                    {
                        throw new UserException("Tên bộ phận không được để trống");
                    }
                    else
                    {
                        if (componentRepository.GetAll(x => x.Name == component.Name && x.ComponentTypeId == component.ComponentTypeId && x.IsActive == true).Any())
                        {
                            throw new UserException($"Tên bộ phận đã được sử dụng");
                        }
                        else
                        {
                            dbComponent.Name = component.Name;
                        }
                    }
                }));

                tasks.Add(Task.Run(async () =>
                {
                    if (newImage != null)
                    {
                        dbComponent.Image = await Ultils.UploadImage(wwwroot, "ComponentType", newImage, null);
                    }
                }));

                await Task.WhenAll(tasks);

                tasks.Add(Task.Run(() =>
                {
                    dbComponent.Id = Ultils.GenGuidString();
                }));

                tasks.Add(Task.Run(() =>
                {
                    dbComponent.CreatedTime = DateTime.UtcNow.AddHours(7);
                }));

                tasks.Add(Task.Run(() =>
                {
                    dbComponent.InactiveTime = null;
                }));

                tasks.Add(Task.Run(() =>
                {
                    dbComponent.IsActive = true;
                }));

                await Task.WhenAll(tasks);

                return componentRepository.Create(dbComponent) ? dbComponent.Id : null;
            }
            else
            {
                throw new UserException("Bộ phận không tồn tại");
            }
        }

        public bool DeleteComponent(string id)
        {
            var dbComponent = componentRepository.Get(id);

            if (dbComponent != null)
            {
                dbComponent.InactiveTime = DateTime.UtcNow.AddHours(7);
                dbComponent.IsActive = false;

                return componentRepository.Update(dbComponent.Id, dbComponent);
            }
            else
            {
                throw new UserException("Bộ phận không tồn tại");
            }
        }

        public async Task<IEnumerable<Component>> GetAllByComponentType(string componentTypeId, string templateId)
        {
            var components = componentRepository.GetAll(x => x.ComponentTypeId == componentTypeId && x.ProductTemplateId == templateId && x.IsActive == true);
            if (components != null && components.Any())
            {
                var tasks = new List<Task>();
                foreach (var component in components.ToList())
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        component.Image = await Ultils.GetUrlImage(component.Image);
                    }));
                }
                await Task.WhenAll(tasks);
            }
            return components;
        }

        public async Task<bool> CheckDefaultComponent(string templateId)
        {
            var tasks = new List<Task>();
            var template = productTemplateRepository.Get(templateId);

            if (template == null)
            {
                throw new UserException("Mẫu sản phẩm không tồn tại");
            }

            var templateComponents = componentRepository.GetAll(x => x.ProductTemplateId == templateId && x.IsActive == true).ToList();

            var componentTypesOfTemplates = componentTypeRepository.GetAll(x => x.CategoryId == template.CategoryId && x.IsActive == true).ToList();

            foreach (var componentTypesOfTemplate in componentTypesOfTemplates)
            {
                tasks.Add(Task.Run(() =>
                {
                    if (!templateComponents.Any(x => x.IsActive == true && x.Default == true))
                    {
                        throw new UserException("Vui lòng chọn kiểu mặc định cho mỗi bộ phận của bản mẫu");
                    }
                }));
            }

            await Task.WhenAll(tasks);

            return true;
        }
    }
}
