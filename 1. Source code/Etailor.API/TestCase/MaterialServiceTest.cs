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
    public class MaterialServiceTest
    {
        IMaterialService materialService;
        ETailor_DBContext dBContext = new ETailor_DBContext();

        IMaterialRepository materialRepository;
        IMaterialCategoryRepository materialCategoryRepository;
        IMaterialTypeRepository materialTypeRepository;
        IOrderMaterialRepository orderMaterialRepository;
        IOrderRepository orderRepository;
        IProductRepository productRepository;

        [SetUp]
        public void Setup()
        {
            materialRepository = new MaterialRepository(dBContext);
            materialCategoryRepository = new MaterialCategoryRepository(dBContext);
            materialTypeRepository = new MaterialTypeRepository(dBContext);
            orderMaterialRepository = new OrderMaterialRepository(dBContext);
            orderRepository = new OrderRepository(dBContext);
            productRepository = new ProductRepository(dBContext);

            materialService = new MaterialService(materialRepository, materialCategoryRepository, materialTypeRepository
                , orderMaterialRepository, orderRepository, productRepository);
        }

        //GetMaterialList By Name
        //IsActive == true is nested in code func GetMaterials of MaterialService
        [Test]
        public void MaterialList_WithNullNameData()
        {
            var result = materialService.GetMaterials(null);

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsNotEmpty(result, "The result list should not be empty.");

            Assert.IsInstanceOf<IEnumerable<Material>>(result, "The result should be a list of Material.");
        }

        [Test]
        public void MaterialList_WithEmptyNameData()
        {
            var result = materialService.GetMaterials("");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsNotEmpty(result, "The result list should not be empty.");

            Assert.IsInstanceOf<IEnumerable<Material>>(result, "The result should be a list of Material.");
        }

        [Test]
        public void MaterialList_WithNameData()
        {
            var result = materialService.GetMaterials("Vải");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsNotEmpty(result, "The result list should not be empty.");

            Assert.IsInstanceOf<IEnumerable<Material>>(result, "The result should be a list of Material.");
        }

        //GetMaterialList By Material Category Id
        //IsActive == true is nested in code func GetMaterials of MaterialService
        [Test]
        public void MaterialList_WithNullMaterialCategoryIdData()
        {
            var result = materialService.GetMaterialsByMaterialCategory(null);

            Assert.IsEmpty(result, "The result should be empty");
        }

        [Test]
        public void MaterialList_WithEmptyMaterialCategoryIdData()
        {
            var result = materialService.GetMaterialsByMaterialCategory("");

            Assert.IsEmpty(result, "The result should be empty");
        }

        [Test]
        public void MaterialList_WithMaterialCategoryIdData()
        {
            var result = materialService.GetMaterialsByMaterialCategory("83c2ea89-2260-425e-88fa-679611");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<IEnumerable<Material>>(result, "The result should be as List of Material object.");
        }

        //GetMaterialList By Material Type Id
        //IsActive == true is nested in code func GetMaterials of MaterialService
        [Test]
        public void MaterialList_WithNullMaterialTypeIdData()
        {
            var result = materialService.GetMaterialsByMaterialCategory(null);

            Assert.IsEmpty(result, "The result should be empty");
        }

        [Test]
        public void MaterialList_WithEmptyMaterialTypeIdData()
        {
            var result = materialService.GetMaterialsByMaterialCategory("");

            Assert.IsEmpty(result, "The result should be empty");
        }

        [Test]
        public void MaterialList_WithMaterialTypeIdData()
        {
            var result = materialService.GetMaterialsByMaterialCategory("c17e5767-6206-4883-820a-dba689");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<IEnumerable<Material>>(result, "The result should be as List of Material object.");
        }

    }
}
