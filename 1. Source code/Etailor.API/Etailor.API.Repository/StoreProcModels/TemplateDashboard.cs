using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Repository.StoreProcModels
{
    public class TemplateDashboard
    {
        [Key]
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? ThumbnailImage { get; set; }
        public int? Total { get; set; }
    }
}
