using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CustomException;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class ProductStageService : IProductStageService
    {
        private readonly IProductStageRepository productStageRepository;
        private readonly IOrderRepository orderRepository;
        private readonly IProductRepository productRepository;
        private readonly IProductTemplateRepository productTemplateRepository;
        private readonly IStaffRepository staffRepository;
        private readonly IMasteryRepository masteryRepository;
        private readonly ITemplateStateRepository templateStateRepository;
        private readonly IComponentStageRepository componentStageRepository;
        private readonly IComponentRepository componentRepository;

        public ProductStageService(IProductStageRepository productStageRepository, IOrderRepository orderRepository, IProductRepository productRepository,
            IProductTemplateRepository productTemplateRepository, IStaffRepository staffRepository, IMasteryRepository masteryRepository,
            ITemplateStateRepository templateStateRepository, IComponentStageRepository componentStageRepository, IComponentRepository componentRepository)
        {
            this.productStageRepository = productStageRepository;
            this.orderRepository = orderRepository;
            this.productRepository = productRepository;
            this.productTemplateRepository = productTemplateRepository;
            this.staffRepository = staffRepository;
            this.masteryRepository = masteryRepository;
            this.templateStateRepository = templateStateRepository;
            this.componentStageRepository = componentStageRepository;
            this.componentRepository = componentRepository;
        }

        public bool CreateProductStage()
        {
            var approveOrders = orderRepository.GetAll(x => x.Status == 2 && x.IsActive == true).ToList();
            if (approveOrders != null && approveOrders.Any())
            {
                var productOrders = productRepository.GetAll(x => approveOrders.Select(o => o.Id).Contains(x.OrderId) && x.Status > 0 && x.IsActive == true);
                if (productOrders != null && productOrders.Any())
                {
                    productOrders = productOrders.ToList();

                    var templates = productTemplateRepository.GetAll(x => productOrders.Select(c => c.ProductTemplateId).Contains(x.Id));

                    if (templates != null && templates.Any())
                    {
                        templates = templates.ToList();

                        var templateStages = templateStateRepository.GetAll(x => templates.Select(c => c.Id).Contains(x.ProductTemplateId) && x.IsActive == true);

                        if (templateStages != null && templateStages.Any())
                        {
                            templateStages = templateStages.ToList();

                            var stageComponents = componentStageRepository.GetAll(x => templateStages.Select(c => c.Id).Contains(x.TemplateStageId));

                            if (stageComponents != null && stageComponents.Any())
                            {
                                stageComponents = stageComponents.ToList();

                                var components = componentRepository.GetAll(x => templates.Select(c => c.Id).Contains(x.ProductTemplateId) && x.IsActive == true);
                                if (components != null && components.Any())
                                {
                                    components = components.ToList();

                                    var productStages = new List<ProductStage>();

                                    foreach (var product in productOrders)
                                    {
                                        if (!string.IsNullOrWhiteSpace(product.SaveOrderComponents))
                                        {
                                            var productTemplate = templates.SingleOrDefault(x => x.Id == product.ProductTemplateId);
                                            if (productTemplate != null)
                                            {
                                                var productTemplateStagees = templateStages.Where(x => x.ProductTemplateId == product.ProductTemplateId);
                                                if (productTemplateStagees != null && productTemplateStagees.Any())
                                                {
                                                    foreach (var stage in productTemplateStagees.OrderBy(x => x.StageNum))
                                                    {
                                                        var productStage = new ProductStage()
                                                        {
                                                            Id = Ultils.GenGuidString(),
                                                            Deadline = null,
                                                            StartTime = null,
                                                            FinishTime = null,
                                                            InactiveTime = null,
                                                            IsActive = true,
                                                            ProductId = product.Id,
                                                            StaffId = null,
                                                            StageNum = stage.StageNum,
                                                            TaskIndex = null,
                                                            Status = 1,
                                                            TemplateStageId = stage.Id,
                                                            ProductComponents = new List<ProductComponent>()
                                                        };

                                                        var componentTypesInStage = stageComponents.Where(x => x.TemplateStageId == stage.Id);
                                                        if (componentTypesInStage != null && componentTypesInStage.Any())
                                                        {
                                                            var componentsInStage = components.Where(x => componentTypesInStage.Select(c => c.Id).Contains(x.ComponentTypeId));
                                                            if (componentsInStage != null && componentsInStage.Any())
                                                            {
                                                                var productComponents = JsonConvert.DeserializeObject<List<ProductComponent>>(product.SaveOrderComponents);
                                                                if (productComponents != null && productComponents.Any())
                                                                {
                                                                    var productComponent = productComponents.SingleOrDefault(x => componentsInStage.Select(c => c.Id).Contains(x.ComponentId));
                                                                    if (productComponent != null)
                                                                    {
                                                                        productComponent.ProductStageId = productStage.Id;
                                                                        productComponent.ProductComponentMaterials = null;

                                                                        productStage.ProductComponents.Add(productComponent);
                                                                    }
                                                                }
                                                            }
                                                        }

                                                        productStages.Add(productStage);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    return productStageRepository.CreateRange(productStages);
                                }
                            }
                        }
                    }
                }
            }

            throw new SystemsException("Error some where");
        }

        public void SendDemoSchedule(string hourly)
        {
            try
            {
                //string fromMail = "tungnt14062001@gmail.com";
                //string fromPassword = "gblfgbilbwaehjkw"; //"tungnt14062001@gmail.com"

                string fromMail = "tuetailor@gmail.com";
                string fromPassword = "idpqyvuzktpgstlb"; //"tuetailor@gmail.com"

                //string fromMail = "tudase151149@gmail.com";
                //string frompassword = "abrxaexoqqpkrjiz"; //"tudase151149@gmail.com"

                var timeNow = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                var timeUTC = DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss");

                MailMessage message = new MailMessage();
                message.From = new MailAddress(fromMail);
                message.Subject = $"Demo Test Schedule at: [{timeNow}]; UTC: [{timeUTC}]";
                message.To.Add(new MailAddress("tungnt14062001@gmail.com"));
                message.Body = $"Body test auto run function \"{hourly}\" at: [{timeNow}]; UTC: [{timeUTC}]";
                message.IsBodyHtml = true;

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromMail, fromPassword),
                    EnableSsl = true,
                };

                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
