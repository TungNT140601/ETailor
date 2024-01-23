using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class ComponentTypeService : IComponentTypeService
    {
        private readonly ICategoryRepository categoryRepository;
        private readonly IComponentTypeRepository componentTypeRepository;
        private readonly IComponentRepository componentRepository;
        public ComponentTypeService(ICategoryRepository categoryRepository, IComponentTypeRepository componentTypeRepository, IComponentRepository componentRepository)
        {
            this.categoryRepository = categoryRepository;
            this.componentTypeRepository = componentTypeRepository;
            this.componentRepository = componentRepository;
        }
        //public bool AddComponentType(ComponentType componentType)
        //{

        //}
    }
}
