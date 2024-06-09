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
    public class MaterialCategoryServiceTest
    {
        IMaterialCategoryService materialCategoryService;
        ETailor_DBContext dBContext = new ETailor_DBContext();

        IMaterialRepository materialRepository;
        IMaterialCategoryRepository materialCategoryRepository;

        [SetUp]
        public void Setup()
        {
            materialRepository = new MaterialRepository(dBContext);
            materialCategoryRepository = new MaterialCategoryRepository(dBContext);

            materialCategoryService = new MaterialCategoryService(materialRepository, materialCategoryRepository);
        }

        //GetMaterialCategoryList By Name
        //IsActive == true is nested in code func GetMaterialCategorys of MaterialService
        [Test]
        public void MaterialCategoryList_WithNullNameData()
        {
            var result = materialCategoryService.GetMaterialCategorys(null);

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsNotEmpty(result, "The result list should not be empty.");

            Assert.IsInstanceOf<IEnumerable<MaterialCategory>>(result, "The result should be a list of Material Category.");
        }

        [Test]
        public void MaterialCategoryList_WithEmptyNameData()
        {
            var result = materialCategoryService.GetMaterialCategorys("");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsNotEmpty(result, "The result list should not be empty.");

            Assert.IsInstanceOf<IEnumerable<MaterialCategory>>(result, "The result should be a list of Material Category.");
        }

        [Test]
        public void MaterialCategoryList_WithNameData()
        {
            var result = materialCategoryService.GetMaterialCategorys("Vải");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsNotEmpty(result, "The result list should not be empty.");

            Assert.IsInstanceOf<IEnumerable<MaterialCategory>>(result, "The result should be a list of Material Category.");
        }

        //GetStaff By Id 
        //IsActive == true is nested in code func GetStaff of StaffService
        [Test]
        public void MaterialCategoryDetail_WithNullIdData()
        {
            var result = materialCategoryService.GetMaterialCategory(null);

            Assert.IsNull(result, "The result should be null");
        }

        [Test]
        public void MaterialCategoryDetail_WithEmptyIdData()
        {
            var result = materialCategoryService.GetMaterialCategory("");

            Assert.AreEqual(null, result, "The result should be null");
        }

        [Test]
        public void MaterialCategoryDetail_WithIdData()
        {
            var result = materialCategoryService.GetMaterialCategory("4014522f-8858-446a-bdc3-e16d49");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<MaterialCategory>(result, "The result should be as Material Category object.");
        }
    }
}
