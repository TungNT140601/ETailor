using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class ChatList
    {
        public string Id { get; set; } = null!;
        public string? ChatId { get; set; }
        public string? ReplierId { get; set; }
        public string? Message { get; set; }
        public string? Images { get; set; }
        public bool? FromCus { get; set; } = false;
        public DateTime? SendTime { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? ReadTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual Chat? Chat { get; set; }
        public virtual Staff? Replier { get; set; }
    }
}
