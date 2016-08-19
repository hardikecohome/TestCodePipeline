using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Utilities
{
    public interface ILoggingService
    {
        void LogInfo(string info);
        void LogWarning(string warning);
        void LogError(string error);
        void LogError(string error, Exception ex);    
    }
}
