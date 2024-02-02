using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
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

        public async Task<string> AddComponent(Component component, IFormFile? image, string wwwroot)
        {
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
                var componentType = componentTypeRepository.Get(component.ComponentTypeId);
                if (componentType == null || componentType.IsActive != true)
                {
                    throw new UserException("Kiểu bộ phận không tồn tại");
                }

                if (string.IsNullOrEmpty(component.ProductTemplateId))
                {
                    throw new UserException("Mẫu sản phẩm không tồn tại");
                }
                var template = productTemplateRepository.Get(component.ProductTemplateId);
                if (template == null)
                {
                    throw new UserException("Mẫu sản phẩm không tồn tại");
                }

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
                }
            }));

            tasks.Add(Task.Run(async () =>
            {
                if (image != null)
                {
                    component.Image = await Ultils.UploadImage(wwwroot, "ComponentType", image, null);
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                component.CreatedTime = DateTime.Now;
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
                    dbComponent.InactiveTime = DateTime.Now;
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
                    dbComponent.CreatedTime = DateTime.Now;
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
                dbComponent.InactiveTime = DateTime.Now;
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
            if (components.Any())
            {
                var tasks = new List<Task>();
                foreach (var component in components)
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
    }
}
