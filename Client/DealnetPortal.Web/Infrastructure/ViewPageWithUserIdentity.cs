using System.Web.Mvc;
using DealnetPortal.Web.Common.Security;

namespace DealnetPortal.Web.Infrastructure
{
    public abstract class ViewPageWithUserIdentity : WebViewPage
    {
        public virtual new UserPrincipal User => base.User as UserPrincipal;
    }

    public abstract class BaseViewPage<TModel> : WebViewPage<TModel>
    {
        public virtual new UserPrincipal User => base.User as UserPrincipal;
    }
}
