using Etailor.API.Repository.DataAccess;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Ultity.CustomException;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Repository.Repository
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(ETailor_DBContext dBContext) : base(dBContext)
        {
        }

        public async Task UpdateRangeProduct(List<Product> products)
        {
            try
            {
                foreach (var product in products)
                {
                    var attach = dBContext.Attach(product);
                    attach.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                    dbSet.Update(product);
                }

                dBContext.SaveChanges();
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message, nameof(ProductRepository));
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(ProductRepository));
            }
        }
    }
}
