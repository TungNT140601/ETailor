using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CustomException;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
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
                tasks.Add(Task.Run(() =>
                {
                    productTemplate.Id = Ultils.GenGuidString();
                }));
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
                        productTemplate.ThumbnailImage = await Ultils.UploadImage(wwwroot, "ProductTemplates/ThumnailImages", thumbnailImage, null);
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
                                listImageObject.Add(await Ultils.UploadImage(wwwroot, "ProductTemplates/Images", image, null));
                            }));
                        }

                        await Task.WhenAll(tasksImage);

                        productTemplate.Image = JsonSerializer.Serialize(listImageObject);
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
                                listImageObject.Add(await Ultils.UploadImage(wwwroot, "ProductTemplates/CollectionImages", image, null));
                            }));
                        }

                        await Task.WhenAll(tasksImage);

                        productTemplate.CollectionImage = JsonSerializer.Serialize(listImageObject);
                    }
                }));
                tasks.Add(Task.Run(() =>
                {
                    productTemplate.LastestUpdatedTime = DateTime.Now;
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
                            dbTemplate.ThumbnailImage = await Ultils.UploadImage(wwwroot, "ProductTemplates/ThumnailImages", thumbnailImage, dbTemplate.ThumbnailImage);
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
                                    listImageObject.Add(await Ultils.UploadImage(wwwroot, "ProductTemplates/Images", image, null));
                                }));
                            }

                            await Task.WhenAll(tasksImage);

                            dbTemplate.Image = JsonSerializer.Serialize(listImageObject);
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
                                    listImageObject.Add(await Ultils.UploadImage(wwwroot, "ProductTemplates/CollectionImages", image, null));
                                }));
                            }

                            await Task.WhenAll(tasksImage);

                            dbTemplate.CollectionImage = JsonSerializer.Serialize(listImageObject);
                        }
                    }));
                    tasks.Add(Task.Run(() =>
                    {
                        dbTemplate.LastestUpdatedTime = DateTime.Now;
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
                dbTemplate.CreatedTime = DateTime.Now;
                dbTemplate.LastestUpdatedTime = DateTime.Now;
                dbTemplate.IsActive = true;

                return productTemplateRepository.Update(dbTemplate.Id, dbTemplate);
            }
            else
            {
                return false;
            }
        }
        public async Task<string> UpdateDraftTemplate(ProductTemplate productTemplate, string wwwroot, IFormFile? thumbnailImage, List<IFormFile>? images, List<IFormFile>? collectionImages)
        {

            var dbTemplate = productTemplateRepository.Get(productTemplate.Id);

            if (dbTemplate != null && dbTemplate.IsActive == true)
            {
                var fileDraft = Path.Combine(wwwroot, "UpdateDraft", $"{dbTemplate.Id}.json");

                var draftTemplate = new ProductTemplate();

                if (File.Exists(fileDraft))
                {
                    //using (StreamReader reader = new StreamReader(fileDraft, Encoding.UTF8))
                    //{
                    //    string jsonString = reader.ReadToEnd();

                    //    draftTemplate = JsonSerializer.Deserialize<ProductTemplate>(jsonString);
                    //}
                    string jsonString = File.ReadAllText(fileDraft);

                    draftTemplate = JsonSerializer.Deserialize<ProductTemplate>(jsonString);
                }
                var tasks = new List<Task>();

                #region CheckCategoryAndName
                //tasks.Add(Task.Run(() =>
                //        {
                //            if (string.IsNullOrWhiteSpace(productTemplate.Name))
                //            {
                //                throw new UserException("Vui lòng nhập tên cho mẫu sản phẩm");
                //            }
                //            else
                //            {
                //                productTemplate.UrlPath = Ultils.ConvertToEnglishAlphabet(productTemplate.Name);
                //                if (productTemplateRepository.GetAll(x => x.Id != dbTemplate.Id && (x.Name == productTemplate.Name || x.UrlPath == productTemplate.UrlPath) && x.IsActive == true).Any())
                //                {
                //                    throw new UserException($"Tên mẫu sản phẩm đã được sử dụng: {productTemplate.UrlPath.Replace("-", " ")}");
                //                }
                //                else
                //                {
                //                    dbTemplate.Name = productTemplate.Name;
                //                    dbTemplate.UrlPath = productTemplate.UrlPath;
                //                }
                //            }
                //        }));
                #endregion

                #region SetDesc
                tasks.Add(Task.Run(() =>
                        {
                            if (string.IsNullOrWhiteSpace(productTemplate.Description))
                            {
                                productTemplate.Description = "";
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
                                dbTemplate.ThumbnailImage = await Ultils.UploadImage(wwwroot, "ProductTemplates/ThumnailImages", thumbnailImage, draftTemplate == null ? dbTemplate.ThumbnailImage : draftTemplate.ThumbnailImage);
                            }
                            else
                            {
                                dbTemplate.ThumbnailImage = "";
                            }
                        }));
                #endregion

                #region RemoveDraftThumnailImage
                tasks.Add(Task.Run(() =>
                        {
                            if (!string.IsNullOrEmpty(draftTemplate.ThumbnailImage))
                            {
                                Ultils.DeleteObject(draftTemplate.ThumbnailImage);
                            }
                        }));
                #endregion

                #region SetImage
                tasks.Add(Task.Run(async () =>
                {
                    if (images != null)
                    {
                        dbTemplate.Image = JsonSerializer.Serialize(await Ultils.UploadImages(wwwroot, "ProductTemplates/Images", images));
                    }
                    else
                    {
                        dbTemplate.Image = "";
                    }
                }));
                #endregion

                #region RemoveDraftImage
                if (!string.IsNullOrEmpty(draftTemplate.Image))
                {
                    var draftImages = JsonSerializer.Deserialize<List<string>>(draftTemplate.Image);
                    foreach (var draftImage in draftImages)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            Ultils.DeleteObject(draftImage);
                        }));
                    }
                }
                #endregion

                #region SetCollectionImage
                tasks.Add(Task.Run(async () =>
                {
                    if (collectionImages != null)
                    {
                        dbTemplate.CollectionImage = JsonSerializer.Serialize(await Ultils.UploadImages(wwwroot, "ProductTemplates/CollectionImages", collectionImages));
                    }
                    else
                    {
                        dbTemplate.CollectionImage = "";
                    }
                }));
                #endregion

                #region RemoveDraftCollectionImage
                if (!string.IsNullOrEmpty(draftTemplate.CollectionImage))
                {
                    var draftCollectionImages = JsonSerializer.Deserialize<List<string>>(draftTemplate.CollectionImage);
                    foreach (var draftCollectionImage in draftCollectionImages)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            Ultils.DeleteObject(draftCollectionImage);
                        }));
                    }
                }
                #endregion

                #region SetValue
                tasks.Add(Task.Run(() =>
                        {
                            dbTemplate.LastestUpdatedTime = DateTime.Now;
                            dbTemplate.InactiveTime = null;
                            dbTemplate.IsActive = true;
                        }));
                #endregion

                await Task.WhenAll(tasks);

                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                };

                var saveDraftString = JsonSerializer.Serialize(dbTemplate, options);

                File.WriteAllText(fileDraft, saveDraftString);

                return dbTemplate.Id;
            }
            else
            {
                throw new UserException("Mẫu sản phẩm không tìm thấy");
            }
        }
        public async Task<string> UpdateTemplate(string id, string wwwroot)
        {
            var dbTemplate = productTemplateRepository.Get(id);

            if (dbTemplate != null)
            {
                var fileDraft = Path.Combine(wwwroot, "UpdateDraft", $"{dbTemplate.Id}.json");

                var productTemplate = new ProductTemplate();

                if (File.Exists(fileDraft))
                {
                    string jsonString = File.ReadAllText(fileDraft);

                    productTemplate = JsonSerializer.Deserialize<ProductTemplate>(jsonString);
                }

                var tasks = new List<Task>();

                var checkUpdateBodySize = await templateBodySizeService.UpdateTemplateBodySize(productTemplate.TemplateBodySizes.ToList(), dbTemplate.Id);

                tasks.Add(Task.Run(() =>
                {
                    dbTemplate.CategoryId = productTemplate.CategoryId;
                }));

                tasks.Add(Task.Run(() =>
                {
                    dbTemplate.Name = productTemplate.Name;
                    dbTemplate.UrlPath = productTemplate.UrlPath;
                }));

                tasks.Add(Task.Run(() =>
                {
                    dbTemplate.Description = productTemplate.Description;
                }));

                tasks.Add(Task.Run(() =>
                {
                    dbTemplate.Price = productTemplate.Price;
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (!string.IsNullOrEmpty(dbTemplate.ThumbnailImage))
                    {

                    }
                    dbTemplate.ThumbnailImage = productTemplate.ThumbnailImage;
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (!string.IsNullOrEmpty(dbTemplate.ThumbnailImage))
                    {

                    }
                    dbTemplate.Image = productTemplate.Image;
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (!string.IsNullOrEmpty(dbTemplate.CollectionImage))
                    {

                    }
                    dbTemplate.CollectionImage = productTemplate.CollectionImage;
                }));

                tasks.Add(Task.Run(() =>
                {
                    dbTemplate.LastestUpdatedTime = DateTime.Now;
                    dbTemplate.InactiveTime = null;
                    dbTemplate.IsActive = true;
                    dbTemplate.TemplateBodySizes = null;
                }));

                //tasks.Add(Task.Run(() =>
                //{
                //    dbTemplate.TemplateStages = productTemplate.TemplateStages;
                //}));

                await Task.WhenAll(tasks);

                if (checkUpdateBodySize)
                {
                    return productTemplateRepository.Update(dbTemplate.Id, dbTemplate) ? productTemplate.Id : null;
                }
                else
                {
                    return null;
                }
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
                        await Task.WhenAll(
                            Task.Run(async () =>
                                {
                                    if (!string.IsNullOrEmpty(template.ThumbnailImage))
                                    {
                                        template.ThumbnailImage = await Ultils.GetUrlImage(template.ThumbnailImage);
                                    }
                                    template.CollectionImage = null;
                                    template.Image = null;
                                })
                            //,
                            //Task.Run(async () =>
                            //    {
                            //        if (!string.IsNullOrEmpty(template.Image))
                            //        {
                            //            var listImages = JsonSerializer.Deserialize<List<string>>(template.Image);
                            //            if (listImages != null && listImages.Count() > 0)
                            //            {
                            //                var listTaskImages = new List<Task>();
                            //                var listUrls = new List<string>();
                            //                foreach (var image in listImages)
                            //                {
                            //                    listTaskImages.Add(Task.Run(async () =>
                            //                    {
                            //                        listUrls.Add(await Ultils.GetUrlImage(image));
                            //                    }));
                            //                }
                            //                await Task.WhenAll(listTaskImages);

                            //                template.Image = JsonSerializer.Serialize(listUrls);
                            //            }
                            //        }
                            //    })
                            );
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
                        await Task.WhenAll(
                            Task.Run(async () =>
                                {
                                    if (!string.IsNullOrEmpty(template.ThumbnailImage))
                                    {
                                        template.ThumbnailImage = await Ultils.GetUrlImage(template.ThumbnailImage);
                                    }
                                    template.CollectionImage = null;
                                    template.Image = null;
                                })
                            //,
                            //Task.Run(async () =>
                            //    {
                            //        if (!string.IsNullOrEmpty(template.Image))
                            //        {
                            //            var listImages = JsonSerializer.Deserialize<List<string>>(template.Image);
                            //            if (listImages != null && listImages.Count() > 0)
                            //            {
                            //                var listTaskImages = new List<Task>();
                            //                var listUrls = new List<string>();
                            //                foreach (var image in listImages)
                            //                {
                            //                    listTaskImages.Add(Task.Run(async () =>
                            //                    {
                            //                        listUrls.Add(await Ultils.GetUrlImage(image));
                            //                    }));
                            //                }
                            //                await Task.WhenAll(listTaskImages);

                            //                template.Image = JsonSerializer.Serialize(listUrls);
                            //            }
                            //        }
                            //    })
                            );
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
                await Task.WhenAll(
                        Task.Run(async () =>
                        {
                            if (!string.IsNullOrEmpty(template.ThumbnailImage))
                            {
                                template.ThumbnailImage = await Ultils.GetUrlImage(template.ThumbnailImage);
                            }
                        }),
                        Task.Run(async () =>
                        {
                            if (!string.IsNullOrEmpty(template.Image))
                            {
                                var listImages = JsonSerializer.Deserialize<List<string>>(template.Image);
                                if (listImages != null && listImages.Count() > 0)
                                {
                                    var listTaskImages = new List<Task>();
                                    var listUrls = new List<string>();
                                    foreach (var image in listImages)
                                    {
                                        listTaskImages.Add(Task.Run(async () =>
                                        {
                                            listUrls.Add(await Ultils.GetUrlImage(image));
                                        }));
                                    }
                                    await Task.WhenAll(listTaskImages);

                                    template.Image = JsonSerializer.Serialize(listUrls);
                                }
                            }
                        }),
                        Task.Run(() =>
                        {
                            template.Category = categoryRepository.Get(template.CategoryId);
                        }),
                        Task.Run(async () =>
                        {
                            if (!string.IsNullOrEmpty(template.CollectionImage))
                            {
                                var listImages = JsonSerializer.Deserialize<List<string>>(template.CollectionImage);
                                if (listImages != null && listImages.Count() > 0)
                                {
                                    var listTaskImages = new List<Task>();
                                    var listUrls = new List<string>();
                                    foreach (var image in listImages)
                                    {
                                        listTaskImages.Add(Task.Run(async () =>
                                        {
                                            listUrls.Add(await Ultils.GetUrlImage(image));
                                        }));
                                    }
                                    await Task.WhenAll(listTaskImages);

                                    template.CollectionImage = JsonSerializer.Serialize(listUrls);
                                }
                            }
                        })
                        );

                return template;
            }
            return null;
        }
        public async Task<ProductTemplate> GetById(string Id)
        {
            var template = productTemplateRepository.Get(Id);
            if (template != null)
            {
                await Task.WhenAll(
                        Task.Run(async () =>
                        {
                            if (!string.IsNullOrEmpty(template.ThumbnailImage))
                            {
                                template.ThumbnailImage = await Ultils.GetUrlImage(template.ThumbnailImage);
                            }
                        }),
                        Task.Run(async () =>
                        {
                            if (!string.IsNullOrEmpty(template.Image))
                            {
                                var listImages = JsonSerializer.Deserialize<List<string>>(template.Image);
                                if (listImages != null && listImages.Count() > 0)
                                {
                                    var listTaskImages = new List<Task>();
                                    var listUrls = new List<string>();
                                    foreach (var image in listImages)
                                    {
                                        listTaskImages.Add(Task.Run(async () =>
                                        {
                                            listUrls.Add(await Ultils.GetUrlImage(image));
                                        }));
                                    }
                                    await Task.WhenAll(listTaskImages);

                                    template.Image = JsonSerializer.Serialize(listUrls);
                                }
                            }
                        }),
                        Task.Run(async () =>
                        {
                            if (!string.IsNullOrEmpty(template.CollectionImage))
                            {
                                var listImages = JsonSerializer.Deserialize<List<string>>(template.CollectionImage);
                                if (listImages != null && listImages.Count() > 0)
                                {
                                    var listTaskImages = new List<Task>();
                                    var listUrls = new List<string>();
                                    foreach (var image in listImages)
                                    {
                                        listTaskImages.Add(Task.Run(async () =>
                                        {
                                            listUrls.Add(await Ultils.GetUrlImage(image));
                                        }));
                                    }
                                    await Task.WhenAll(listTaskImages);

                                    template.CollectionImage = JsonSerializer.Serialize(listUrls);
                                }
                            }
                        })
                        );
                return template;
            }
            return null;
        }
        public bool DeleteTemplate(string id)
        {
            var dbTemplate = productTemplateRepository.Get(id);

            if (dbTemplate != null && dbTemplate.IsActive == true)
            {

                dbTemplate.InactiveTime = DateTime.Now;

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
                    foreach (var componentType in componentTypes)
                    {
                        componentType.Components = new List<Component>();
                    }

                    //var templateComponents = componentRepository.GetAll(x => x.ProductTemplateId == templateId && x.IsActive == true);
                    //if (templateComponents != null && templateComponents.Any())
                    //{
                    //    templateComponents = templateComponents.ToList();

                    //    var templateComponentTypeIds = templateComponents.GroupBy(x => x.ComponentTypeId).Select(x => x.Key);
                    //    if (templateComponentTypeIds != null && templateComponentTypeIds.Any())
                    //    {
                    //        templateComponentTypeIds = templateComponentTypeIds.ToList();


                    //        await Task.WhenAll(tasks);
                    //        foreach (var componentType in componentTypes)
                    //        {
                    //            tasks.Add(Task.Run(async () =>
                    //            {
                    //                componentType.Components = new List<Component>();
                    //                var components = templateComponents.Where(x => x.ComponentTypeId == componentType.Id);
                    //                if (components != null && components.Any())
                    //                {
                    //                    components = components.ToList();
                    //                    var tasks1 = new List<Task>();
                    //                    foreach (var component in components)
                    //                    {
                    //                        tasks1.Add(Task.Run(async () =>
                    //                        {
                    //                            if (!string.IsNullOrEmpty(component.Image))
                    //                            {
                    //                                component.Image = await Ultils.GetUrlImage(component.Image);
                    //                            }
                    //                            componentType.Components.Add(component);
                    //                        }));
                    //                    }
                    //                    await Task.WhenAll(tasks1);
                    //                }

                    //                if (componentType.Components != null && componentType.Components.Any())
                    //                {
                    //                    componentType.Components = componentType.Components.OrderBy(x => x.Index).OrderBy(x => x.Name).ToList();
                    //                }
                    //            }));
                    //        }
                    //        await Task.WhenAll(tasks);
                    //    }
                    //}

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
                        tasks.Add(Task.Run(async () =>
                        {
                            if (!string.IsNullOrEmpty(template.ThumbnailImage))
                            {
                                template.ThumbnailImage = await Ultils.GetUrlImage(template.ThumbnailImage);
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
    }
}
