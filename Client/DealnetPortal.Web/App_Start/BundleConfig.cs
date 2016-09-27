using System.Web;
using System.Web.Optimization;

namespace DealnetPortal.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));
            bundles.Add(new ScriptBundle("~/bundles/libs").Include(
                     "~/Scripts/chart.js",
                     "~/Scripts/DataTables/jquery.dataTables.js",
                     "~/Scripts/datatables/dataTables.bootstrap.js",
                     "~/Scripts/datatables/dataTables.responsive.js",
                     "~/Scripts/jquery.placeholder.min.js",
                     "~/Scripts/svgxuse.min.js",
                     "~/Scripts/jquery.loader.js")
                     );
            bundles.Add(new ScriptBundle("~/bundles/dealnet").Include(
                     "~/Scripts/layout.js", "~/Scripts/home-page.js", "~/Scripts/jquery-ui-1.12.0.js"));
            bundles.Add(new ScriptBundle("~/bundles/basic-info").IncludeDirectory(
      "~/Scripts/BasicInfo", "*.js").Include(
 "~/Scripts/BasicInfo/DlScanning/dl-scanning.js").Include("~/Scripts/datejs.js")
 .Include("~/Scripts/camera-capturing.js"));
            bundles.Add(new ScriptBundle("~/bundles/basic-info-mobile").IncludeDirectory(
                     "~/Scripts/BasicInfo", "*.js").Include(
                "~/Scripts/BasicInfo/DlScanning/dl-scanning-mobile.js").Include("~/Scripts/datejs.js"));
            bundles.Add(new ScriptBundle("~/bundles/credit-check").Include(
                "~/Scripts/credit-check.js").Include("~/Scripts/datejs.js").Include("~/Scripts/jquery.form.js")
                .Include("~/Scripts/general-address-autocomplete.js")
                .Include("~/Scripts/editable-in-modal.js"));

            bundles.Add(new ScriptBundle("~/bundles/summary-and-confirmation").Include(
                "~/Scripts/summary-and-confirmation.js").Include("~/Scripts/jquery.form.js")
                .Include("~/Scripts/general-address-autocomplete.js")
                .Include("~/Scripts/editable-in-modal.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(

                      "~/Content/bootstrap.css",
                      "~/Content/site.css",
                      "~/Content/datatables/css/jquery.dataTables.css",
                "~/Content/datatables/css/dataTables.bootstrap.css",
                "~/Content/datatables/css/responsive.bootstrap.css",
                      "~/Content/home-page.css", "~/Content/credit-check.css"
                      ));
        }
    }
}
