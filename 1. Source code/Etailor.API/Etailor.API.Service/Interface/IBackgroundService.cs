using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IBackgroundService
    {
        void StartSchedule(string? id);
        void StopSchedule(string? id);
    }
}
