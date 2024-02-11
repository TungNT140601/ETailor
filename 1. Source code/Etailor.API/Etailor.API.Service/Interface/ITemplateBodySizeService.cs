using Etailor.API.Repository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface ITemplateBodySizeService
    {
        Task<bool> CreateTemplateBodySize(List<string> ids, string templateId);
        Task<bool> UpdateDraftTemplateBodySize(string wwwroot, List<string> ids, string templateId);
        Task<bool> UpdateTemplateBodySize(List<TemplateBodySize> templateBodySizes, string templateId);
        bool DeleteTemplateBodySize(string id);
    }
}
