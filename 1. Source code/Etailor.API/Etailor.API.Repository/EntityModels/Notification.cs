using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class Notification
    {
        public string Id { get; set; } = null!;
        public string? CustomerId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public DateTime? SendTime { get; set; }
        public DateTime? ReadTime { get; set; }
        public bool? IsRead { get; set; }
        public bool? IsActive { get; set; }

        public virtual Customer? Customer { get; set; }
    }
}
