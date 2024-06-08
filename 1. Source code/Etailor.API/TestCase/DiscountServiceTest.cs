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
    public class DiscountServiceTest
    {
        IDiscountService discountService;
        ETailor_DBContext dBContext = new ETailor_DBContext();

        IDiscountRepository discountRepository;


        [SetUp]
        public void Setup()
        {
            discountRepository = new DiscountRepository(dBContext);


            discountService = new DiscountService(discountRepository);
        }

        //GetDiscountList By Name
        //IsActive == true is nested in code func GetDiscounts of DiscountService
        [Test]
        public void DiscountList_WithNullNameOrCodeData()
        {
            var result = discountService.GetDiscounts(null);

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsNotEmpty(result, "The result list should not be empty.");

            Assert.IsInstanceOf<IEnumerable<Discount>>(result, "The result should be a list of Discount.");
        }

        [Test]
        public void DiscountList_WithEmptyNameOrCodeData()
        {
            var result = discountService.GetDiscounts("");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsNotEmpty(result, "The result list should not be empty.");

            Assert.IsInstanceOf<IEnumerable<Discount>>(result, "The result should be a list of Discount.");
        }

        [Test]
        public void DiscountList_WithNameOrCodeData()
        {
            var result = discountService.GetDiscounts("Sale");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsNotEmpty(result, "The result list should not be empty.");

            Assert.IsInstanceOf<IEnumerable<Discount>>(result, "The result should be a list of Discount.");
        }

        //GetDiscount By Id 
        //IsActive == true is nested in code func GetStaff of StaffService
        [Test]
        public void DiscountDetail_WithNullIdData()
        {
            var result = discountService.GetDiscount(null);

            Assert.IsNull(result, "The result should be null");
        }

        [Test]
        public void DiscountDetail_WithEmptyIdData()
        {
            var result = discountService.GetDiscount("");

            Assert.AreEqual(null, result, "The result should be null");
        }

        [Test]
        public void DiscountDetail_WithIdData()
        {
            var result = discountService.GetDiscount("2f9a3b93-1e9e-4664-84ac-1695a8");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<Discount>(result, "The result should be as Discount object.");
        }
    }
}
