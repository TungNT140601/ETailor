using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CustomException;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class ProductBodySizeService : IProductBodySizeService
    {
        private readonly IProductBodySizeRepository productBodySizeRepository;
        private readonly IBodySizeRepository bodySizeRepository;
        private readonly ITemplateBodySizeRepository templateBodySizeRepository;
        private readonly IProductTemplateRepository productTemplateRepository;
        private readonly IProfileBodyRepository profileBodyRepository;
        private readonly IBodyAttributeRepository bodyAttributeRepository;
        private readonly IProductRepository productRepository;

        public ProductBodySizeService(IProductBodySizeRepository productBodySizeRepository, IBodySizeRepository bodySizeRepository,
            ITemplateBodySizeRepository templateBodySizeRepository, IProductTemplateRepository productTemplateRepository,
            IProfileBodyRepository profileBodyRepository, IBodyAttributeRepository bodyAttributeRepository, IProductRepository productRepository)
        {
            this.productBodySizeRepository = productBodySizeRepository;
            this.bodySizeRepository = bodySizeRepository;
            this.templateBodySizeRepository = templateBodySizeRepository;
            this.productTemplateRepository = productTemplateRepository;
            this.profileBodyRepository = profileBodyRepository;
            this.bodyAttributeRepository = bodyAttributeRepository;
            this.productRepository = productRepository;
        }

        public async Task<bool> CreateProductBodySize(string productId, string templateId, string profileId, string cusId)
        {
            var tasks = new List<Task>();

            #region GetDataDB
            var product = productRepository.Get(productId);
            var template = productTemplateRepository.Get(templateId);
            var profile = profileBodyRepository.Get(profileId);
            var templateBodySizes = templateBodySizeRepository.GetAll(x => x.ProductTemplateId == templateId && x.IsActive == true).ToList();
            var bodyAttributes = bodyAttributeRepository.GetAll(x => x.ProfileBodyId == profileId && x.IsActive == true).ToList();
            var productBodySizes = new List<ProductBodySize>();
            #endregion

            #region CheckData
            tasks.Add(Task.Run(() =>
                {
                    if (product == null || product.IsActive == false)
                    {
                        throw new UserException("Không tìm thấy sản phẩm");
                    }
                }));

            tasks.Add(Task.Run(() =>
            {
                if (template == null || template.IsActive == false)
                {
                    throw new UserException("Không tìm thấy bản mẫu");
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                if (profile == null || profile.IsActive == false)
                {
                    throw new UserException("Không tìm thấy hồ sơ số đo");
                }
                else if (profile.CustomerId != cusId)
                {
                    throw new UserException("Hồ sơ số đo không phải của khách hàng. Vui lòng chọn lại hồ sơ số đo");
                }
                else if (string.IsNullOrEmpty(profile.StaffId))
                {
                    throw new UserException("Số đo được thực hiện bởi khách! Vui lòng kiểm tra và cập nhật lại số liệu hoặc chọn hồ sơ số đo khác được thực hiện bởi nhân viên cửa hàng");
                }
                else
                {
                    if (!bodyAttributes.Any())
                    {
                        throw new UserException("Hồ sơ số đo trống. Vui lòng đo và cập nhật cho hồ sơ");
                    }
                }
            }));
            #endregion

            await Task.WhenAll(tasks);

            if (templateBodySizes.Any() && templateBodySizes.Count > 0)
            {
                foreach (var templateBodySize in templateBodySizes)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        var cusBodySize = bodyAttributes.SingleOrDefault(x => x.BodySizeId == templateBodySize.BodySizeId);
                        if (cusBodySize == null || cusBodySize.IsActive == false || !cusBodySize.Value.HasValue || cusBodySize.Value == 0)
                        {
                            throw new UserException("Số đo cần thiết bị thiếu trong hồ sơ đo. Vui lòng đo và cập nhật bổ sung");
                        }
                        else
                        {
                            productBodySizes.Add(new ProductBodySize()
                            {
                                Id = Ultils.GenGuidString(),
                                BodySizeId = templateBodySize.BodySizeId,
                                Value = cusBodySize.Value,
                                CreatedTime = DateTime.Now,
                                InactiveTime = null,
                                IsActive = true,
                                LastestUpdatedTime = DateTime.Now,
                                ProductId = productId
                            });
                        }
                    }));
                }

                await Task.WhenAll(tasks);

                return productBodySizeRepository.CreateRange(productBodySizes);
            }
            else
            {
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateProductBodySize(string productId, string templateId, string profileId, string cusId)
        {
            var tasks = new List<Task>();

            #region GetDataDB
            var product = productRepository.Get(productId);
            var template = productTemplateRepository.Get(templateId);
            var profile = profileBodyRepository.Get(profileId);
            var templateBodySizes = templateBodySizeRepository.GetAll(x => x.ProductTemplateId == templateId && x.IsActive == true).ToList();
            var bodyAttributes = bodyAttributeRepository.GetAll(x => x.ProfileBodyId == profileId && x.IsActive == true).ToList();
            var oldProductBodySizes = productBodySizeRepository.GetAll(x => x.ProductId == productId && x.IsActive == true).ToList();
            var productBodySizes = new List<ProductBodySize>();
            var updateProductBodySizes = new List<ProductBodySize>();
            var disableProductBodySizes = new List<ProductBodySize>();
            #endregion

            #region CheckData
            tasks.Add(Task.Run(() =>
            {
                if (product == null || product.IsActive == false)
                {
                    throw new UserException("Không tìm thấy sản phẩm");
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                if (template == null || template.IsActive == false)
                {
                    throw new UserException("Không tìm thấy bản mẫu");
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                if (profile == null || profile.IsActive == false)
                {
                    throw new UserException("Không tìm thấy hồ sơ số đo");
                }
                else if (profile.CustomerId != cusId)
                {
                    throw new UserException("Hồ sơ số đo không phải của khách hàng. Vui lòng chọn lại hồ sơ số đo");
                }
                else if (string.IsNullOrEmpty(profile.StaffId))
                {
                    throw new UserException("Số đo được thực hiện bởi khách! Vui lòng kiểm tra và cập nhật lại số liệu hoặc chọn hồ sơ số đo khác được thực hiện bởi nhân viên cửa hàng");
                }
                else
                {
                    if (!bodyAttributes.Any())
                    {
                        throw new UserException("Hồ sơ số đo trống. Vui lòng đo và cập nhật cho hồ sơ");
                    }
                }
            }));
            #endregion

            await Task.WhenAll(tasks);

            if (templateBodySizes.Any() && templateBodySizes.Count > 0)
            {
                foreach (var templateBodySize in templateBodySizes)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        var oldProductBodySize = oldProductBodySizes.SingleOrDefault(x => x.BodySizeId == templateBodySize.BodySizeId);
                        if (oldProductBodySize != null)
                        {
                            var cusBodySize = bodyAttributes.SingleOrDefault(x => x.BodySizeId == templateBodySize.BodySizeId);
                            if (cusBodySize == null || cusBodySize.IsActive == false || !cusBodySize.Value.HasValue || cusBodySize.Value == 0)
                            {
                                throw new UserException("Số đo cần thiết bị thiếu trong hồ sơ đo. Vui lòng đo và cập nhật bổ sung");
                            }
                            else
                            {
                                if (oldProductBodySize.Value != cusBodySize.Value)
                                {
                                    oldProductBodySize.Value = cusBodySize.Value;
                                    oldProductBodySize.LastestUpdatedTime = DateTime.Now;

                                    updateProductBodySizes.Add(oldProductBodySize);
                                }
                            }
                        }
                        else
                        {
                            var cusBodySize = bodyAttributes.SingleOrDefault(x => x.BodySizeId == templateBodySize.BodySizeId);
                            if (cusBodySize == null || cusBodySize.IsActive == false || !cusBodySize.Value.HasValue || cusBodySize.Value == 0)
                            {
                                throw new UserException("Số đo cần thiết bị thiếu trong hồ sơ đo. Vui lòng đo và cập nhật bổ sung");
                            }
                            else
                            {
                                productBodySizes.Add(new ProductBodySize()
                                {
                                    Id = Ultils.GenGuidString(),
                                    BodySizeId = templateBodySize.BodySizeId,
                                    Value = cusBodySize.Value,
                                    CreatedTime = DateTime.Now,
                                    InactiveTime = null,
                                    IsActive = true,
                                    LastestUpdatedTime = DateTime.Now,
                                    ProductId = productId
                                });
                            }
                        }
                    }));
                }

                tasks.Add(Task.Run(() =>
                {
                    disableProductBodySizes = oldProductBodySizes.Where(x => !templateBodySizes.Select(c => c.Id).Contains(x.BodySizeId)).ToList();
                    if (disableProductBodySizes != null && disableProductBodySizes.Any() && disableProductBodySizes.Count > 0)
                    {
                        foreach (var disableProductBodySize in disableProductBodySizes)
                        {
                            disableProductBodySize.IsActive = false;
                            disableProductBodySize.InactiveTime = DateTime.Now;
                        }

                        updateProductBodySizes.AddRange(disableProductBodySizes);
                    }
                }));

                await Task.WhenAll(tasks);

                if (productBodySizeRepository.CreateRange(productBodySizes))
                {
                    var check = new List<bool>();
                    if (updateProductBodySizes.Any() && updateProductBodySizes.Count > 0)
                    {
                        foreach (var updateProductBodySize in updateProductBodySizes)
                        {
                            check.Add(productBodySizeRepository.Update(updateProductBodySize.Id, updateProductBodySize));
                        }
                        if (check.Any(c => c == false))
                        {
                            throw new SystemsException("Lỗi trong quá trình cập nhật số đo sản phẩm");
                        }
                    }
                    return true;
                }
                else
                {
                    throw new SystemsException("Lỗi trong quá trình thêm mới số đo sản phẩm");
                }
            }
            else
            {
                return true;
            }
            return false;
        }

        public bool UpdateSingle(ProductBodySize productBodySize)
        {
            var productbsDd = productBodySizeRepository.Get(productBodySize.Id);

            if (productbsDd == null || productbsDd.IsActive == false)
            {
                throw new UserException("Không tìm thấy thông tin số đo");
            }

            var bodySize = bodySizeRepository.Get(productbsDd.BodySizeId);

            if (!productBodySize.Value.HasValue || productBodySize.Value == 0 || productBodySize.Value < bodySize.MinValidValue || productBodySize.Value > bodySize.MaxValidValue)
            {
                throw new UserException($"Số liệu nhập không phù hợp. Số đo phù hợp trong khoảng ({bodySize.MinValidValue} - {bodySize.MaxValidValue})");
            }
            else
            {
                productbsDd.Value = productBodySize.Value.Value;
                productbsDd.LastestUpdatedTime = DateTime.Now;

                return productBodySizeRepository.Update(productbsDd.Id, productbsDd);
            }
        }
    }
}
