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

        public async Task<bool> AddTemplate(ProductTemplate productTemplate, string wwwroot, List<IFormFile>? images, List<IFormFile>? collectionImages)
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
            var setImage = Task.Run(() =>
            {
                if (images != null)
                {
                    var imagesUrls = Ultils.UploadImages(wwwroot, "TemplateImages", images);

                    productTemplate.Image = JsonSerializer.Serialize(imagesUrls);
                }
            });
            var setCoolectionImage = Task.Run(async () =>
            {
                if (collectionImages != null)
                {
                    var imagesUrls = await Ultils.UploadImages(wwwroot, "TemplateCollectionImages", collectionImages);

                    productTemplate.CollectionImage = JsonSerializer.Serialize(imagesUrls);
                }
            });
            var setValue = Task.Run(() =>
            {
                productTemplate.CreatedTime = DateTime.Now;
                productTemplate.LastestUpdatedTime = DateTime.Now;
                productTemplate.InactiveTime = null;
                productTemplate.IsActive = true;
            });

            await Task.WhenAll(genId, checkCategoryId, checkName, setDesc, checkPrice, setImage, setCoolectionImage, setValue);

            return productTemplateRepository.Create(productTemplate);
        }

        public async Task<bool> UpdateTemplate(ProductTemplate productTemplate, string wwwroot, List<IFormFile>? images, List<IFormFile>? collectionImages)
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
                var setImage = Task.Run(async () =>
                {
                    if (images != null)
                    {
                        var imagesUrls = await Ultils.UploadImages(wwwroot, "TemplateImages", images);
                        if (!string.IsNullOrWhiteSpace(dbTemplate.Image))
                        {
                            imagesUrls.AddRange(JsonSerializer.Deserialize<List<string>>(dbTemplate.Image));
                        }
                        dbTemplate.Image = JsonSerializer.Serialize(imagesUrls);
                    }
                });
                var setCoolectionImage = Task.Run(() =>
                {
                    if (collectionImages != null)
                    {
                        var imagesUrls = Ultils.UploadImages(wwwroot, "TemplateCollectionImages", collectionImages);

                        productTemplate.CollectionImage = JsonSerializer.Serialize(imagesUrls);
                    }
                });
                var setValue = Task.Run(() =>
                {
                    dbTemplate.LastestUpdatedTime = DateTime.Now;
                    dbTemplate.InactiveTime = null;
                    dbTemplate.IsActive = true;
                });

                await Task.WhenAll(checkCategoryId, checkName, setDesc, checkPrice, setImage, setCoolectionImage, setValue);
                return productTemplateRepository.Update(dbTemplate.Id, dbTemplate);
            }
            else
            {
                throw new UserException("Mẫu sản phẩm không tìm thấy");
            }
        }
    }
}
