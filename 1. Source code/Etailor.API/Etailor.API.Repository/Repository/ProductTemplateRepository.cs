using Etailor.API.Repository.DataAccess;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Repository.Repository
{
    public class ProductTemplateRepository : GenericRepository<ProductTemplate>, IProductTemplateRepository
    {
        public ProductTemplateRepository(ETailor_DBContext dBContext) : base(dBContext)
        {
        }
    }
}
