using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IProductStageService
    {
        bool CreateProductStage();
        void SendDemoSchedule(string hourly);
    }
}
