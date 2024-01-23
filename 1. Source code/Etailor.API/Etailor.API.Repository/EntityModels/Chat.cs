using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class Chat
    {
        public Chat()
        {
            ChatHistories = new HashSet<ChatHistory>();
        }

        public string Id { get; set; } = null!;
        public string? CustomerId { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual Customer? Customer { get; set; }
        public virtual ICollection<ChatHistory> ChatHistories { get; set; }
    }
}
