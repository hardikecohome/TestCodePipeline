using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Utilities.Logging;

namespace DealnetPortal.DataAccess
{
    public class SecureAppDbContext : ApplicationDbContext
    {
        private ILoggingService _loggingService;
        public SecureAppDbContext()
        {
            SetupDb();            
        }

        public SecureAppDbContext(ILoggingService loggingService)
        {
            _loggingService = loggingService;
            SetupDb();
        }

        public static SecureAppDbContext Create(ILoggingService loggingService = null)
        {
            return new SecureAppDbContext(loggingService);
        }

        private void SetupDb()
        {
            try
            {
                
                Crypteron.CipherDb.Session.Create(this);
            }
            catch (Exception ex)
            {
                //Logging exception of secure context creation here
                Crypteron.ErrorHandling.Logging.Logger.Log($"Cannot create secure DB context: {ex}", TraceEventType.Error);
                _loggingService?.LogError("Cannot create secure DB context", ex);                                
            }            
        }
    }
}
