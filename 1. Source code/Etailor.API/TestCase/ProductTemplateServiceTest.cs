using Etailor.API.Repository.DataAccess;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.Repository;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using Microsoft.EntityFrameworkCore;

namespace TestCase
{
    public class ProductTemplateServiceTest
    {
        IProductTemplateService productTemplateService;
        
        ETailor_DBContext dBContext = new ETailor_DBContext();

        IProductTemplateRepository productTemplateRepository;
        ICategoryRepository categoryRepository;
        ITemplateBodySizeService templateBodySizeService;
        IComponentRepository componentRepository;
        IComponentTypeRepository componentTypeRepository;
        
        ITemplateBodySizeRepository templateBodySizeRepository;
        IBodySizeRepository bodySizeRepository;

        [OneTimeSetUp]
        public void Setup()
        {

            productTemplateRepository = new ProductTemplateRepository(dBContext);
            categoryRepository = new CategoryRepository(dBContext);
            componentRepository = new ComponentRepository(dBContext);
            componentTypeRepository = new ComponentTypeRepository(dBContext);

            templateBodySizeRepository = new TemplateBodySizeRepository(dBContext);
            bodySizeRepository = new BodySizeRepository(dBContext);
            templateBodySizeService = new TemplateBodySizeService(templateBodySizeRepository, productTemplateRepository, bodySizeRepository);
            productTemplateService = new ProductTemplateService(productTemplateRepository, categoryRepository, templateBodySizeService, componentTypeRepository, componentRepository);
        }


        //GetProductTemplateList By Name
        //IsActive == true is nested in code func GetTemplates of ProductTemplateService
        [Test]
        public async Task ProductTemplateList_WithNullNameData()
        {
            var result = await productTemplateService.GetTemplates(null);

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsNotEmpty(result, "The result list should not be empty.");

            Assert.IsInstanceOf<IEnumerable<ProductTemplate>>(result, "The result should be a list of ProductTemplate.");
        }

        [Test]
        public async Task ProductTemplateList_WithEmptyNameData()
        {
            var result = await productTemplateService.GetTemplates("");
            
            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsNotEmpty(result, "The result list should not be empty.");

            Assert.IsInstanceOf<IEnumerable<ProductTemplate>>(result, "The result should be a list of ProductTemplate.");
        }

        [Test]
        public async Task ProductTemplateList_WithNameData()
        {
            var result = await productTemplateService.GetTemplates("Áo");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsNotEmpty(result, "The result list should not be empty.");

            Assert.IsInstanceOf<IEnumerable<ProductTemplate>>(result, "The result should be a list of ProductTemplate.");
        }

        //GetProductTemplateList By Category Id
        //IsActive == true is nested in code func GetTemplates of ProductTemplateService
        [Test]
        public async Task ProductTemplateList_WithNullCategoryIdData()
        {
            var result = await productTemplateService.GetByCategory(null);

            Assert.IsNull(result, "The result should be null.");
        }

        [Test]
        public async Task ProductTemplateList_WithEmptyCategoryIdData()
        {
            var result = await productTemplateService.GetByCategory("");

            Assert.AreEqual(null, result, "The result should be null");
        }

        [Test]
        public async Task ProductTemplateList_WithCategoryIdData()
        {
            var result = await productTemplateService.GetByCategory("72d50e40-81b7-452d-84fa-298068");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsNotEmpty(result, "The result list should not be empty.");

            Assert.IsInstanceOf<IEnumerable<ProductTemplate>>(result, "The result should be a list of ProductTemplate.");
        }

        //GetProductTemplateList By list of Category Id 
        //IsActive == true is nested in code func GetTemplates of ProductTemplateService
        [Test]
        public async Task ProductTemplateList_WithNullCategoryIdsData()
        {
            List<string> categoryIds = null;

            var result = productTemplateService.GetByCategorys(categoryIds);

            Assert.ThrowsAsync<NullReferenceException>(async () => await result);
        }

        [Test]
        public async Task ProductTemplateList_WithEmptyCategoryIdsData()
        {
            List<string> categoryIds = new List<string>();

            var result = await productTemplateService.GetByCategorys(categoryIds);

            Assert.AreEqual(null, result, "The result should be null");
        }

        [Test]
        public async Task ProductTemplateList_WithCategoryIdsData()
        {
            List<string> categoryIds = new List<string> { "72d50e40-81b7-452d-84fa-298068", "e10cee9f-00aa-4fef-a89e-f6663f" };

            var result = await productTemplateService.GetByCategorys(categoryIds);

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsNotEmpty(result, "The result list should not be empty.");

            Assert.IsInstanceOf<IEnumerable<ProductTemplate>>(result, "The result should be a list of ProductTemplate.");
        }

        //GetProductTemplate By Id 
        //IsActive == true is nested in code func GetTemplates of ProductTemplateService
        [Test]
        public async Task ProductTemplateDetail_WithNullIdData()
        {
            var result = await productTemplateService.GetById(null);

            Assert.IsNull(result, "The result should be null");
        }

        [Test]
        public async Task ProductTemplateDetail_WithEmptyIdData()
        {
            var result = await productTemplateService.GetById("");

            Assert.AreEqual(null, result, "The result should be null");
        }

        [Test]
        public async Task ProductTemplateDetail_WithIdData()
        {
            var result = await productTemplateService.GetById("ec6e1f75-3f05-48d6-b243-bb06f3");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<ProductTemplate>(result, "The result should be as Product Template object.");
        }

        //GetProductTemplate By UrlPath 
        //IsActive == true is nested in code func GetTemplates of ProductTemplateService
        [Test]
        public async Task ProductTemplateDetail_WithNullUrlPathData()
        {
            var result = await productTemplateService.GetByUrlPath(null);

            Assert.IsNull(result, "The result should be null");
        }

        [Test]
        public async Task ProductTemplateDetail_WithEmptyUrlPathData()
        {
            var result = await productTemplateService.GetByUrlPath("");

            Assert.AreEqual(null, result, "The result should be null");
        }

        [Test]
        public async Task ProductTemplateDetail_WithUrlPathData()
        {
            var result = await productTemplateService.GetByUrlPath("ao-đam-co-đan-tong");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<ProductTemplate>(result, "The result should be as Product Template object.");
        }
    }
}