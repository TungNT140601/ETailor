using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Ultity.CustomException;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class ProductTemplateService : IProductTemplateService
    {
        private readonly IProductTemplateRepository productTemplateRepository;
        private readonly ICategoryRepository categoryRepository;
        private readonly ITemplateBodySizeService templateBodySizeService;
        private readonly IComponentRepository componentRepository;
        private readonly IComponentTypeRepository componentTypeRepository;

        public ProductTemplateService(IProductTemplateRepository productTemplateRepository, ICategoryRepository categoryRepository, ITemplateBodySizeService templateBodySizeService,
            IComponentTypeRepository componentTypeRepository, IComponentRepository componentRepository)
        {
            this.productTemplateRepository = productTemplateRepository;
            this.categoryRepository = categoryRepository;
            this.templateBodySizeService = templateBodySizeService;
            this.componentRepository = componentRepository;
            this.componentTypeRepository = componentTypeRepository;
        }
        public async Task<string> AddTemplate(ProductTemplate productTemplate, string wwwroot, IFormFile? thumbnailImage, List<IFormFile>? images, List<IFormFile>? collectionImages)
        {
            var duplicate = productTemplateRepository.GetAll(x => x.Name == productTemplate.Name && x.IsActive == true);
            var category = categoryRepository.Get(productTemplate.CategoryId);

            var tasks = new List<Task>();
            if (string.IsNullOrEmpty(productTemplate.Id) || productTemplateRepository.Get(productTemplate.Id) == null)
            {
                productTemplate.Id = Ultils.GenGuidString();
                tasks.Add(Task.Run(() =>
                {
                    if (string.IsNullOrWhiteSpace(productTemplate.Name))
                    {
                        throw new UserException("Vui lòng nhập tên cho mẫu sản phẩm");
                    }
                    else
                    {
                        if (duplicate.Any())
                        {
                            throw new UserException($"Tên mẫu sản phẩm đã được sử dụng");
                        }
                        else
                        {
                            productTemplate.UrlPath = Ultils.ConvertToEnglishAlphabet(productTemplate.Name);
                        }
                    }
                }));
                tasks.Add(Task.Run(() =>
                {
                    if (productTemplate.CategoryId == null)
                    {
                        throw new UserException("Vui lòng chọn loại danh mục");
                    }
                    else
                    {
                        if (category == null || category.IsActive != true)
                        {
                            throw new UserException("Loại danh mục không tồn tại");
                        }
                    }
                }));
                tasks.Add(Task.Run(() =>
                {
                    if (string.IsNullOrWhiteSpace(productTemplate.Description))
                    {
                        productTemplate.Description = "";
                    }
                }));
                tasks.Add(Task.Run(() =>
                {
                    if (productTemplate.Price == null || productTemplate.Price == 0)
                    {
                        throw new UserException("Vui lòng nhập giá tham khảo sản phẩm cho mẫu sản phẩm");
                    }
                    else if (productTemplate.Price < 10000)
                    {
                        throw new UserException("Giá tham khảo không phù hợp");
                    }
                }));
                tasks.Add(Task.Run(async () =>
                {
                    if (thumbnailImage != null)
                    {
                        productTemplate.ThumbnailImage = await Ultils.UploadImage(wwwroot, $"ProductTemplates/{productTemplate.Id}/ThumnailImages", thumbnailImage, null);
                    }
                    else
                    {
                        throw new UserException("Vui lòng chọn hình ảnh đại diện cho mẫu sản phẩm");
                    }
                }));
                tasks.Add(Task.Run(async () =>
                {
                    if (images != null && images.Count > 0)
                    {
                        var listImageObject = new List<string>();

                        var tasksImage = new List<Task>();

                        foreach (var image in images)
                        {
                            tasksImage.Add(Task.Run(async () =>
                            {
                                listImageObject.Add(await Ultils.UploadImage(wwwroot, $"ProductTemplates/{productTemplate.Id}/Images", image, null));
                            }));
                        }

                        await Task.WhenAll(tasksImage);

                        productTemplate.Image = JsonConvert.SerializeObject(listImageObject);
                    }
                    else
                    {
                        throw new UserException("Vui lòng chọn ít nhất 1 hình ảnh cho mẫu sản phẩm");
                    }
                }));
                tasks.Add(Task.Run(async () =>
                {
                    if (collectionImages != null && collectionImages.Count > 0)
                    {
                        var listImageObject = new List<string>();

                        var tasksImage = new List<Task>();

                        foreach (var image in collectionImages)
                        {
                            tasksImage.Add(Task.Run(async () =>
                            {
                                listImageObject.Add(await Ultils.UploadImage(wwwroot, $"ProductTemplates/{productTemplate.Id}/CollectionImages", image, null));
                            }));
                        }

                        await Task.WhenAll(tasksImage);

                        productTemplate.CollectionImage = JsonConvert.SerializeObject(listImageObject);
                    }
                }));
                tasks.Add(Task.Run(() =>
                {
                    productTemplate.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                    productTemplate.InactiveTime = null;
                    productTemplate.IsActive = false;
                }));

                await Task.WhenAll(tasks);

                return productTemplateRepository.Create(productTemplate) ? productTemplate.Id : null;
            }
            else
            {
                var dbTemplate = productTemplateRepository.Get(productTemplate.Id);

                if (dbTemplate.IsActive == false)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        if (productTemplate.CategoryId == null)
                        {
                            throw new UserException("Vui lòng chọn loại danh mục");
                        }
                        else if (category == null)
                        {
                            throw new UserException("Loại danh mục không tồn tại");
                        }
                        else
                        {
                            if (dbTemplate.CategoryId != productTemplate.CategoryId)
                            {
                                throw new UserException("Không thể thay đổi loại danh mục của bản mẫu");
                            }
                        }
                    }));
                    tasks.Add(Task.Run(() =>
                    {
                        if (string.IsNullOrWhiteSpace(productTemplate.Name))
                        {
                            throw new UserException("Vui lòng nhập tên cho mẫu sản phẩm");
                        }
                        else
                        {
                            productTemplate.UrlPath = Ultils.ConvertToEnglishAlphabet(productTemplate.Name);
                            if (productTemplateRepository.GetAll(x => x.Id != dbTemplate.Id && (x.Name == productTemplate.Name || x.UrlPath == productTemplate.UrlPath) && x.IsActive == true).Any())
                            {
                                throw new UserException($"Tên mẫu sản phẩm đã được sử dụng: {productTemplate.UrlPath.Replace("-", " ")}");
                            }
                            else
                            {
                                dbTemplate.Name = productTemplate.Name;
                                dbTemplate.UrlPath = productTemplate.UrlPath;
                            }
                        }
                    }));
                    tasks.Add(Task.Run(() =>
                    {
                        if (string.IsNullOrWhiteSpace(productTemplate.Description))
                        {
                            dbTemplate.Description = "";
                        }
                        else
                        {
                            dbTemplate.Description = productTemplate.Description;
                        }
                    }));
                    tasks.Add(Task.Run(() =>
                    {
                        if (productTemplate.Price == null || productTemplate.Price == 0)
                        {
                            throw new UserException("Vui lòng nhập giá tham khảo sản phẩm cho mẫu sản phẩm");
                        }
                        else if (productTemplate.Price < 10000)
                        {
                            throw new UserException("Giá tham khảo không phù hợp");
                        }
                        else
                        {
                            dbTemplate.Price = productTemplate.Price;
                        }
                    }));
                    tasks.Add(Task.Run(async () =>
                    {
                        if (thumbnailImage != null)
                        {
                            dbTemplate.ThumbnailImage = await Ultils.UploadImage(wwwroot, $"ProductTemplates/{dbTemplate.Id}/ThumnailImages", thumbnailImage, dbTemplate.ThumbnailImage);
                        }
                    }));
                    tasks.Add(Task.Run(async () =>
                    {
                        if (!string.IsNullOrEmpty(dbTemplate.Image))
                        {
                            var listImageString = JsonConvert.DeserializeObject<List<string>>(dbTemplate.Image);
                            if (listImageString != null && listImageString.Count > 0)
                            {
                                var listTask = new List<Task>();
                                foreach (var image in listImageString)
                                {
                                    listTask.Add(Task.Run(() =>
                                    {
                                        Ultils.DeleteObject(JsonConvert.DeserializeObject<ImageFileDTO>(image)?.ObjectName);
                                    }));
                                }
                                await Task.WhenAll(listTask);
                            }
                        }
                        if (images != null && images.Count > 0)
                        {
                            var listImageObject = new List<string>();

                            var tasksImage = new List<Task>();

                            foreach (var image in images)
                            {
                                tasksImage.Add(Task.Run(async () =>
                                {
                                    listImageObject.Add(await Ultils.UploadImage(wwwroot, $"ProductTemplates/{dbTemplate.Id}/Images", image, null));
                                }));
                            }

                            await Task.WhenAll(tasksImage);

                            dbTemplate.Image = JsonConvert.SerializeObject(listImageObject);
                        }
                        else
                        {
                            throw new UserException("Vui lòng chọn ít nhất 1 hình ảnh cho mẫu sản phẩm");
                        }
                    }));
                    tasks.Add(Task.Run(async () =>
                    {
                        if (!string.IsNullOrEmpty(dbTemplate.CollectionImage))
                        {
                            var listImageString = JsonConvert.DeserializeObject<List<string>>(dbTemplate.CollectionImage);
                            if (listImageString != null && listImageString.Count > 0)
                            {
                                var listTask = new List<Task>();
                                foreach (var image in listImageString)
                                {
                                    listTask.Add(Task.Run(() =>
                                    {
                                        Ultils.DeleteObject(JsonConvert.DeserializeObject<ImageFileDTO>(image)?.ObjectName);
                                    }));
                                }
                                await Task.WhenAll(listTask);
                            }
                        }
                        if (collectionImages != null && collectionImages.Count > 0)
                        {
                            var listImageObject = new List<string>();

                            var tasksImage = new List<Task>();

                            foreach (var image in collectionImages)
                            {
                                tasksImage.Add(Task.Run(async () =>
                                {
                                    listImageObject.Add(await Ultils.UploadImage(wwwroot, $"ProductTemplates/{dbTemplate.Id}/CollectionImages", image, null));
                                }));
                            }

                            await Task.WhenAll(tasksImage);

                            dbTemplate.CollectionImage = JsonConvert.SerializeObject(listImageObject);
                        }
                    }));
                    tasks.Add(Task.Run(() =>
                    {
                        dbTemplate.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                        dbTemplate.InactiveTime = null;
                        dbTemplate.IsActive = false;
                    }));

                    await Task.WhenAll(tasks);

                    return productTemplateRepository.Update(dbTemplate.Id, dbTemplate) ? dbTemplate.Id : null;
                }
                return dbTemplate.Id;
            }
        }
        public bool CreateSaveActiveTemplate(string id)
        {
            var dbTemplate = productTemplateRepository.Get(id);
            if (dbTemplate != null)
            {
                dbTemplate.CreatedTime = DateTime.UtcNow.AddHours(7);
                dbTemplate.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                dbTemplate.IsActive = true;

                return productTemplateRepository.Update(dbTemplate.Id, dbTemplate);
            }
            else
            {
                return false;
            }
        }
        public async Task<string> UpdateTemplate(string wwwroot, ProductTemplate productTemplate, IFormFile? thumbnailImage, List<IFormFile>? images, List<string>? existOldImages, List<IFormFile>? collectionImages, List<string>? existOldCollectionImages)
        {

            var dbTemplate = productTemplateRepository.Get(productTemplate.Id);

            if (dbTemplate != null && dbTemplate.IsActive == true)
            {
                var tasks = new List<Task>();
                var dupplicate = productTemplateRepository.GetAll(x => x.Id != dbTemplate.Id && (x.Name == productTemplate.Name && x.IsActive == true));

                #region CheckCategoryAndName
                tasks.Add(Task.Run(() =>
                        {
                            if (string.IsNullOrWhiteSpace(productTemplate.Name))
                            {
                                throw new UserException("Vui lòng nhập tên cho mẫu sản phẩm");
                            }
                            else
                            {
                                productTemplate.UrlPath = Ultils.ConvertToEnglishAlphabet(productTemplate.Name);
                                if (dupplicate.Any())
                                {
                                    throw new UserException($"Tên mẫu sản phẩm đã được sử dụng: {productTemplate.UrlPath.Replace("-", " ")}");
                                }
                                else
                                {
                                    dbTemplate.Name = productTemplate.Name;
                                    dbTemplate.UrlPath = productTemplate.UrlPath;
                                }
                            }
                        }));
                #endregion

                #region SetDesc
                tasks.Add(Task.Run(() =>
                        {
                            if (string.IsNullOrWhiteSpace(productTemplate.Description))
                            {
                                dbTemplate.Description = "";
                            }
                            else
                            {
                                dbTemplate.Description = productTemplate.Description;
                            }
                        }));
                #endregion

                #region CheckPrice
                tasks.Add(Task.Run(() =>
                        {
                            if (productTemplate.Price == null || productTemplate.Price == 0)
                            {
                                throw new UserException("Vui lòng nhập giá tham khảo sản phẩm cho mẫu sản phẩm");
                            }
                            else if (productTemplate.Price < 0)
                            {
                                throw new UserException("Giá tham khảo không phù hợp");
                            }
                            else
                            {
                                dbTemplate.Price = productTemplate.Price;
                            }
                        }));
                #endregion

                #region SetThumnailImage
                tasks.Add(Task.Run(async () =>
                        {
                            if (thumbnailImage != null)
                            {
                                dbTemplate.ThumbnailImage = await Ultils.UploadImage(wwwroot, $"ProductTemplates/{dbTemplate.Id}/ThumnailImages", thumbnailImage, dbTemplate.ThumbnailImage);
                            }
                        }));
                #endregion

                #region SetImage
                //tasks.Add(Task.Run(async () =>
                //{
                dbTemplate.Image = await Ultils.CheckExistImageAfterUpdate(dbTemplate.Image, existOldImages);

                if (images != null && images.Count > 0)
                {
                    //var imageTasks = new List<Task>();
                    var imageList = new List<string>();
                    if (!string.IsNullOrEmpty(dbTemplate.Image))
                    {
                        if (!string.IsNullOrEmpty(dbTemplate.Image))
                        {
                            imageList.AddRange(JsonConvert.DeserializeObject<List<string>>(dbTemplate.Image));
                        }
                        if (images != null && images.Count > 0)
                        {
                            foreach (var image in images)
                            {
                                imageList.Add(await Ultils.UploadImage(wwwroot, $"ProductTemplates/{dbTemplate.Id}/Images", image, null));
                            }
                        }
                    }
                    else
                    {
                        if (images != null && images.Count > 0)
                        {
                            foreach (var image in images)
                            {
                                imageList.Add(await Ultils.UploadImage(wwwroot, $"ProductTemplates/{dbTemplate.Id}/Images", image, null));
                            }
                        }
                    }
                    //await Task.WhenAll(imageTasks);

                    dbTemplate.Image = JsonConvert.SerializeObject(imageList);
                }
                else if ((images == null || images.Count == 0) && string.IsNullOrEmpty(dbTemplate.Image) && (existOldImages == null || existOldImages.Count == 0))
                {
                    throw new UserException("Vui lòng chọn ít nhất 1 hình ảnh cho mẫu sản phẩm");
                }
                //}));
                #endregion

                #region SetCollectionImage
                //tasks.Add(Task.Run(async () =>
                //{
                dbTemplate.CollectionImage = await Ultils.CheckExistImageAfterUpdate(dbTemplate.CollectionImage, existOldCollectionImages);

                if (images != null && images.Count > 0)
                {
                    //var imageTasks = new List<Task>();
                    var imageList = new List<string>();
                    if (!string.IsNullOrEmpty(dbTemplate.CollectionImage))
                    {
                        if (!string.IsNullOrEmpty(dbTemplate.CollectionImage))
                        {
                            imageList.AddRange(JsonConvert.DeserializeObject<List<string>>(dbTemplate.CollectionImage));
                        }
                        if (images != null && images.Count > 0)
                        {
                            foreach (var image in images)
                            {
                                imageList.Add(await Ultils.UploadImage(wwwroot, $"ProductTemplates/{dbTemplate.Id}/CollectionImages", image, null));
                            }
                        }
                    }
                    else
                    {
                        if (images != null && images.Count > 0)
                        {
                            foreach (var image in images)
                            {
                                imageList.Add(await Ultils.UploadImage(wwwroot, $"ProductTemplates/{dbTemplate.Id}/CollectionImages", image, null));
                            }
                        }
                    }
                    //await Task.WhenAll(imageTasks);

                    dbTemplate.CollectionImage = JsonConvert.SerializeObject(imageList);
                }
                //}));
                #endregion

                #region SetValue
                tasks.Add(Task.Run(() =>
                        {
                            dbTemplate.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                            dbTemplate.InactiveTime = null;
                            dbTemplate.IsActive = true;
                        }));
                #endregion

                await Task.WhenAll(tasks);

                return productTemplateRepository.Update(dbTemplate.Id, dbTemplate) ? dbTemplate.Id : null;
            }
            else
            {
                throw new UserException("Mẫu sản phẩm không tìm thấy");
            }
        }
        public async Task<IEnumerable<ProductTemplate>> GetByCategory(string id)
        {
            var templates = productTemplateRepository.GetAll(x => x.CategoryId == id && x.IsActive == true);
            if (templates != null && templates.Any())
            {
                var listTask = new List<Task>();
                foreach (var template in templates)
                {
                    listTask.Add(Task.Run(async () =>
                    {
                        if (!string.IsNullOrEmpty(template.ThumbnailImage))
                        {
                            template.ThumbnailImage = Ultils.GetUrlImage(template.ThumbnailImage);
                        }
                        template.CollectionImage = null;
                        template.Image = null;
                    }));
                }
                await Task.WhenAll(listTask);

                return templates;
            }
            else
            {
                return null;
            }
        }
        public async Task<IEnumerable<ProductTemplate>> GetByCategorys(List<string> ids)
        {
            var templates = productTemplateRepository.GetAll(x => ids.Contains(x.CategoryId) && x.IsActive == true);
            if (templates != null && templates.Any())
            {
                var listTask = new List<Task>();
                foreach (var template in templates)
                {
                    listTask.Add(Task.Run(async () =>
                    {
                        if (!string.IsNullOrEmpty(template.ThumbnailImage))
                        {
                            template.ThumbnailImage = Ultils.GetUrlImage(template.ThumbnailImage);
                        }
                        template.CollectionImage = null;
                        template.Image = null;
                    }));
                }
                await Task.WhenAll(listTask);

                return templates;
            }
            else
            {
                return null;
            }
        }
        public async Task<ProductTemplate> GetByUrlPath(string urlPath)
        {
            var template = productTemplateRepository.GetAll(x => x.UrlPath == urlPath && x.IsActive == true).FirstOrDefault();
            if (template != null)
            {
                var tasks = new List<Task>();

                tasks.Add(Task.Run(async () =>
                {
                    if (!string.IsNullOrEmpty(template.ThumbnailImage))
                    {
                        template.ThumbnailImage = Ultils.GetUrlImage(template.ThumbnailImage);
                    }
                }));
                tasks.Add(Task.Run(async () =>
                {
                    if (!string.IsNullOrEmpty(template.Image))
                    {
                        var listImages = JsonConvert.DeserializeObject<List<string>>(template.Image);
                        if (listImages != null && listImages.Count() > 0)
                        {
                            var listTaskImages = new List<Task>();
                            var listUrls = new List<string>();
                            foreach (var image in listImages)
                            {
                                listTaskImages.Add(Task.Run(async () =>
                                {
                                    listUrls.Add(Ultils.GetUrlImage(image));
                                }));
                            }
                            await Task.WhenAll(listTaskImages);

                            template.Image = JsonConvert.SerializeObject(listUrls);
                        }
                    }
                }));
                tasks.Add(Task.Run(() =>
                {
                    template.Category = categoryRepository.Get(template.CategoryId);
                }));
                tasks.Add(Task.Run(async () =>
                {
                    if (!string.IsNullOrEmpty(template.CollectionImage))
                    {
                        var listImages = JsonConvert.DeserializeObject<List<string>>(template.CollectionImage);
                        if (listImages != null && listImages.Count() > 0)
                        {
                            var listTaskImages = new List<Task>();
                            var listUrls = new List<string>();
                            foreach (var image in listImages)
                            {
                                listTaskImages.Add(Task.Run(async () =>
                                {
                                    listUrls.Add(Ultils.GetUrlImage(image));
                                }));
                            }
                            await Task.WhenAll(listTaskImages);

                            template.CollectionImage = JsonConvert.SerializeObject(listUrls);
                        }
                    }
                }));

                await Task.WhenAll(tasks);

                return template;
            }
            return null;
        }
        public async Task<ProductTemplate> GetById(string Id)
        {
            var template = productTemplateRepository.Get(Id);
            if (template != null)
            {
                var tasks = new List<Task>();

                tasks.Add(Task.Run(async () =>
                {
                    if (!string.IsNullOrEmpty(template.ThumbnailImage))
                    {
                        template.ThumbnailImage = Ultils.GetUrlImage(template.ThumbnailImage);
                    }
                }));
                tasks.Add(Task.Run(async () =>
                {
                    if (!string.IsNullOrEmpty(template.Image))
                    {
                        var listImages = JsonConvert.DeserializeObject<List<string>>(template.Image);
                        if (listImages != null && listImages.Count() > 0)
                        {
                            var listUrls = new List<string>();

                            foreach (var image in listImages)
                            {
                                listUrls.Add(Ultils.GetUrlImage(image));
                            }

                            template.Image = JsonConvert.SerializeObject(listUrls);
                        }
                    }
                }));
                tasks.Add(Task.Run(() =>
                {
                    template.Category = categoryRepository.Get(template.CategoryId);
                }));
                tasks.Add(Task.Run(async () =>
                {
                    if (!string.IsNullOrEmpty(template.CollectionImage))
                    {
                        var listImages = JsonConvert.DeserializeObject<List<string>>(template.CollectionImage);
                        if (listImages != null && listImages.Count() > 0)
                        {
                            var listUrls = new List<string>();
                            foreach (var image in listImages)
                            {
                                listUrls.Add(Ultils.GetUrlImage(image));
                            }
                            template.CollectionImage = JsonConvert.SerializeObject(listUrls);
                        }
                    }
                }));

                await Task.WhenAll(tasks);

                return template;
            }
            return null;
        }
        public bool DeleteTemplate(string id)
        {
            var dbTemplate = productTemplateRepository.Get(id);

            if (dbTemplate != null && dbTemplate.IsActive == true)
            {

                dbTemplate.InactiveTime = DateTime.UtcNow.AddHours(7);

                dbTemplate.IsActive = false;

                return productTemplateRepository.Update(dbTemplate.Id, dbTemplate);
            }
            else
            {
                throw new UserException("Mẫu sản phẩm không tìm thấy");
            }
        }
        public async Task<IEnumerable<ComponentType>> GetTemplateComponent(string templateId)
        {
            var template = productTemplateRepository.Get(templateId);
            if (template == null)
            {
                throw new UserException("Bản mẫu không tìm thấy");
            }
            else
            {

                var componentTypes = componentTypeRepository.GetAll(x => x.CategoryId == template.CategoryId && x.IsActive == true);
                if (componentTypes != null && componentTypes.Any())
                {
                    componentTypes = componentTypes.ToList();
                    var tasks = new List<Task>();

                    var templateComponents = componentRepository.GetAll(x => x.ProductTemplateId == templateId && x.IsActive == true);
                    if (templateComponents != null && templateComponents.Any())
                    {
                        templateComponents = templateComponents.ToList();

                        foreach (var componentType in componentTypes)
                        {
                            tasks.Add(Task.Run(async () =>
                            {
                                componentType.Components = new List<Component>();
                                var components = templateComponents.Where(x => x.ComponentTypeId == componentType.Id);
                                if (components != null && components.Any())
                                {
                                    components = components.ToList();
                                    var tasks1 = new List<Task>();
                                    foreach (var component in components)
                                    {
                                        tasks1.Add(Task.Run(async () =>
                                        {
                                            if (!string.IsNullOrEmpty(component.Image))
                                            {
                                                component.Image = Ultils.GetUrlImage(component.Image);
                                            }
                                            componentType.Components.Add(component);
                                        }));
                                    }
                                    await Task.WhenAll(tasks1);
                                }

                                if (componentType.Components != null && componentType.Components.Any())
                                {
                                    componentType.Components = componentType.Components.OrderBy(x => x.Index).OrderBy(x => x.Name).ToList();
                                }
                            }));
                        }
                    }
                    else
                    {
                        foreach (var componentType in componentTypes)
                        {
                            tasks.Add(Task.Run(() =>
                            {
                                componentType.Components = new List<Component>();
                            }));
                        }
                    }
                    await Task.WhenAll(tasks);

                    return componentTypes;
                }
                return null;
            }
        }
        public async Task<IEnumerable<ProductTemplate>> GetTemplates(string? search)
        {
            var activeCategories = categoryRepository.GetAll(x => x.IsActive == true);
            if (activeCategories != null && activeCategories.Any())
            {
                activeCategories = activeCategories.ToList();

                var templates = productTemplateRepository.GetAll(x => (string.IsNullOrEmpty(search) || (!string.IsNullOrEmpty(search) && x.Name.Trim().ToLower().Contains(search.Trim().ToLower()))) && x.IsActive == true);
                if (templates != null && templates.Any())
                {
                    templates = templates.OrderBy(x => x.Name).ToList();
                    var tasks = new List<Task>();
                    foreach (var template in templates)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            if (!string.IsNullOrEmpty(template.ThumbnailImage))
                            {
                                template.ThumbnailImage = Ultils.GetUrlImage(template.ThumbnailImage);
                            }
                            template.Image = null;
                            template.CollectionImage = null;
                        }));
                    }
                    await Task.WhenAll(tasks);

                    return templates;
                }
            }

            return null;
        }

        public string ExportFile(string templateId)
        {
            var template = productTemplateRepository.Get(templateId);
            if (template != null)
            {
                var category = categoryRepository.Get(template.CategoryId);
                if (category != null && category.IsActive == true)
                {
                    var componentTypes = componentTypeRepository.GetAll(x => x.CategoryId == category.Id && x.IsActive == true);
                    if (componentTypes != null && componentTypes.Any())
                    {
                        componentTypes = componentTypes.OrderBy(x => x.Name).ToList();

                        // Set the license context
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                        FileInfo templateFile = new FileInfo("./wwwroot/File/Export/Output.xlsx");
                        using (ExcelPackage excelPackage = new ExcelPackage(templateFile))
                        {
                            if (excelPackage.Workbook.Worksheets.Any(ws => ws.Name == "Sheet1"))
                            {
                                // Delete the sheet
                                excelPackage.Workbook.Worksheets.Delete("Sheet1");
                            }

                            foreach (var componentType in componentTypes)
                            {
                                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add($"{componentType.Name}");

                                worksheet.View.ZoomScale = 150;

                                worksheet.Cells[1, 1].Value = $"{componentType.Id}";
                                worksheet.Cells[1, 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
                                worksheet.Cells[1, 2].Value = $"{componentType.Name}";
                                worksheet.Cells["B1:D1"].Merge = true;

                                worksheet.Cells[2, 1].Value = "STT";
                                worksheet.Cells[2, 2].Value = "Tên kiểu";
                                worksheet.Cells[2, 3].Value = "Hình ảnh";
                                worksheet.Cells[2, 4].Value = "Kiểu mặc định (x)";

                                for (int i = 3; i < 13; i++)
                                {
                                    worksheet.Cells[i, 1].Value = i - 2;
                                    worksheet.Rows[i].Height = 50;
                                }

                                worksheet.Columns[1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                                worksheet.Columns[1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                worksheet.Columns[2].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                worksheet.Columns[3].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                worksheet.Columns[4].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                                worksheet.Columns[1].Width = 3.5;
                                worksheet.Columns[3].Width = 25;

                                worksheet.Columns[4].AutoFit();

                                worksheet.Cells.Style.Locked = false;
                                worksheet.Cells["A1:D2"].Style.Locked = true;
                                worksheet.Protection.IsProtected = true;

                                #region ExportData

                                //var templateComponents = componentRepository.GetAll(x => x.ProductTemplateId == templateId && x.IsActive == true);
                                //if (templateComponents != null && templateComponents.Any())
                                //{
                                //    templateComponents = templateComponents.OrderBy(x => x.Name).ToList();

                                //}
                                //else
                                //{
                                //    templateComponents = new List<Component>();
                                //}

                                //int startRow = 2; // Assuming data starts from row 2

                                //if (templateComponents != null && templateComponents.Any(x => x.ComponentTypeId == componentType.Id))
                                //{
                                //    var componentOfType = templateComponents.Where(x => x.ComponentTypeId == componentType.Id).ToList();

                                //    for (int i = 0; i < componentOfType.Count; i++)
                                //    {
                                //        var row = startRow + i;
                                //        worksheet.Rows[row].Height = 100;
                                //        var colName = 1;
                                //        worksheet.Columns[colName].AutoFit();
                                //        var colImage = 2;
                                //        worksheet.Columns[colImage].Width = 20;
                                //        var colDefault = 3;
                                //        worksheet.Columns[colDefault].AutoFit();
                                //        var rowData = componentOfType[i];

                                //        worksheet.Cells[row, colName].Value = rowData.Name;


                                //        if (!string.IsNullOrEmpty(rowData.Image))
                                //        {

                                //            var fileDtp = JsonConvert.DeserializeObject<ImageFileDTO>(rowData.Image);

                                //            byte[] imageBytes = Ultils.DownloadImageFromFirebase(fileDtp.ObjectName);

                                //            var fileName = fileDtp.ObjectName.Split("/").Last();
                                //            var filePath = $"./wwwroot/File/Template/{fileName}";
                                //            File.WriteAllBytes(filePath, imageBytes);

                                //            using (var fileStream = System.IO.File.OpenRead(filePath))
                                //            {
                                //                ExcelPicture picture = worksheet.Drawings.AddPicture(fileDtp.ObjectName, fileStream);

                                //                picture.SetPosition(row - 1, 1, colImage - 1, 1); // Set the position of the image within the cell
                                //                picture.SetSize(100, 100);
                                //            }
                                //            System.IO.File.Delete(filePath);

                                //        }
                                //        worksheet.Cells[startRow + i, 3].Value = rowData.Default;


                                //    }


                                //    ExcelRange range = worksheet.Cells[2, 1, componentOfType.Count + 1, 3];

                                //    // Set border style
                                //    var border = range.Style.Border;
                                //    border.Bottom.Style = border.Left.Style = border.Top.Style = border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Medium; // Set border style to thin
                                //    border.Bottom.Color.SetColor(System.Drawing.Color.Black); // Set border color to black
                                //    border.Left.Color.SetColor(System.Drawing.Color.Black);
                                //    border.Top.Color.SetColor(System.Drawing.Color.Black);
                                //    border.Right.Color.SetColor(System.Drawing.Color.Black);

                                //} 
                                #endregion
                                excelPackage.Save();
                            }

                            excelPackage.Save();
                        }

                        return "./wwwroot/File/Export/Output.xlsx";
                    }
                    else
                    {
                        throw new UserException("Không tìm thấy bộ phận của loại bản mẫu");
                    }
                }
                else
                {
                    throw new UserException("Không tìm thấy loại bản mẫu");
                }
            }
            else
            {
                throw new UserException("Không tìm thấy bản mẫu");
            }
        }
    }
}
