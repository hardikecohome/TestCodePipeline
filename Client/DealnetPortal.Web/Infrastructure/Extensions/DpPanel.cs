using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace DealnetPortal.Web.Infrastructure.Extensions
{
    public class DpPanel : IDisposable
    {
        private readonly ViewContext _context;
        private bool _isDisposed;

        public DpPanel(ViewContext context)
        {
            _context = context;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                PanelExtentions.EndPanel(_context);
            }
        }

        public void EndPanel()
        {
            Dispose(true);
        }
    }
}