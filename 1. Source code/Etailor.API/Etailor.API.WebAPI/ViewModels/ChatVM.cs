using Etailor.API.Repository.EntityModels;

namespace Etailor.API.WebAPI.ViewModels
{
    public class ChatVM
    {
        public string? Message { get; set; }
        public List<IFormFile>? MessageImages { get; set; }
    }
    public class ChatAllVM
    {
        public string? Id { get; set; }
        public string? OrderId { get; set; }
        public DateTime? CreatedTime { get; set; }
        public virtual ICollection<ChatListVM> ChatLists { get; set; }
    }
    public class ChatListVM
    {
        public string? Id { get; set; }
        public string? ChatId { get; set; }
        public string? ReplierId { get; set; }
        public string? Message { get; set; }
        public string? Images { get; set; }
        public bool? FromCus { get; set; } = false;
        public DateTime? SendTime { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? ReadTime { get; set; }
    }
}
