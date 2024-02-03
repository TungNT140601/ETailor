using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class Blog
    {
        public string Id { get; set; } = null!;
        public string? Title { get; set; }
        public string? UrlPath { get; set; }
        public string? Content { get; set; }
        public string? Hastag { get; set; }
        public DateTime? CreatedTime { get; set; }
        public string? StaffId { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual Staff? Staff { get; set; }
    }
}
