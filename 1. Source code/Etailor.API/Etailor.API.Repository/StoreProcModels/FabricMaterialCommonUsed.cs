using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Repository.StoreProcModels
{
    public class FabricMaterialCommonUsed
    {
        [Key]
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }
        public int? TotalProducts { get; set; }
        public int? TotalOrders { get; set;}
    }
}
