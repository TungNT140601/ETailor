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

        public async Task<bool> AddTemplate(ProductTemplate productTemplate, string wwwroot, IFormFile? thumbnailImage, List<IFormFile>? images, List<IFormFile>? collectionImages)
        {
            var genId = Task.Run(() =>
            {
                productTemplate.Id = Ultils.GenGuidString();
            });

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
            });

            var checkName = Task.Run(() =>
            {
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

            var setThumbnailImage = Task.Run(async () =>
            {
                if (thumbnailImage != null)
                {
                    productTemplate.ThumbnailImage = await Ultils.UploadImage(wwwroot, "ThumnailImages", thumbnailImage, null);
                }
            });

            var setImage = Task.Run(async () =>
            {
                if (images != null && images.Count > 0)
                {
                    productTemplate.Image = await HandleImage(null, null, images, wwwroot, "TemplateImages");
                }
            });

            var setCoolectionImage = Task.Run(async () =>
            {
                if (collectionImages != null && collectionImages.Count > 0)
                {
                    productTemplate.CollectionImage = await HandleImage(null, null, collectionImages, wwwroot, "TemplateCollectionImages");
                }
            });

            var setValue = Task.Run(() =>
            {
                productTemplate.CreatedTime = DateTime.Now;
                productTemplate.LastestUpdatedTime = DateTime.Now;
                productTemplate.InactiveTime = null;
                productTemplate.IsActive = true;
            });

            await Task.WhenAll(genId, checkCategoryId, checkName, setDesc, checkPrice, setThumbnailImage, setImage, setCoolectionImage, setValue);

            return productTemplateRepository.Create(productTemplate);
        }

        public async Task<bool> UpdateTemplate(ProductTemplate productTemplate, string wwwroot, List<string>? existImages, IFormFile? thumbnailImage, List<IFormFile>? newImages, List<string>? existCollectionImages, List<IFormFile>? newCollectionImages)
        {
            var dbTemplate = productTemplateRepository.Get(productTemplate.Id);

            if (dbTemplate != null)
            {
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
                        dbTemplate.ThumbnailImage = await Ultils.UploadImage(wwwroot, "ThumnailImages", thumbnailImage, dbTemplate.ThumbnailImage);
                    }
                });

                var setImage = Task.Run(async () => dbTemplate.Image = await HandleImage(dbTemplate.Image, existImages, newImages, wwwroot, "TemplateImages"));

                var setCoolectionImage = Task.Run(async () => dbTemplate.CollectionImage = await HandleImage(dbTemplate.CollectionImage, existCollectionImages, newCollectionImages, wwwroot, "TemplateCollectionImages"));

                var setValue = Task.Run(() =>
                {
                    dbTemplate.LastestUpdatedTime = DateTime.Now;
                    dbTemplate.InactiveTime = null;
                    dbTemplate.IsActive = true;
                });

                await Task.WhenAll(checkCategoryId, checkName, setDesc, checkPrice, setThumbnailImage, setImage, setCoolectionImage, setValue);
                return productTemplateRepository.Update(dbTemplate.Id, dbTemplate);
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

        private async Task<string> HandleImage(string? dbImages, List<string>? existImages, List<IFormFile>? newImages, string wwwroot, string generalPath)
        {
            if (newImages != null)
            {
                var existLinkPayload = new List<string>();
                if (existImages != null && existImages.Count > 0)
                {
                    existLinkPayload = existImages.Select(c => c.Contains("%2F") ? c.Replace("%2F", "/") : c).ToList();

                    if (!string.IsNullOrWhiteSpace(dbImages))
                    {
                        var existImagesDB = JsonSerializer.Deserialize<List<string>>(dbImages);
                        var checkImages = Task.Run(async () =>
                        {
                            List<Task> tasks = new List<Task>();
                            foreach (var existImage in existImagesDB)
                            {
                                tasks.Add(Task.Run(() =>
                                {
                                    if (!existImages.Contains(existImage))
                                    {
                                        Ultils.DeleteObject(existImage);
                                        existImagesDB.Remove(existImage);
                                    }
                                }));
                            }
                            await Task.WhenAll(tasks);
                        });

                        await Task.WhenAll(checkImages);

                        existImagesDB.AddRange(await Ultils.UploadImages(wwwroot, generalPath, newImages));

                        dbImages = JsonSerializer.Serialize(existImagesDB);
                    }
                    else
                    {
                        dbImages = JsonSerializer.Serialize(await Ultils.UploadImages(wwwroot, generalPath, newImages));
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(dbImages))
                    {
                        var existImagesDB = JsonSerializer.Deserialize<List<string>>(dbImages);
                        var checkImages = Task.Run(async () =>
                        {
                            List<Task> tasks = new List<Task>();
                            foreach (var existImage in existImagesDB)
                            {
                                tasks.Add(Task.Run(() =>
                                {
                                    Ultils.DeleteObject(existImage);
                                    existImagesDB.Remove(existImage);
                                }));
                            }
                            await Task.WhenAll(tasks);
                        });

                        await Task.WhenAll(checkImages);

                        existImagesDB.AddRange(await Ultils.UploadImages(wwwroot, generalPath, newImages));

                        dbImages = JsonSerializer.Serialize(existImagesDB);
                    }
                    else
                    {
                        dbImages = JsonSerializer.Serialize(await Ultils.UploadImages(wwwroot, generalPath, newImages));
                    }
                }
            }
            else
            {
                var existLinkPayload = new List<string>();
                if (existImages != null && existImages.Count > 0)
                {
                    existLinkPayload = existImages.Select(c => c.Contains("%2F") ? c.Replace("%2F", "/") : c).ToList();

                    if (!string.IsNullOrWhiteSpace(dbImages))
                    {
                        var existImagesDB = JsonSerializer.Deserialize<List<string>>(dbImages);
                        var checkImages = Task.Run(async () =>
                        {
                            List<Task> tasks = new List<Task>();
                            foreach (var existImage in existImagesDB)
                            {
                                tasks.Add(Task.Run(() =>
                                {
                                    if (!existImages.Contains(existImage))
                                    {
                                        Ultils.DeleteObject(existImage);
                                        existImagesDB.Remove(existImage);
                                    }
                                }));
                            }
                            await Task.WhenAll(tasks);
                        });
                        await Task.WhenAll(checkImages);

                        dbImages = JsonSerializer.Serialize(existImagesDB);
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(dbImages))
                    {
                        var existImagesDB = JsonSerializer.Deserialize<List<string>>(dbImages);
                        var checkImages = Task.Run(async () =>
                        {
                            List<Task> tasks = new List<Task>();
                            foreach (var existImage in existImagesDB)
                            {
                                tasks.Add(Task.Run(() =>
                                {
                                    Ultils.DeleteObject(existImage);
                                    existImagesDB.Remove(existImage);
                                }));
                            }
                            await Task.WhenAll(tasks);
                        });

                        await Task.WhenAll(checkImages);

                        dbImages = JsonSerializer.Serialize(existImagesDB);
                    }
                }
            }
            return dbImages;
        }
    }
}
