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
                .Include("~/Scripts/vendor/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").NonOrdering()
                .Include(
                "~/Scripts/vendor/jquery.validate*",
                "~/Scripts/vendor/jquery-validation-messages.js"
                //,"~/Scripts/vendor/knockout.validation.js"
                            ));

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
                    "~/Scripts/utils/localization-init.js"));

            bundles.Add(new ScriptBundle("~/bundles/dealnet")
                .Include(
                    "~/Scripts/vendor/jquery-ui-1.12.0.js",
                    "~/Scripts/vendor/jquery.tmpl.min.js",
                    "~/Scripts/vendor/jquery.placeholder.min.js",
                    "~/Scripts/vendor/svgxuse.min.js",
                    "~/Scripts/vendor/jquery.loader.js",
                    "~/Scripts/vendor/jquery.jcarousel.min.js",
                    "~/Scripts/vendor/jquery.touchSwipe.min.js",
                   // "~/Scripts/vendor/knockout-3.4.2.debug.js",
                    "~/Scripts/vendor/js.cookie.js",
                    "~/Scripts/utils/modules/index.js",
                    "~/Scripts/utils/helpPopup.js",
                    "~/Scripts/utils/alertModal.js",
                    "~/Scripts/utils/fields.js",
                    "~/Scripts/utils/modals.js",
                    "~/Scripts/utils/common.langSwitcher.js",
                    "~/Scripts/utils/scrollPosition.js",
                    "~/Scripts/utils/myDeals.counter.js",
                    "~/Scripts/utils/common.timezone.js",
                    "~/Scripts/utils/detectIE.js",
                    "~/Scripts/index.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap")
                .Include(
                    "~/Scripts/vendor/bootstrap.js",
                    "~/Scripts/vendor/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/data-tables-scripts")
                .Include(
                    "~/Scripts/vendor/dataTables/jquery.dataTables.js",
                    "~/Scripts/vendor/datatables/dataTables.bootstrap.js",
                    "~/Scripts/vendor/datatables/dataTables.responsive.js"));

            bundles.Add(new ScriptBundle("~/bundles/home-page")
                .Include(
                "~/Scripts/vendor/js.cookie.js",
                "~/Scripts/vendor/chart.js",
                "~/Scripts/vendor/jquery.form.js",
                "~/Scripts/utils/loader.js",
                "~/Scripts/utils/tables.js",
                "~/Scripts/pages/homePage/home-page.js"));

            bundles.Add(new ScriptBundle("~/bundles/basic-info")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/utils/general-address-autocomplete.js")
                .Include("~/Scripts/utils/customer-validation.js")
                .Include("~/Scripts/utils/dl-scanning.js")
                .Include("~/Scripts/utils/camera-capturing.js")
                .Include("~/Scripts/utils/dob-selecter.js")
                .Include("~/Scripts/utils/loader.js")
                .Include("~/Scripts/utils/navigateToStep.js")
                .Include("~/Scripts/utils/scrollPageTo.js")
                //.Include("~/Scripts/components/addressInformation.js")
                //.Include("~/Scripts/components/employmentInformation.js")
                .Include("~/Scripts/pages/basicInfo/additionalApplicants/employment.js")
                    .Include("~/Scripts/pages/basicInfo/homeOwner/employment.js")
                    .Include("~/Scripts/pages/basicInfo/address-autocomplete.js")
                    .Include("~/Scripts/pages/basicInfo/basicInfo.index.js"));

            bundles.Add(new ScriptBundle("~/bundles/credit-check")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/vendor/jquery.form.js")
                .Include("~/Scripts/utils/general-address-autocomplete.js")
                .Include("~/Scripts/utils/editable-in-modal.js")
                .Include("~/Scripts/utils/loader.js")
                .Include("~/Scripts/utils/navigateToStep.js")
                .Include("~/Scripts/pages/creditCheck/credit-check.js"));

            bundles.Add(new ScriptBundle("~/bundles/contact-and-payment")
                .Include("~/Scripts/utils/navigateToStep.js")
                .Include("~/Scripts/utils/custom-validation.js")
                .Include("~/Scripts/utils/contact-and-payment-management.js"));

            bundles.Add(new ScriptBundle("~/bundles/summary-and-confirmation")
                .Include("~/Scripts/vendor/jquery.form.js")
                .Include("~/Scripts/utils/custom-validation.js")
                .Include("~/Scripts/utils/province-codes-helper.js")
                .Include("~/Scripts/utils/financial-functions.js")
                .Include("~/Scripts/utils/loan-calculator.js")
                .Include("~/Scripts/utils/contact-and-payment-management.js")
                .Include("~/Scripts/utils/general-address-autocomplete.js")
                .Include("~/Scripts/utils/editable-in-modal.js")
                .Include("~/Scripts/utils/datepicker.js")
                .Include("~/Scripts/utils/navigateToStep.js")
                .Include("~/Scripts/utils/loader.js")
                .Include("~/Scripts/pages/summaryAndConfirmation/summary-and-confirmation.js")
                .Include("~/Scripts/pages/basicInfo/address-autocomplete.js"));

            bundles.Add(new ScriptBundle("~/bundles/agreement-submit-success")
                .Include("~/Scripts/vendor/jquery.form.js")
                .Include("~/Scripts/utils/print-contract.js")
                .Include("~/Scripts/pages/agrementSubmitSuccess/agreement-submit-success.js"));

            bundles.Add(new ScriptBundle("~/bundles/my-customers")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/utils/datepicker.js")
                .Include("~/Scripts/utils/tables.js")
                .Include("~/Scripts/pages/myCustomers/my-customers.js"));

            bundles.Add(new ScriptBundle("~/bundles/leads")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/utils/datepicker.js")
                .Include("~/Scripts/utils/loader.js")
                .Include("~/Scripts/utils/tables.js")
                .Include("~/Scripts/pages/leads/leads.js"));

            bundles.Add(new ScriptBundle("~/bundles/my-deals")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/vendor/js.cookie.js")
                .Include("~/Scripts/vendor/jquery.form.js")
                .Include("~/Scripts/utils/datepicker.js")
                .Include("~/Scripts/utils/loader.js")
                .Include("~/Scripts/utils/tables.js")
                .Include("~/Scripts/pages/myDeals/my-deals.js"));

            bundles.Add(new ScriptBundle("~/bundles/reports")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/utils/datepicker.js")
                .Include("~/Scripts/utils/tables.js")
                .Include("~/Scripts/pages/reports/reports.js"));

            bundles.Add(new ScriptBundle("~/bundles/report-contracts")
                .Include("~/Scripts/common/init-responsive-tabs.js")
                .Include("~/Scripts/pages/reportContracts/index.js"));

            bundles.Add(new ScriptBundle("~/bundles/contract-edit")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/vendor/jquery.form.js")
                .Include("~/Scripts/vendor/js.cookie.js")
                .Include("~/Scripts/utils/custom-validation.js")
                .Include("~/Scripts/utils/contact-and-payment-management.js")
                .Include("~/Scripts/utils/print-contract.js")
                .Include("~/Scripts/utils/general-address-autocomplete.js")
                .Include("~/Scripts/utils/editable-in-modal.js")
                .Include("~/Scripts/utils/datepicker.js")
                .Include("~/Scripts/utils/loader.js")
                .Include("~/Scripts/utils/navigateToStep.js")
                .Include("~/Scripts/utils/backToTop.js")
                .Include("~/Scripts/utils/resetPlaceholder.js")
                .Include("~/Scripts/common/init-responsive-tabs.js")
                .Include("~/Scripts/pages/aggrementSubmitSuccess/agreement-submit-success.js")
                .IncludeDirectory("~/Scripts/pages/contractEdit", "*.js", true));

            bundles.Add(new ScriptBundle("~/bundles/shareable-link")
                .Include("~/Scripts/vendor/jquery.form.js")
                .Include("~/Scripts/utils/loader.js")
                .Include("~/Scripts/pages/sharableLink/shareable-link.js"));

            bundles.Add(new StyleBundle("~/bundles/data-tables-content")
                .Include(
                    "~/Content/datatables/css/jquery.dataTables.css",
                    "~/Content/datatables/css/dataTables.bootstrap.css",
                    "~/Content/datatables/css/responsive.bootstrap.css"));

            bundles.Add(new LessBundle("~/bundles/less/main")
                    .Include("~/Content/less/main.less"));

            bundles.Add(new LessBundle("~/bundles/less/main/dealer")
                    .Include("~/Content/less/main-dealer.less"));

            bundles.Add(new ScriptBundle("~/bundles/customer-form").Include(
                "~/Scripts/vendor/datejs.js",
                "~/Scripts/utils/logger/logdebug.js",
                "~/Scripts/utils/functionUtils.js",
                "~/Scripts/utils/logMiddleware.js",
                "~/Scripts/utils/objectUtils.js",
                "~/Scripts/utils/redux.js",
                "~/Scripts/utils/dob-selecter.js")
                .Include("~/Scripts/utils/datepicker.js")
                .Include("~/Scripts/utils/panelCollapsed.js")
                .IncludeDirectory("~/Scripts/pages/customerForm", "*.js", true));

            bundles.Add(new ScriptBundle("~/bundles/new-client")
                .Include(
                "~/Scripts/vendor/datejs.js",
                "~/Scripts/utils/general-address-autocomplete.js",
                "~/Scripts/utils/dl-scanning.js",
                "~/Scripts/utils/camera-capturing.js",
                "~/Scripts/utils/logger/logdebug.js",
                "~/Scripts/utils/functionUtils.js",
                "~/Scripts/utils/logMiddleware.js",
                "~/Scripts/utils/objectUtils.js",
                "~/Scripts/utils/redux.js",
                "~/Scripts/utils/dob-selecter.js")
                .Include("~/Scripts/utils/datepicker.js")
                .Include("~/Scripts/utils/loader.js")
                .Include("~/Scripts/utils/panelCollapsed.js")
                .IncludeDirectory("~/Scripts/pages/clients", "*.js", true));

            bundles.Add(new ScriptBundle("~/bundles/my-profile")
                .Include("~/Scripts/vendor/jquery.form.js")
                .Include("~/Scripts/utils/loader.js")
                .IncludeDirectory("~/Scripts/pages/myProfile", "*.js", true)
            );

            bundles.Add(new ScriptBundle("~/bundles/equipment-info-layout")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/utils/financial-functions.js")
                .Include("~/Scripts/utils/loan-calculator.js")
                .Include("~/Scripts/utils/logger/log.js")
                .Include("~/Scripts/utils/financial-functions.module.js")
                .Include("~/Scripts/utils/datepicker.js")
                .Include("~/Scripts/utils/navigateToStep.js")
                .Include("~/Scripts/utils/setEqualHeightRows.js")
                .Include("~/Scripts/utils/resetPlaceholder.js")
                .IncludeDirectory("~/Scripts/pages/newEquipment/common", "*.js", true)
                .IncludeDirectory("~/Scripts/utils/rateCards", "*.js", true)
            );

            bundles.Add(new ScriptBundle("~/bundles/equipment-info-clarity")
                .Include("~/Scripts/pages/newEquipment/newEquipment.management.js")
                .IncludeDirectory("~/Scripts/pages/newEquipment/clarity", "*.js", true)
            );

            bundles.Add(new ScriptBundle("~/bundles/new-equipment-information")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/utils/financial-functions.js")
                .Include("~/Scripts/utils/loan-calculator.js")
                .Include("~/Scripts/utils/logger/log.js")
                .Include("~/Scripts/utils/financial-functions.module.js")
                .Include("~/Scripts/utils/datepicker.js")
                .Include("~/Scripts/utils/navigateToStep.js")
                .Include("~/Scripts/utils/setEqualHeightRows.js")
                .Include("~/Scripts/utils/resetPlaceholder.js")
                .IncludeDirectory("~/Scripts/pages/newEquipment", "*.js", true)
                .IncludeDirectory("~/Scripts/utils/rateCards", "*.js", true)
            );

            bundles.Add(new ScriptBundle("~/bundles/additional-equipment-information")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/utils/resetPlaceholder.js")
                .Include("~/Scripts/pages/additionalContractInfo/contract-information.js")
            );

            bundles.Add(new ScriptBundle("~/bundles/standalone-calculator")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/utils/financial-functions.js")
                .Include("~/Scripts/utils/financial-functions.module.js")
                .Include("~/Scripts/utils/loan-calculator.js")
                .Include("~/Scripts/utils/logger/log.js")
                .IncludeDirectory("~/Scripts/pages/calculator", "*.js", true)
                .IncludeDirectory("~/Scripts/utils/rateCards", "*.js", true));

            bundles.Add(new ScriptBundle("~/bundles/onboarding")
                .Include("~/Scripts/vendor/jquery.form.js")
                .Include("~/Scripts/utils/objectUtils.js")
                .Include("~/Scripts/utils/general-address-autocomplete.js")
                .Include("~/Scripts/utils/dob-selecter.js")
                .Include("~/Scripts/utils/datepicker.js")
                .Include("~/Scripts/utils/loader.js")
                .Include("~/Scripts/utils/clearAddress.js")
                .Include("~/Scripts/utils/panelCollapsed.js")
                .Include("~/Scripts/utils/setEqualHeightRows.js")
                .IncludeDirectory("~/Scripts/pages/onboarding", "*.js", true));
        }
    }
}
