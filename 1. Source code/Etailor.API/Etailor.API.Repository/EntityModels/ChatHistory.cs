using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class ChatHistory
    {
        public string Id { get; set; } = null!;
        public string? ChatId { get; set; }
        public string? Message { get; set; }
        public bool? FromCus { get; set; }
        public string? StaffReply { get; set; }
        public DateTime? SendTime { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? ReadTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Chat? Chat { get; set; }
        public virtual Staff? StaffReplyNavigation { get; set; }
    }
}
