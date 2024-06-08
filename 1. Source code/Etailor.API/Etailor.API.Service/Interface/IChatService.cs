using Etailor.API.Repository.EntityModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IChatService
    {
        Task SendChat(string wwwrootPath, string orderId, string? staffId, string? customerId, string? message, List<IFormFile>? images);
        Task<Chat> GetChatDetail(string orderId, string role, string id);
    }
}
