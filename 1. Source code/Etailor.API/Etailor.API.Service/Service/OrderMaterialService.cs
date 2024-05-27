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
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class OrderMaterialService : IOrderMaterialService
    {
        private readonly IOrderMaterialRepository orderMaterialRepository;
        private readonly IOrderRepository orderRepository;
        private readonly IMaterialRepository materialRepository;
        private readonly IMaterialCategoryRepository materialCategoryRepository;

        public OrderMaterialService(IOrderMaterialRepository orderMaterialRepository, IOrderRepository orderRepository, IMaterialRepository materialRepository, IMaterialCategoryRepository materialCategoryRepository)
        {
            this.orderMaterialRepository = orderMaterialRepository;
            this.orderRepository = orderRepository;
            this.materialRepository = materialRepository;
            this.materialCategoryRepository = materialCategoryRepository;
        }

        public async Task<bool> UpdateOrderMaterials(string orderId, List<OrderMaterial>? orderMaterials)
        {
            var order = orderRepository.Get(orderId);

            if (order != null && order.Status > 0)
            {
                switch (order.Status)
                {
                    case 5:
                        throw new UserException("Các sản phẩm của hóa đơn đã hoàn thành, không thể thay đổi");
                    case 6:
                        throw new UserException("Hóa đơn đang chờ khách kiểm thử, không thể thay đổi");
                    case 7:
                        throw new UserException("Hóa đơn đã hoàn thành, không thể thay đổi");
                }
                var dbOrderMaterials = orderMaterialRepository.GetAll(x => x.OrderId == orderId && x.IsActive == true);
                if (dbOrderMaterials != null && dbOrderMaterials.Any())
                {
                    dbOrderMaterials = dbOrderMaterials.ToList();
                }
                else
                {
                    return true;
                }

                if (orderMaterials != null && orderMaterials.Any())
                {
                    var materialIds = string.Join(",", orderMaterials.Select(x => x.MaterialId).Distinct().ToList());

                    var materials = materialRepository.GetAll(x => materialIds.Contains(x.Id));
                    if (materials != null && materials.Any())
                    {
                        materials = materials.ToList();
                    }
                    else
                    {
                        throw new UserException("Không tìm thấy vật liệu");
                    }
                    var tasks = new List<Task>();
                    var updateOrderMaterials = new List<OrderMaterial>();

                    foreach (var orderMaterial in orderMaterials)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            var material = materials.FirstOrDefault(x => x.Id == orderMaterial.MaterialId);

                            if (material == null)
                            {
                                throw new UserException("Không tìm thấy vật liệu");
                            }
                            else
                            {
                                var dbOrderMaterial = dbOrderMaterials.FirstOrDefault(x => x.MaterialId == orderMaterial.MaterialId);
                                if (dbOrderMaterial != null)
                                {
                                    dbOrderMaterial.IsCusMaterial = orderMaterial.IsCusMaterial.HasValue ? orderMaterial.IsCusMaterial.Value ? true : false : false;
                                    dbOrderMaterial.Value = orderMaterial.Value;

                                    updateOrderMaterials.Add(dbOrderMaterial);
                                }
                            }
                        }));
                    }

                    await Task.WhenAll(tasks);

                    var check = new List<bool>();
                    if (updateOrderMaterials.Any())
                    {
                        foreach (var orderMaterial in updateOrderMaterials)
                        {
                            check.Add(orderMaterialRepository.Update(orderMaterial.Id, orderMaterial));
                        }
                    }

                    return check.All(x => x == true);
                }
                else
                {
                    return true;
                }
            }
            else
            {
                throw new UserException("Không tìm thấy hóa đơn");
            }
        }

        public async Task<bool> AddCustomerMaterial(string wwwroot, string orderId, Material material, IFormFile? image)
        {
            var quantity = material.Quantity;

            if (!quantity.HasValue || quantity < 0)
            {
                throw new UserException("Số lượng nguyên vật liệu không hợp lệ");
            }

            var order = orderRepository.Get(orderId);
            if (order != null)
            {
                if (string.IsNullOrEmpty(material.Id))
                {
                    if (string.IsNullOrWhiteSpace(material.Name))
                    {
                        throw new UserException("Tên nguyên vật liệu không được để trống");
                    }

                    if (string.IsNullOrWhiteSpace(material.MaterialCategoryId))
                    {
                        throw new UserException("Danh mục nguyên vật liệu không được để trống");
                    }
                    else
                    {
                        var materialCategory = materialCategoryRepository.Get(material.MaterialCategoryId);

                        if (materialCategory == null || materialCategory.IsActive != true)
                        {
                            throw new UserException("Không tìm thấy danh mục nguyên vật liệu");
                        }
                    }

                    if (image != null)
                    {
                        material.Image = await Ultils.UploadImage(wwwroot, "Materials", image, null);
                    }
                    else
                    {
                        material.Image = "";
                    }

                    material.Id = Ultils.GenGuidString();
                    material.IsActive = true;
                    material.CreatedTime = DateTime.UtcNow.AddHours(7);
                    material.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                    material.Quantity = 0;
                    material.InactiveTime = null;

                    if (materialRepository.Create(material))
                    {
                        var orderMaterial = new OrderMaterial
                        {
                            Id = Ultils.GenGuidString(),
                            OrderId = orderId,
                            MaterialId = material.Id,
                            IsCusMaterial = true,
                            Value = quantity,
                            ValueUsed = 0,
                            CreatedTime = DateTime.UtcNow.AddHours(7),
                            LastestUpdatedTime = DateTime.UtcNow.AddHours(7),
                            InactiveTime = null,
                            Image = null,
                            IsActive = true
                        };

                        return orderMaterialRepository.Create(orderMaterial);
                    }
                    else
                    {
                        throw new SystemsException("Lỗi khi thêm nguyên vật liệu", nameof(OrderMaterialService.AddCustomerMaterial));
                    }
                }
                else
                {
                    var dbMaterial = materialRepository.Get(material.Id);
                    if (dbMaterial != null && dbMaterial.IsActive == true)
                    {
                        var orderMaterial = orderMaterialRepository.GetAll(x => x.OrderId == orderId && x.MaterialId == material.Id && x.IsActive == true)?.FirstOrDefault();

                        if (orderMaterial != null)
                        {
                            orderMaterial.Value = quantity;
                            orderMaterial.IsCusMaterial = true;

                            return orderMaterialRepository.Update(orderMaterial.Id, orderMaterial);
                        }
                        else
                        {
                            orderMaterial = new OrderMaterial
                            {
                                Id = Ultils.GenGuidString(),
                                OrderId = orderId,
                                MaterialId = material.Id,
                                IsCusMaterial = true,
                                Value = quantity,
                                ValueUsed = 0,
                                CreatedTime = DateTime.UtcNow.AddHours(7),
                                LastestUpdatedTime = DateTime.UtcNow.AddHours(7),
                                InactiveTime = null,
                                Image = null,
                                IsActive = true
                            };

                            return orderMaterialRepository.Create(orderMaterial);
                        }
                    }
                    else
                    {
                        throw new UserException("Không tìm thấy nguyên vật liệu");
                    }
                }
            }
            else
            {
                throw new UserException("Không tìm thấy hóa đơn");
            }
        }

        public List<string> GetMaterialIdOfOrder(string orderId)
        {
            var orderMaterials = orderMaterialRepository.GetAll(x => x.OrderId == orderId && x.IsActive == true);
            if (orderMaterials != null && orderMaterials.Any())
            {
                return orderMaterials.Select(x => x.MaterialId).Distinct().ToList();
            }
            else
            {
                return new List<string>();
            }
        }
    }
}
