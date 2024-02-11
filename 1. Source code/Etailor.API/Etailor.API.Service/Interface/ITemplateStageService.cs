using Etailor.API.Repository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface ITemplateStageService
    {
        Task<bool> CreateTemplateStages(string templateId, List<TemplateStage> templateStages);
        List<TemplateStage> GetAll(string templateId, string? search);
        Task<bool> UpdateTemplateStages(string templateId, List<TemplateStage> inputStages,string wwwroot);
    }
}
