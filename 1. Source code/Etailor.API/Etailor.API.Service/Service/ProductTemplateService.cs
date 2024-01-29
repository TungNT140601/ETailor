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
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class ProductTemplateService : IProductTemplateService
    {
        private readonly IProductTemplateRepository productTemplateRepository;
        private readonly ICategoryRepository categoryRepository;

        public ProductTemplateService(IProductTemplateRepository productTemplateRepository, ICategoryRepository categoryRepository)
        {
            this.productTemplateRepository = productTemplateRepository;
            this.categoryRepository = categoryRepository;
        }

        public async Task<string> AddTemplate(ProductTemplate productTemplate, string wwwroot, IFormFile? thumbnailImage, List<IFormFile>? images, List<IFormFile>? collectionImages)
        {
            if (string.IsNullOrEmpty(productTemplate.Id) || productTemplateRepository.Get(productTemplate.Id) == null)
            {
                var genId = Task.Run(() =>
                {
                    productTemplate.Id = Ultils.GenGuidString();
                });

                var checkCategoryAndName = Task.Run(() =>
                {
                    if (productTemplate.CategoryId == null)
                    {
                        throw new UserException("Vui lòng chọn loại danh mục");
                    }
                    else
                    {
                        var category = categoryRepository.Get(productTemplate.CategoryId);
                        if (category == null || category.IsActive != true)
                        {
                            throw new UserException("Loại danh mục không tồn tại");
                        }
                    }


                    if (string.IsNullOrWhiteSpace(productTemplate.Name))
                    {
                        throw new UserException("Vui lòng nhập tên cho mẫu sản phẩm");
                    }
                    else
                    {
                        productTemplate.UrlPath = Ultils.ConvertToEnglishAlphabet(productTemplate.Name);
                        if (productTemplateRepository.GetAll(x => (x.Name == productTemplate.Name || x.UrlPath == productTemplate.UrlPath) && x.IsActive == true).Any())
                        {
                            throw new UserException($"Tên mẫu sản phẩm đã được sử dụng: {productTemplate.UrlPath.Replace("-", " ")}");
                        }
                    }
                });

                var setDesc = Task.Run(() =>
                {
                    if (string.IsNullOrWhiteSpace(productTemplate.Description))
                    {
                        productTemplate.Description = "";
                    }
                });

                var setThumbnailImage = Task.Run(async () =>
                {
                    if (thumbnailImage != null)
                    {
                        productTemplate.ThumbnailImage = await Ultils.UploadImage(wwwroot, "ThumnailImages", thumbnailImage, null);
                    }
                });

                var checkPrice = Task.Run(() =>
                {
                    if (productTemplate.Price == null || productTemplate.Price == 0)
                    {
                        throw new UserException("Vui lòng nhập giá tham khảo sản phẩm cho mẫu sản phẩm");
                    }
                    else if (productTemplate.Price < 0)
                    {
                        throw new UserException("Giá tham khảo không phù hợp");
                    }
                });

                var setImage = Task.Run(async () =>
                {
                    if (images != null && images.Count > 0)
                    {
                        productTemplate.Image = await HandleImage(null, images, wwwroot, "TemplateImages");
                    }
                });

                var setCoolectionImage = Task.Run(async () =>
                {
                    if (collectionImages != null && collectionImages.Count > 0)
                    {
                        productTemplate.CollectionImage = await HandleImage(null, collectionImages, wwwroot, "TemplateCollectionImages");
                    }
                });

                var setValue = Task.Run(() =>
                {
                    productTemplate.LastestUpdatedTime = DateTime.Now;
                    productTemplate.InactiveTime = null;
                    productTemplate.IsActive = false;
                });

                await Task.WhenAll(genId, checkCategoryAndName, setDesc, checkPrice, setThumbnailImage, setImage, setCoolectionImage, setValue);

                return productTemplateRepository.Create(productTemplate) ? productTemplate.Id : null;
            }
            else
            {
                var dbTemplate = productTemplateRepository.Get(productTemplate.Id);

                if (dbTemplate.IsActive == false)
                {

                    var checkCategoryAndName = Task.Run(() =>
                    {
                        if (productTemplate.CategoryId == null)
                        {
                            throw new UserException("Vui lòng chọn loại danh mục");
                        }
                        else if (categoryRepository.Get(productTemplate.CategoryId) == null || categoryRepository.Get(productTemplate.CategoryId).IsActive != true)
                        {
                            throw new UserException("Loại danh mục không tồn tại");
                        }
                        else
                        {
                            dbTemplate.CategoryId = productTemplate.CategoryId;
                        }

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
                    });

                    var setDesc = Task.Run(() =>
                    {
                        if (string.IsNullOrWhiteSpace(productTemplate.Description))
                        {
                            dbTemplate.Description = "";
                        }
                        else
                        {
                            dbTemplate.Description = productTemplate.Description;
                        }
                    });


                    var checkPrice = Task.Run(() =>
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
                    });


                    var setThumbnailImage = Task.Run(async () =>
                    {
                        if (thumbnailImage != null)
                        {
                            dbTemplate.ThumbnailImage = await Ultils.UploadImage(wwwroot, "ProductTemplates/ThumnailImages", thumbnailImage, dbTemplate.ThumbnailImage);
                        }
                    });

                    var setImage = Task.Run(async () =>
                    {
                        if (images != null && images.Count > 0)
                        {
                            dbTemplate.Image = await HandleImage(dbTemplate.Image, images, wwwroot, "TemplateImages");
                        }
                    });

                    var setCoolectionImage = Task.Run(async () =>
                    {
                        if (collectionImages != null && collectionImages.Count > 0)
                        {
                            dbTemplate.CollectionImage = await HandleImage(dbTemplate.CollectionImage, collectionImages, wwwroot, "TemplateCollectionImages");
                        }
                    });

                    var setValue = Task.Run(() =>
                    {
                        dbTemplate.LastestUpdatedTime = DateTime.Now;
                        dbTemplate.InactiveTime = null;
                        dbTemplate.IsActive = false;
                    });

                    await Task.WhenAll(checkCategoryAndName, setDesc, checkPrice, setThumbnailImage, setImage, setCoolectionImage, setValue);

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

        public async Task<string> UpdateDraftTemplate(ProductTemplate productTemplate, string wwwroot, IFormFile? thumbnailImage, List<IFormFile>? newImages, List<IFormFile>? newCollectionImages)
        {

            var dbTemplate = productTemplateRepository.Get(productTemplate.Id);

            if (dbTemplate != null)
            {
                var fileDraft = Path.Combine(wwwroot, "UpdateDraft", $"{dbTemplate.Id}.json");

                var draftTemplate = new ProductTemplate();

                if (File.Exists(fileDraft))
                {
                    string jsonString = File.ReadAllText(fileDraft);

                    draftTemplate = JsonSerializer.Deserialize<ProductTemplate>(jsonString);
                }
                var checkCategoryId = Task.Run(() =>
                {
                    if (productTemplate.CategoryId == null)
                    {
                        throw new UserException("Vui lòng chọn loại danh mục");
                    }
                    else if (categoryRepository.Get(productTemplate.CategoryId) == null || categoryRepository.Get(productTemplate.CategoryId).IsActive != true)
                    {
                        throw new UserException("Loại danh mục không tồn tại");
                    }
                    else
                    {
                        dbTemplate.CategoryId = productTemplate.CategoryId;
                    }
                });

                await checkCategoryId;

                var checkName = Task.Run(() =>
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
                });

                var setDesc = Task.Run(() =>
                {
                    if (string.IsNullOrWhiteSpace(productTemplate.Description))
                    {
                        productTemplate.Description = "";
                    }
                    else
                    {
                        dbTemplate.Description = productTemplate.Description;
                    }
                });

                await checkName;

                var checkPrice = Task.Run(() =>
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
                });

                var setThumbnailImage = Task.Run(async () =>
                {
                    if (thumbnailImage != null)
                    {
                        dbTemplate.ThumbnailImage = await Ultils.UploadImage(wwwroot, "ThumnailImages", thumbnailImage, draftTemplate == null ? dbTemplate.ThumbnailImage : draftTemplate.ThumbnailImage);
                    }
                });

                var setImage = Task.Run(async () => dbTemplate.Image = await HandleImage(draftTemplate == null ? dbTemplate.Image : draftTemplate.Image, newImages, wwwroot, "TemplateImages"));

                var setCoolectionImage = Task.Run(async () => dbTemplate.CollectionImage = await HandleImage(draftTemplate == null ? dbTemplate.CollectionImage : draftTemplate.CollectionImage, newCollectionImages, wwwroot, "TemplateCollectionImages"));

                var setValue = Task.Run(() =>
                {
                    dbTemplate.LastestUpdatedTime = DateTime.Now;
                    dbTemplate.InactiveTime = null;
                    dbTemplate.IsActive = true;
                });

                await Task.WhenAll(checkCategoryId, checkName, setDesc, checkPrice, setThumbnailImage, setImage, setCoolectionImage, setValue);

                var saveDraftString = JsonSerializer.Serialize(dbTemplate);

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
                var checkCategoryId = Task.Run(() =>
                {
                    dbTemplate.CategoryId = productTemplate.CategoryId;
                });

                var checkName = Task.Run(() =>
                {
                    dbTemplate.Name = productTemplate.Name;
                    dbTemplate.UrlPath = productTemplate.UrlPath;
                });

                var setDesc = Task.Run(() =>
                {
                    dbTemplate.Description = productTemplate.Description;
                });

                var checkPrice = Task.Run(() =>
                {
                    dbTemplate.Price = productTemplate.Price;
                });

                var setThumbnailImage = Task.Run(() =>
                {
                    dbTemplate.ThumbnailImage = productTemplate.ThumbnailImage;
                });

                var setImage = Task.Run(() =>
                {
                    dbTemplate.Image = productTemplate.Image;
                });

                var setCoolectionImage = Task.Run(() =>
                {
                    dbTemplate.CollectionImage = productTemplate.CollectionImage;
                });

                var setValue = Task.Run(() =>
                {
                    dbTemplate.LastestUpdatedTime = DateTime.Now;
                    dbTemplate.InactiveTime = null;
                    dbTemplate.IsActive = true;
                });

                await Task.WhenAll(checkCategoryId, checkName, setDesc, checkPrice, setThumbnailImage, setImage, setCoolectionImage, setValue);

                return productTemplateRepository.Update(dbTemplate.Id, dbTemplate) ? productTemplate.Id : null;
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
                                })
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
                        })
                        );
                return template;
            }
            return null;
        }

        private async Task<string> HandleImage(string? dbImages, List<IFormFile>? newImages, string wwwroot, string generalPath)
        {
            if (newImages != null)
            {
                var imageNames = new List<string>();
                List<Task> tasks = new List<Task>();
                if (!string.IsNullOrWhiteSpace(dbImages))
                {
                    var existImagesDB = JsonSerializer.Deserialize<List<string>>(dbImages);
                    foreach (var existImage in existImagesDB)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            Ultils.DeleteObject(existImage);
                        }));
                    }
                }

                tasks.Add(Task.Run(async () =>
                {
                    imageNames = await Ultils.UploadImages(wwwroot, $"ProductTemplates/{generalPath}", newImages);
                }));

                await Task.WhenAll(tasks);

                dbImages = JsonSerializer.Serialize(imageNames);
            }
            else
            {

                if (!string.IsNullOrWhiteSpace(dbImages))
                {
                    var existImagesDB = JsonSerializer.Deserialize<List<string>>(dbImages);
                    var checkImages = Task.Run(async () =>
                    {
                        foreach (var existImage in existImagesDB)
                        {
                            List<Task> tasks = new List<Task>();
                            tasks.Add(Task.Run(() =>
                            {
                                Ultils.DeleteObject(existImage);
                            }));
                            await Task.WhenAll(tasks);
                        }
                    });
                    await Task.WhenAll(checkImages);
                }
                dbImages = "";
            }
            return dbImages;
        }
    }
}
