using System.Web.Optimization;
using DealnetPortal.Web.Infrastructure.Extensions;

namespace DealnetPortal.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery")
                .Include(
                    "~/Scripts/vendor/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").NonOrdering()
                .Include("~/Scripts/vendor/jquery.validate*")
                .Include("~/Scripts/vendor/jquery-validation-messages.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval-msg-fr")
                .Include("~/Scripts/vendor/jquery-validate-messages-fr.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr")
                .Include("~/Scripts/vendor/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/localization")
                .Include(
                    "~/Scripts/vendor/cldr/cldr.js",
                    "~/Scripts/vendor/cldr/cldr/event.js",
                    "~/Scripts/vendor/cldr/cldr/supplemental.js",
                    "~/Scripts/vendor/globalize/globalize.js",
                    "~/Scripts/vendor/globalize/globalize/number.js",
                    "~/Scripts/vendor/globalize/globalize/currency.js",
                    "~/Scripts/Utils/localization-init.js"));

            bundles.Add(new ScriptBundle("~/bundles/dealnet")
                .Include(
                    "~/Scripts/vendor/jquery.tmpl.min.js",
                    "~/Scripts/vendor/jquery.placeholder.min.js",
                    "~/Scripts/vendor/svgxuse.min.js",
                    "~/Scripts/vendor/jquery.loader.js",
                    "~/Scripts/vendor/slick.min.js",
                    "~/Scripts/vendor/jquery.jcarousel.min.js",
                    "~/Scripts/vendor/jquery.touchSwipe.min.js",
                    "~/Scripts/layout.js",
                    "~/Scripts/vendor/jquery-ui-1.12.0.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap")
                .Include(
                    "~/Scripts/vendor/bootstrap.js",
                    "~/Scripts/vendor/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/data-tables-scripts")
                .Include(
                    "~/Scripts/vendor/DataTables/jquery.dataTables.js",
                    "~/Scripts/vendor/datatables/dataTables.bootstrap.js",
                    "~/Scripts/vendor/datatables/dataTables.responsive.js"));

            bundles.Add(new ScriptBundle("~/bundles/home-page")
                .Include("~/Scripts/Modules/index.js")
                .Include("~/Scripts/common/js.cookie.js")
                .Include("~/Scripts/common/common.timezone.js")
                .Include(
                    "~/Scripts/vendor/chart.js",
                    "~/Scripts/vendor/jquery.form.js",
                    "~/Scripts/home-page.js"));

            bundles.Add(new ScriptBundle("~/bundles/basic-info")
                .Include("~/Scripts/general-address-autocomplete.js")
                .Include("~/Scripts/customer-validation.js")
                .IncludeDirectory("~/Scripts/BasicInfo", "*.js")
                .Include("~/Scripts/BasicInfo/DlScanning/dl-scanning.js")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/camera-capturing.js")
                .Include("~/Scripts/Modules/index.js")
                .Include("~/Scripts/Utils/dob-selecter.js"));

            bundles.Add(new ScriptBundle("~/bundles/credit-check")
                .Include("~/Scripts/credit-check.js")
                .Include("~/Scripts/datejs.js")
                .Include("~/Scripts/vendor/jquery.form.js")
                .Include("~/Scripts/general-address-autocomplete.js")
                .Include("~/Scripts/editable-in-modal.js"));

            //bundles.Add(new ScriptBundle("~/bundles/equipment-information")
            //    .Include("~/Scripts/financial-functions.js")
            //    .Include("~/Scripts/loan-calculator.js")
            //    .Include("~/Scripts/equipment-information.js")
            //    .Include("~/Scripts/datejs.js")
            //    .Include("~/Scripts/navigation-with-warning.js"));

            bundles.Add(new ScriptBundle("~/bundles/contact-and-payment")
                .Include("~/Scripts/custom-validation.js")
                .Include("~/Scripts/contact-and-payment-management.js")
                .Include("~/Scripts/navigation-with-warning.js"));

            bundles.Add(new ScriptBundle("~/bundles/summary-and-confirmation")
                .Include("~/Scripts/custom-validation.js")
                .Include("~/Scripts/province-codes-helper.js")
                .Include("~/Scripts/financial-functions.js")
                .Include("~/Scripts/loan-calculator.js")
                .Include("~/Scripts/contact-and-payment-management.js")
                .Include("~/Scripts/summary-and-confirmation.js")
                .Include("~/Scripts/vendor/jquery.form.js")
                .Include("~/Scripts/general-address-autocomplete.js")
                .Include("~/Scripts/editable-in-modal.js")
                .Include("~/Scripts/navigation-with-warning.js"));

            bundles.Add(new ScriptBundle("~/bundles/agreement-submit-success")
                .Include("~/Scripts/vendor/jquery.form.js")
                .Include("~/Scripts/print-contract.js")
                .Include("~/Scripts/agreement-submit-success.js"));

            bundles.Add(new ScriptBundle("~/bundles/my-customers")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/my-customers.js"));

            bundles.Add(new ScriptBundle("~/bundles/leads")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/leads.js"));

            bundles.Add(new ScriptBundle("~/bundles/my-deals")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/Modules/index.js")
                .Include("~/Scripts/common/js.cookie.js")
                .Include("~/Scripts/common/common.timezone.js")
                .Include("~/Scripts/vendor/jquery.form.js")
                .Include("~/Scripts/my-deals.js"));

            bundles.Add(new ScriptBundle("~/bundles/reports")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/reports.js"));

            bundles.Add(new ScriptBundle("~/bundles/contract-edit")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/Modules/index.js")
                .Include("~/Scripts/custom-validation.js")
                .Include("~/Scripts/contact-and-payment-management.js")
                .Include("~/Scripts/agreement-submit-success.js")
                .Include("~/Scripts/print-contract.js")
                .Include("~/Scripts/vendor/jquery.form.js")
                .Include("~/Scripts/general-address-autocomplete.js")
                .Include("~/Scripts/editable-in-modal.js")
                .Include("~/Scripts/contract-edit.js")
                .Include("~/Scripts/common/js.cookie.js")
                .Include("~/Scripts/common/common.timezone.js")
                .IncludeDirectory("~/Scripts/Pages/contractEdit", "*.js", true));

            bundles.Add(new ScriptBundle("~/bundles/shareable-link")
                .Include("~/Scripts/vendor/jquery.form.js")
                .Include("~/Scripts/shareable-link.js"));

            bundles.Add(new StyleBundle("~/bundles/data-tables-content")
                .Include(
                    "~/Content/datatables/css/jquery.dataTables.css",
                    "~/Content/datatables/css/dataTables.bootstrap.css",
                    "~/Content/datatables/css/responsive.bootstrap.css"));

            bundles.Add(new ScriptBundle("~/bundles/customer-form").Include(
                "~/Scripts/vendor/datejs.js",
                "~/Scripts/Modules/index.js",
                "~/Scripts/Logger/logdebug.js",
                "~/Scripts/Pages/customer-form.js",
                "~/Scripts/Reducers/customer.js",
                "~/Scripts/Actions/customer.js",
                "~/Scripts/Selectors/customer.js",
                "~/Scripts/Views/agreement.js",
                "~/Scripts/Views/contact-info.js",
                "~/Scripts/Views/your-info.js",
                "~/Scripts/Views/installation-address.js",
                "~/Scripts/Utils/functionUtils.js",
                "~/Scripts/Utils/logMiddleware.js",
                "~/Scripts/Utils/objectUtils.js",
                "~/Scripts/Utils/redux.js",
                "~/Scripts/Utils/dob-selecter.js"));

            bundles.Add(new ScriptBundle("~/bundles/new-client")
                .Include(
                "~/Scripts/general-address-autocomplete.js",
                "~/Scripts/BasicInfo/DlScanning/dl-scanning.js",
                "~/Scripts/camera-capturing.js",
                "~/Scripts/vendor/datejs.js",
                "~/Scripts/Modules/index.js",
                "~/Scripts/Logger/logdebug.js",
                "~/Scripts/Selectors/new-client-selectors.js",
                "~/Scripts/Pages/Clients/index.js",
                "~/Scripts/Pages/Clients/new-client-autocomplete.js",
                "~/Scripts/Pages/Clients/new-client-flow.js",
                "~/Scripts/Pages/Clients/new-client-store.js",
                "~/Scripts/Reducers/new-client-reducer.js",
                "~/Scripts/Actions/new-client-actions.js",
                "~/Scripts/Views/Clients/basic-information.js",
                "~/Scripts/Views/Clients/address-information.js",
                "~/Scripts/Views/Clients/contact-information.js",
                "~/Scripts/Views/Clients/home-improvments.js",
                "~/Scripts/Views/Clients/client-consents.js",
                "~/Scripts/Utils/functionUtils.js",
                "~/Scripts/Utils/logMiddleware.js",
                "~/Scripts/Utils/objectUtils.js",
                "~/Scripts/Utils/redux.js",
                "~/Scripts/Utils/dob-selecter.js"));

            //bundles.Add(new ScriptBundle("~/bundles/new-client-mobile")
            //    .Include(
            //        "~/Scripts/general-address-autocomplete.js",
            //        "~/Scripts/BasicInfo/DlScanning/dl-scanning-mobile.js",
            //        "~/Scripts/camera-capturing.js",
            //        "~/Scripts/datejs.js",
            //        "~/Scripts/Modules/index.js",
            //        "~/Scripts/Logger/logdebug.js",
            //        "~/Scripts/Selectors/new-client-selectors.js",
            //        "~/Scripts/Pages/Clients/index.js",
            //        "~/Scripts/Pages/Clients/new-client-autocomplete.js",
            //        "~/Scripts/Pages/Clients/new-client-flow.js",
            //        "~/Scripts/Pages/Clients/new-client-store.js",
            //        "~/Scripts/Reducers/new-client-reducer.js",
            //        "~/Scripts/Actions/new-client-actions.js",
            //        "~/Scripts/Views/Clients/basic-information.js",
            //        "~/Scripts/Views/Clients/address-information.js",
            //        "~/Scripts/Views/Clients/contact-information.js",
            //        "~/Scripts/Views/Clients/home-improvments.js",
            //        "~/Scripts/Views/Clients/client-consents.js",
            //        "~/Scripts/Utils/functionUtils.js",
            //        "~/Scripts/Utils/logMiddleware.js",
            //        "~/Scripts/Utils/objectUtils.js",
            //        "~/Scripts/Utils/redux.js"));
            bundles.Add(new ScriptBundle("~/bundles/my-profile")
                .Include("~/Scripts/vendor/jquery.form.js")
                .Include("~/Scripts/Modules/index.js")
                .Include("~/Scripts/Pages/MyProfile/index.js")
                .Include("~/Scripts/Pages/MyProfile/form-handlers.js")
                .Include("~/Scripts/Pages/MyProfile/category-handlers.js")
                .Include("~/Scripts/Pages/MyProfile/postalCode-handlers.js")
                .Include("~/Scripts/Pages/MyProfile/my-profile-state.js")
                .Include("~/Scripts/Pages/MyProfile/postalCode-template.js")
                .Include("~/Scripts/Pages/MyProfile/category-template.js")
            );

            bundles.Add(new ScriptBundle("~/bundles/new-equipment-information")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/financial-functions.js")
                .Include("~/Scripts/loan-calculator.js")
                .Include("~/Scripts/Modules/index.js")
                .Include("~/Scripts/Logger/log.js")
                .Include("~/Scripts/Utils/financial-functions.js")
                .Include("~/Scripts/Pages/newEquipment/rate-cards.js")
                .Include("~/Scripts/Pages/newEquipment/rate-cards-ui.js")
                .Include("~/Scripts/Pages/newEquipment/template.js")
                .Include("~/Scripts/Pages/newEquipment/equipment.js")
                .Include("~/Scripts/Pages/newEquipment/value-setters.js")
                .Include("~/Scripts/Pages/newEquipment/custom-rate-card.js")
                .Include("~/Scripts/Pages/newEquipment/index.js")
                .Include("~/Scripts/Pages/newEquipment/state.js")
                .Include("~/Scripts/Pages/newEquipment/rate-cards-init.js")
                .Include("~/Scripts/Pages/newEquipment/validation.js")
            );

            bundles.Add(new ScriptBundle("~/bundles/additional-equipment-information")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/Pages/additionalContractInfo/contract-information.js")
            );

            bundles.Add(new ScriptBundle("~/bundles/standalone-calculator")
                .Include("~/Scripts/financial-functions.js")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/loan-calculator.js")
                .Include("~/Scripts/Modules/index.js")
                .Include("~/Scripts/Logger/log.js")
                .Include("~/Scripts/Pages/calculator/calculator-init.js")
                .Include("~/Scripts/Pages/calculator/calculator-index.js")
                .Include("~/Scripts/Pages/calculator/calculator-state.js")
                .Include("~/Scripts/Pages/calculator/calculator-option.js")
                .Include("~/Scripts/Pages/calculator/calculator-value-setters.js")
                .Include("~/Scripts/Pages/calculator/calculator-ui.js")
                .Include("~/Scripts/Pages/calculator/calculator-conversion.js")
                .Include("~/Scripts/Pages/calculator/calculator-jcarousel.js")
                .Include("~/Scripts/Utils/financial-functions.js"));

            bundles.Add(new ScriptBundle("~/bundles/onboarding")
                .Include("~/Scripts/Modules/index.js")
                .Include("~/Scripts/Utils/objectUtils.js")
                .Include("~/Scripts/general-address-autocomplete.js")
                .Include("~/Scripts/vendor/jquery.form.js")
                .IncludeDirectory("~/Scripts/Pages/onboarding", "*.js", true)
                .Include("~/Scripts/Utils/dob-selecter.js"));
        }
    }
}
