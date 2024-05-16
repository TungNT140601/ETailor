using Etailor.API.Repository.EntityModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IOrderMaterialService
    {
        Task<bool> UpdateOrderMaterials(string orderId, List<OrderMaterial>? orderMaterials);
        Task<bool> AddCustomerMaterial(string wwwroot, string orderId, Material material, IFormFile? image);
    }
}
