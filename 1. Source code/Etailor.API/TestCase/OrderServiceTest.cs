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
    public class OrderServiceTest
    {
        ETailor_DBContext dBContext = new ETailor_DBContext();

        IStaffRepository staffRepository;
        ICustomerRepository customerRepository;
        IOrderRepository orderRepository;
        IDiscountRepository discountRepository;
        IProductRepository productRepository;
        IPaymentRepository paymentRepository;
        IProductTemplateRepository productTemplaTeRepository;
        IProductTemplateService productTemplateService;
        IProductStageRepository productStageRepository;
        IOrderMaterialRepository orderMaterialRepository;
        IMaterialRepository materialRepository;

        IProductTemplateRepository productTemplateRepository;
        ICategoryRepository categoryRepository;
        ITemplateBodySizeService templateBodySizeService;
        IComponentRepository componentRepository;
        IComponentTypeRepository componentTypeRepository;

        ITemplateBodySizeRepository templateBodySizeRepository;
        IBodySizeRepository bodySizeRepository;

        IOrderService orderService;

        [SetUp]
        public void Setup()
        {
            staffRepository = new StaffRepository(dBContext);
            customerRepository = new CustomerRepository(dBContext);
            orderRepository = new OrderRepository(dBContext);
            discountRepository = new DiscountRepository(dBContext);
            productRepository = new ProductRepository(dBContext);
            paymentRepository = new PaymentRepository(dBContext);
            productTemplaTeRepository = new ProductTemplateRepository(dBContext);
            productStageRepository = new ProductStageRepository(dBContext);
            orderMaterialRepository = new OrderMaterialRepository(dBContext);
            materialRepository = new MaterialRepository(dBContext);

            productTemplateRepository = new ProductTemplateRepository(dBContext);
            categoryRepository = new CategoryRepository(dBContext);
            componentRepository = new ComponentRepository(dBContext);
            componentTypeRepository = new ComponentTypeRepository(dBContext);

            templateBodySizeRepository = new TemplateBodySizeRepository(dBContext);
            bodySizeRepository = new BodySizeRepository(dBContext);
            templateBodySizeService = new TemplateBodySizeService(templateBodySizeRepository, productTemplateRepository, bodySizeRepository);
            productTemplateService = new ProductTemplateService(productTemplateRepository, categoryRepository, templateBodySizeService, componentTypeRepository, componentRepository);

            orderService = new OrderService(staffRepository, customerRepository, orderRepository, discountRepository, productRepository, paymentRepository, productTemplaTeRepository,
                productTemplateService, productStageRepository, orderMaterialRepository, materialRepository);
        }

        //GetOrderList no param
        //IsActive == true is nested in code func GetOrders of OrderService
        [Test]
        public async Task OrderList_WithNoParamData()
        {
            var result = await orderService.GetOrders();

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsNotEmpty(result, "The result list should not be empty.");

            Assert.IsInstanceOf<IEnumerable<Order>>(result, "The result should be a list of Order.");
        }

        //GetOrderList By Customer Id 
        //IsActive == true is nested in code func GetCategory of CategoryService
        [Test]
        public async Task OrderList_WithNullCustomerIdData()
        {
            var result = await orderService.GetOrdersByCustomer(null);

            Assert.IsEmpty(result, "The result should be empty");
        }

        [Test]
        public async Task OrderList_WithEmptyCustomerIdData()
        {
            var result = await orderService.GetOrdersByCustomer("");

            Assert.IsEmpty(result, "The result should be empty");
        }

        [Test]
        public void OrderList_WithCustomerIdData()
        {
            var result = orderService.GetOrdersByCustomer("e127339a-252c-4dfd-acf9-65b34d");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<IEnumerable<Order>>(result, "The result should be as Order object.");
        }

        //GetOrder By Id 
        //IsActive == true is nested in code func GetCategory of CategoryService
        [Test]
        public async Task OrderDetail_WithNullIdData()
        {
            var result = await orderService.GetOrder(null);

            Assert.IsNull(result, "The result should be null");
        }

        [Test]
        public async Task OrderDetail_WithEmptyIdData()
        {
            var result = await orderService.GetOrder("");

            Assert.AreEqual(null, result, "The result should be null");
        }

        [Test]
        public async Task OrderDetail_WithIdData()
        {
            var result = await orderService.GetOrder("240409.1213.233941");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<Order>(result, "The result should be as Order object.");
        }

        //GetOrder By Id + Customer Id
        //IsActive == true is nested in code func GetCategory of CategoryService
        [Test]
        public async Task OrderDetail_WithNullId_AndNullCustomerIdData()
        {
            var result = await orderService.GetOrderByCustomer(null, null);

            Assert.IsNull(result, "The result should be null");
        }

        [Test]
        public async Task OrderDetail_WithNullId_AndEmptyCustomerIdData()
        {
            var result = await orderService.GetOrderByCustomer("", null);

            Assert.AreEqual(null, result, "The result should be null");
        }

        [Test]
        public async Task OrderDetail_WithNullId_AndCustomerIdData()
        {
            var result = await orderService.GetOrderByCustomer("e127339a-252c-4dfd-acf9-65b34d", null);

            Assert.AreEqual(null, result, "The result should be null");
        }


        [Test]
        public async Task OrderDetail_WithEmptyId_AndNullCustomerIdData()
        {
            var result = await orderService.GetOrderByCustomer("e127339a-252c-4dfd-acf9-65b34d", null);

            Assert.AreEqual(null, result, "The result should be null");
        }

        [Test]
        public async Task OrderDetail_WithEmptyId_AndEmptyCustomerIdData()
        {
            var result = await orderService.GetOrderByCustomer("", null);

            Assert.AreEqual(null, result, "The result should be null");
        }

        [Test]
        public async Task OrderDetail_WithEmptyId_AndCustomerIdData()
        {
            var result = await orderService.GetOrderByCustomer(null, null);

            Assert.IsNull(result, "The result should be null");
        }


        [Test]
        public async Task OrderDetail_WithId_AndNullCustomerIdData()
        {
            var result = await orderService.GetOrderByCustomer(null, "240409.1227.009671");

            Assert.AreEqual(null, result, "The result should be null");
        }

        [Test]
        public async Task OrderDetail_WithId_AndEmptyCustomerIdData()
        {
            var result = await orderService.GetOrderByCustomer("", "240409.1227.009671");

            Assert.AreEqual(null, result, "The result should be null");
        }

        [Test]
        public async Task OrderDetail_WithId_AndCustomerIdData()
        {
            var result = await orderService.GetOrderByCustomer("e127339a-252c-4dfd-acf9-65b34d", "240409.1227.009671");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<Order>(result, "The result should be as Order object.");
        }

    }
}
