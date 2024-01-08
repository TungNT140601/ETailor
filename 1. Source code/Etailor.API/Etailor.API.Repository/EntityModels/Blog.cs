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
        public string? Image { get; set; }
        public DateTime? CreatedTime { get; set; }
        public string? Creater { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Staff? CreaterNavigation { get; set; }
    }
}
