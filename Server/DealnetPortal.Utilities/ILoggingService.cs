using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Utilities
{
    /// <summary>
    /// Service for logging
    /// </summary>
    public interface ILoggingService
    {
        /// <summary>
        /// Log information
        /// </summary>
        /// <param name="info">Information string</param>
        void LogInfo(string info);
        /// <summary>
        /// Log warning
        /// </summary>
        /// <param name="warning">Warning string</param>
        void LogWarning(string warning);
        /// <summary>
        /// Log error
        /// </summary>
        /// <param name="error">Error string</param>
        void LogError(string error);
        /// <summary>
        /// Log error
        /// </summary>
        /// <param name="error">Error string</param>
        /// <param name="ex">Exception</param>
        void LogError(string error, Exception ex);    
    }
}
