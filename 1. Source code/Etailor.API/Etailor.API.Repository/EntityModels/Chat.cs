using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class Chat
    {
        public Chat()
        {
            ChatLists = new HashSet<ChatList>();
        }

        public string Id { get; set; } = null!;
        public string? OrderId { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual Order? Order { get; set; }
        public virtual ICollection<ChatList> ChatLists { get; set; }
    }
}
