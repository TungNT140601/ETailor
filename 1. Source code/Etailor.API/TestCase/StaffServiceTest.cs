using Etailor.API.Repository.DataAccess;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.Repository;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCase
{
    public class StaffServiceTest
    {
        IStaffService staffService;
        ETailor_DBContext dBContext = new ETailor_DBContext();

        IStaffRepository staffRepository;
        ICategoryRepository categoryRepository;
        IMasteryRepository masteryRepository;
        IProductRepository productRepository;
        IConfiguration configuration;

        [SetUp]
        public void Setup()
        {
            staffRepository = new StaffRepository(dBContext);
            categoryRepository = new CategoryRepository(dBContext);
            masteryRepository = new MasteryRepository(dBContext);
            productRepository = new ProductRepository(dBContext);

            staffService = new StaffService(staffRepository, configuration, categoryRepository, masteryRepository, productRepository);
        }

        //GetStaffList By Name
        //IsActive == true is nested in code func GetStaffs of StaffService
        [Test]
        public void StaffList_WithNullNameData()
        {
            var result =  staffService.GetAll(null);

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsNotEmpty(result, "The result list should not be empty.");

            Assert.IsInstanceOf<IEnumerable<Staff>>(result, "The result should be a list of Staff.");
        }

        [Test]
        public void StaffList_WithEmptyNameData()
        {
            var result = staffService.GetAll("");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsNotEmpty(result, "The result list should not be empty.");

            Assert.IsInstanceOf<IEnumerable<Staff>>(result, "The result should be a list of Staff.");
        }

        [Test]
        public void StaffList_WithNameData()
        {
            var result = staffService.GetAll("Tú");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsNotEmpty(result, "The result list should not be empty.");

            Assert.IsInstanceOf<IEnumerable<Staff>>(result, "The result should be a list of Staff.");
        }

        //GetStaff By Id 
        //Actor: Manager
        //IsActive == true is nested in code func GetStaff of StaffService
        [Test]
        public async Task StaffDetail_WithNullIdData_ActorManager()
        {
            var result = await staffService.GetStaff(null);

            Assert.IsNull(result, "The result should be null");
        }

        [Test]
        public async Task StaffDetail_WithEmptyIdData_ActorManager()
        {
            var result = await staffService.GetStaff("");

            Assert.AreEqual(null, result, "The result should be null");
        }

        [Test]
        public async Task StaffDetail_WithIdData_ActorManager()
        {
            var result = await staffService.GetStaff("18559deb-ae14-4e5a-a011-090c6b");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<Staff>(result, "The result should be as Staff object.");
        }

        //GetStaff By Id 
        //Actor: Staff
        //IsActive == true is nested in code func GetStaff of StaffService
        [Test]
        public async Task StaffDetail_WithNullIdData_ActorStaff()
        {
            var result = await staffService.GetStaffInfo(null);

            Assert.IsNull(result, "The result should be null");
        }

        [Test]
        public async Task StaffDetail_WithEmptyIdData_ActorStaff()
        {
            var result = await staffService.GetStaffInfo("");

            Assert.AreEqual(null, result, "The result should be null");
        }

        [Test]
        public async Task StaffDetail_WithIdData_ActorStaff()
        {
            var result = await staffService.GetStaffInfo("18559deb-ae14-4e5a-a011-090c6b");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<Staff>(result, "The result should be as Staff object.");
        }
    }
}
