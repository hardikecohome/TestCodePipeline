using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Optimization;

namespace DealnetPortal.Web.Infrastructure.Extensions
{
    static class BundleExtentions
    {
        public static Bundle NonOrdering(this Bundle bundle)
        {
            bundle.Orderer = new NonOrderingBundleOrderer();
            return bundle;
        }
    }

    class NonOrderingBundleOrderer : IBundleOrderer
    {
        public IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
        {
            return files;
        }
    }
}
