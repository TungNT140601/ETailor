using Etailor.API.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class MaterialCategoryService
    {
        private readonly IMaterialCategoryRepository materialCategoryRepository;
        private readonly IMaterialTypeRepository materialTypeRepository;
        private readonly IMaterialRepository materialRepository;

        public MaterialCategoryService(IMaterialRepository materialRepository, IMaterialTypeRepository materialTypeRepository, IMaterialCategoryRepository materialCategoryRepository)
        {
            this.materialRepository = materialRepository;
            this.materialTypeRepository = materialTypeRepository;
            this.materialCategoryRepository = materialCategoryRepository;
        }

        //public Task<bool> CreateMaterial
    }
}
