using Etailor.API.Repository.DataAccess;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ETailor_DBContext dBContext;
        private ITemplateStateRepository templateStateRepository;
        private IProductTemplateRepository productTemplateRepository;
        private IComponentTypeRepository componentTypeRepository;
        private IComponentStageRepository componentStageRepository;

        public UnitOfWork()
        {
            this.dBContext = new ETailor_DBContext();
        }

        public ITemplateStateRepository TemplateStateRepository
        {
            get
            {
                if (this.templateStateRepository == null)
                {
                    this.templateStateRepository = new TemplateStageRepository(dBContext);
                }
                return this.templateStateRepository;
            }
        }

        public IProductTemplateRepository ProductTemplateRepository
        {
            get
            {
                if (this.productTemplateRepository == null)
                {
                    this.productTemplateRepository = new ProductTemplateRepository(dBContext);
                }
                return this.productTemplateRepository;
            }
        }

        public IComponentTypeRepository ComponentTypeRepository
        {
            get
            {
                if (this.componentTypeRepository == null)
                {
                    this.componentTypeRepository = new ComponentTypeRepository(dBContext);
                }
                return this.componentTypeRepository;
            }
        }

        public IComponentStageRepository ComponentStageRepository
        {
            get
            {
                if (this.componentStageRepository == null)
                {
                    this.componentStageRepository = new ComponentStageRepository(dBContext);
                }
                return this.componentStageRepository;
            }
        }
    }
}
