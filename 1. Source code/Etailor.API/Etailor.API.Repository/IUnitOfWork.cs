using Etailor.API.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Repository
{
    public interface IUnitOfWork
    {
        public ITemplateStateRepository TemplateStateRepository { get; }
        public IProductTemplateRepository ProductTemplateRepository { get; }
        public IComponentTypeRepository ComponentTypeRepository { get; }
        public IComponentStageRepository ComponentStageRepository { get; }
    }
}
