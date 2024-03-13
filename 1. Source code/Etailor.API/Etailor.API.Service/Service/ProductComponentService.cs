using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.Repository;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class ProductComponentService : IProductComponentService
    {
        private readonly IProductComponentRepository productComponentRepository;
        private readonly IComponentRepository componentRepository;
        private readonly IProductStageRepository productStageRepository;

        public ProductComponentService(IProductComponentRepository productComponentRepository,
            IComponentRepository componentRepository, IProductStageRepository productStageRepository)
        {
            this.productComponentRepository = productComponentRepository;
            this.componentRepository = componentRepository;
            this.productStageRepository = productStageRepository;
        }

        public async Task<ProductComponent> GetProductComponent(string id)
        {
            var productComponent = productComponentRepository.Get(id);

            var setImage= Task.Run(async () =>
            {
                if (string.IsNullOrEmpty(productComponent.Image))
                {
                    productComponent.Image = "https://firebasestorage.googleapis.com/v0/b/etailor-21a50.appspot.com/o/Uploads%2FThumbnail%2Fstill-life-spring-wardrobe-switch.jpg?alt=media&token=7dc9a197-1b76-4525-8dc7-caa2238d8327";
                }
                else
                {
                    productComponent.Image = await Ultils.GetUrlImage(productComponent.Image);
                }
            });
            await Task.WhenAll(setImage);

            return productComponent == null ? null :  productComponent;
        }


        public async Task<IEnumerable<ProductComponent>> GetProductComponents(string productStageId)
        {
            IEnumerable<ProductComponent> ListOfProductComponent = productComponentRepository.GetAll(x => x.ProductStageId == productStageId);
            foreach (ProductComponent productComponent in ListOfProductComponent)
            {
                var setImage = Task.Run(async () =>
                {
                    if (string.IsNullOrEmpty(productComponent.Image))
                    {
                        productComponent.Image = "https://firebasestorage.googleapis.com/v0/b/etailor-21a50.appspot.com/o/Uploads%2FThumbnail%2Fstill-life-spring-wardrobe-switch.jpg?alt=media&token=7dc9a197-1b76-4525-8dc7-caa2238d8327";
                    }
                    else
                    {
                        productComponent.Image = await Ultils.GetUrlImage(productComponent.Image);
                    }
                });
                await Task.WhenAll(setImage);
            };
            return ListOfProductComponent;
        }
    }
}
