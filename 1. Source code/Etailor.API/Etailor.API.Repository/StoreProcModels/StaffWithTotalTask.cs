using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Repository.StoreProcModels
{
    public class StaffWithTotalTask
    {
        public string Id { get; set; } = null!;
        public string? Avatar { get; set; }
        public string? Fullname { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public int? Role { get; set; }
        public int? TotalTask { get; set; } = 0;
    }
}
