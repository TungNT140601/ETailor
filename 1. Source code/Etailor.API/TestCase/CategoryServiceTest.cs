using Etailor.API.Repository.DataAccess;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.Repository;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCase
{
    public class CategoryServiceTest
    {
        ICategoryService categoryService;
        ETailor_DBContext dBContext = new ETailor_DBContext();

        ICategoryRepository categoryRepository;
        IComponentTypeService componentTypeService;
        IComponentTypeRepository componentTypeRepository;
        IProductTemplateRepository productTemplateRepository;

        IComponentRepository componentRepository;

        [SetUp]
        public void Setup()
        {
            categoryRepository = new CategoryRepository(dBContext);
            componentTypeRepository = new ComponentTypeRepository(dBContext);   
            productTemplateRepository = new ProductTemplateRepository(dBContext);
            componentRepository = new ComponentRepository(dBContext);

            componentTypeService = new ComponentTypeService(categoryRepository, componentTypeRepository, componentRepository);
            categoryService = new CategoryService(categoryRepository, componentTypeRepository, productTemplateRepository, componentTypeService);
        }

        //GetCategoryList By Name
        //IsActive == true is nested in code func GetTemplates of ProductTemplateService
        [Test]
        public async Task CategoryList_WithNullNameData()
        {
            var result = await categoryService.GetCategorys(null);

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsNotEmpty(result, "The result list should not be empty.");

            Assert.IsInstanceOf<IEnumerable<Category>>(result, "The result should be a list of Category.");
        }

        [Test]
        public async Task CategoryList_WithEmptyNameData()
        {
            var result = await categoryService.GetCategorys("");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsNotEmpty(result, "The result list should not be empty.");

            Assert.IsInstanceOf<IEnumerable<Category>>(result, "The result should be a list of Category.");
        }

        [Test]
        public async Task CategoryList_WithNameData()
        {
            var result = await categoryService.GetCategorys("Áo");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsNotEmpty(result, "The result list should not be empty.");

            Assert.IsInstanceOf<IEnumerable<Category>>(result, "The result should be a list of Category.");
        }

        //GetCategory By Id 
        //IsActive == true is nested in code func GetCategory of CategoryService
        [Test]
        public void CategoryDetail_WithNullIdData()
        {
            var result = categoryService.GetCategory(null);

            Assert.IsNull(result, "The result should be null");
        }

        [Test]
        public void CategoryDetail_WithEmptyIdData()
        {
            var result = categoryService.GetCategory("");

            Assert.AreEqual(null, result, "The result should be null");
        }

        [Test]
        public void CategoryDetail_WithIdData()
        {
            var result = categoryService.GetCategory("3225e787-9cb5-494f-bf03-e91c89");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<Category>(result, "The result should be as Category object.");
        }
    }
}
