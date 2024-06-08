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
    public class ProfileBodyServiceTest
    {
        
        ETailor_DBContext dBContext = new ETailor_DBContext();
        
        IProfileBodyService profileBodyService;
        IProfileBodyRepository profileBodyRepository;
        ICustomerRepository customerRepository;
        IStaffRepository staffRepository;
        IBodySizeRepository bodySizeRepository;
        IBodyAttributeRepository bodyAttributeRepository;

        IBodySizeService bodySizeService;
        ITemplateBodySizeRepository templateBodySizeRepository;

        IBodyAttributeService bodyAttributeService;


        [SetUp]
        public void Setup()
        {
            profileBodyRepository = new ProfileBodyRepository(dBContext);
            customerRepository = new CustomerRepository(dBContext);
            staffRepository = new StaffRepository(dBContext);
            bodySizeRepository = new BodySizeRepository(dBContext);
            bodyAttributeRepository = new BodyAttributeRepository(dBContext);
            templateBodySizeRepository = new TemplateBodySizeRepository(dBContext);

            bodySizeService = new BodySizeService(bodySizeRepository, bodyAttributeRepository, templateBodySizeRepository);

            bodyAttributeService = new BodyAttributeService(bodyAttributeRepository, profileBodyRepository, bodySizeRepository);

            profileBodyService = new ProfileBodyService(customerRepository, staffRepository, profileBodyRepository,
                bodySizeRepository, bodyAttributeRepository, bodySizeService, bodyAttributeService);
        }

        //GetProfileBodyList By CustomerId
        //IsActive == true is nested in code func GetProfileBodys of ProfileBodyService
        [Test]
        public void ProfileBodyList_WithNullCustomerIdData()
        {
            var result = profileBodyService.GetProfileBodysByCustomerId(null);

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<IEnumerable<ProfileBody>>(result, "The result should be a list of Discount.");
        }

        [Test]
        public void ProfileBodyList_WithEmptyCustomerIdData()
        {
            var result = profileBodyService.GetProfileBodysByCustomerId("");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<IEnumerable<ProfileBody>>(result, "The result should be a list of Blog.");
        }

        [Test]
        public void ProfileBodyList_WithCustomerIdData()
        {
            var result = profileBodyService.GetProfileBodysByCustomerId("a102e976-30c2-44f7-8ba5-7384f2");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<IEnumerable<ProfileBody>>(result, "The result should be a list of Blog.");
        }

        //GetProfileBodyList By StaffId
        //IsActive == true is nested in code func GetProfileBodys of ProfileBodyService
        [Test]
        public void ProfileBodyList_WithNullStaffIdData()
        {
            var result = profileBodyService.GetProfileBodysByStaffId(null);

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<IEnumerable<ProfileBody>>(result, "The result should be a list of Discount.");
        }

        [Test]
        public void ProfileBodyList_WithEmptyStaffIdData()
        {
            var result = profileBodyService.GetProfileBodysByStaffId("");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<IEnumerable<ProfileBody>>(result, "The result should be a list of Blog.");
        }

        [Test]
        public void ProfileBodyList_WithStaffIdData()
        {
            var result = profileBodyService.GetProfileBodysByStaffId("18559deb-ae14-4e5a-a011-090c6b");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<IEnumerable<ProfileBody>>(result, "The result should be a list of Blog.");
        }

        //GetProfileBody By Id 
        //IsActive == true is nested in code func GetProfileBody of ProfileBodyService
        [Test]
        public async Task ProfileBodyDetail_WithNullIdData()
        {
            var result = await profileBodyService.GetProfileBody(null);

            Assert.IsNull(result, "The result should be null");
        }

        [Test]
        public async Task ProfileBodyDetail_WithEmptyIdData()
        {
            var result = await profileBodyService.GetProfileBody("");

            Assert.AreEqual(null, result, "The result should be null");
        }

        [Test]
        public async Task ProfileBodyDetail_WithIdData()
        {
            var result = await profileBodyService.GetProfileBody("340f90c4-2420-4156-8865-8ca84d");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<ProfileBody>(result, "The result should be as ProfileBody object.");
        }
    }
}
