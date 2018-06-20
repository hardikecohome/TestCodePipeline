using System;
using System.Diagnostics;

namespace DealnetPortal.DataAccess
{
    public class SecureDbContext : ApplicationDbContext
    {
        public SecureDbContext()
        {
            SetupSecureDb();
        }

        #region private
        private void SetupSecureDb()
        {
            try
            {
                Crypteron.CipherDb.Session.Create(this);
            }
            catch (Exception ex)
            {
                //Logging exception of secure context creation here
                Crypteron.ErrorHandling.Logging.Logger.Log($"Cannot create secure DB context: {ex}", TraceEventType.Error);
            }
        }
        #endregion
    }
}
