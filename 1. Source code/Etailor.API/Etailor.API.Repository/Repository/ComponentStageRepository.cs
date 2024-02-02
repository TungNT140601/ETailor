using Etailor.API.Repository.DataAccess;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Repository.Repository
{
    public class ComponentStageRepository : GenericRepository<ComponentStage>, IComponentStageRepository
    {
        public ComponentStageRepository(ETailor_DBContext dBContext) : base(dBContext)
        {
        }
    }
}
