using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity.CustomException;
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

        public OrderMaterialService(IOrderMaterialRepository orderMaterialRepository, IOrderRepository orderRepository, IMaterialRepository materialRepository)
        {
            this.orderMaterialRepository = orderMaterialRepository;
            this.orderRepository = orderRepository;
            this.materialRepository = materialRepository;
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
    }
}
