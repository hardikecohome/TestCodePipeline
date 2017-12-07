﻿using System.Web.Optimization;
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
                    "~/Scripts/utils/localization-init.js"));

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
                    "~/Scripts/vendor/dataTables/jquery.dataTables.js",
                    "~/Scripts/vendor/datatables/dataTables.bootstrap.js",
                    "~/Scripts/vendor/datatables/dataTables.responsive.js"));

            bundles.Add(new ScriptBundle("~/bundles/home-page")
                .Include("~/Scripts/utils/modules/index.js")
                .Include("~/Scripts/common/js.cookie.js")
                .Include("~/Scripts/common/common.timezone.js")
                .Include(
                    "~/Scripts/vendor/chart.js",
                    "~/Scripts/vendor/jquery.form.js",
                    "~/Scripts/home-page.js"));

            bundles.Add(new ScriptBundle("~/bundles/basic-info")
                .Include("~/Scripts/general-address-autocomplete.js")
                .Include("~/Scripts/utils/customer-validation.js")
                .IncludeDirectory("~/Scripts/basicInfo", "*.js",true)
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/utils/camera-capturing.js")
                .Include("~/Scripts/utils/modules/index.js")
                .Include("~/Scripts/utils/dob-selecter.js"));

            bundles.Add(new ScriptBundle("~/bundles/credit-check")
                .Include("~/Scripts/creditCheck/credit-check.js")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/vendor/jquery.form.js")
                .Include("~/Scripts/general-address-autocomplete.js")
                .Include("~/Scripts/utils/editable-in-modal.js"));

            bundles.Add(new ScriptBundle("~/bundles/contact-and-payment")
                .Include("~/Scripts/utils/custom-validation.js")
                .Include("~/Scripts/utils/contact-and-payment-management.js"));

            bundles.Add(new ScriptBundle("~/bundles/summary-and-confirmation")
                .Include("~/Scripts/utils/custom-validation.js")
                .Include("~/Scripts/province-codes-helper.js")
                .Include("~/Scripts/utils/financial-functions.js")
                .Include("~/Scripts/loan-calculator.js")
                .Include("~/Scripts/utils/contact-and-payment-management.js")
                .Include("~/Scripts/summary-and-confirmation.js")
                .Include("~/Scripts/vendor/jquery.form.js")
                .Include("~/Scripts/general-address-autocomplete.js")
                .Include("~/Scripts/utils/editable-in-modal.js"));

            bundles.Add(new ScriptBundle("~/bundles/agreement-submit-success")
                .Include("~/Scripts/vendor/jquery.form.js")
                .Include("~/Scripts/print-contract.js")
                .Include("~/Scripts/aggrementSubmitSuccess/agreement-submit-success.js"));

            bundles.Add(new ScriptBundle("~/bundles/my-customers")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/my-customers.js"));

            bundles.Add(new ScriptBundle("~/bundles/leads")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/leads.js"));

            bundles.Add(new ScriptBundle("~/bundles/my-deals")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/utils/modules/index.js")
                .Include("~/Scripts/common/js.cookie.js")
                .Include("~/Scripts/common/common.timezone.js")
                .Include("~/Scripts/vendor/jquery.form.js")
                .Include("~/Scripts/my-deals.js"));

            bundles.Add(new ScriptBundle("~/bundles/reports")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/reports.js"));

            bundles.Add(new ScriptBundle("~/bundles/contract-edit")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/utils/modules/index.js")
                .Include("~/Scripts/utils/custom-validation.js")
                .Include("~/Scripts/utils/contact-and-payment-management.js")
                .Include("~/Scripts/aggrementSubmitSuccess/agreement-submit-success.js")
                .Include("~/Scripts/print-contract.js")
                .Include("~/Scripts/vendor/jquery.form.js")
                .Include("~/Scripts/general-address-autocomplete.js")
                .Include("~/Scripts/utils/editable-in-modal.js")
                .Include("~/Scripts/contractEdit/contract-edit.js")
                .Include("~/Scripts/common/js.cookie.js")
                .Include("~/Scripts/common/common.timezone.js")
                .IncludeDirectory("~/Scripts/pages/contractEdit", "*.js", true));

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
                "~/Scripts/utils/modules/index.js",
                "~/Scripts/logger/logdebug.js",
                "~/Scripts/pages/customer-form.js",
                "~/Scripts/reducers/customer.js",
                "~/Scripts/actions/customer.js",
                "~/Scripts/selectors/customer.js",
                "~/Scripts/views/agreement.js",
                "~/Scripts/views/contact-info.js",
                "~/Scripts/views/your-info.js",
                "~/Scripts/views/installation-address.js",
                "~/Scripts/utils/functionUtils.js",
                "~/Scripts/utils/logMiddleware.js",
                "~/Scripts/utils/objectUtils.js",
                "~/Scripts/utils/redux.js",
                "~/Scripts/utils/dob-selecter.js"));

            bundles.Add(new ScriptBundle("~/bundles/new-client")
                .Include(
                "~/Scripts/general-address-autocomplete.js",
                "~/Scripts/basicInfo/DlScanning/dl-scanning.js",
                "~/Scripts/utils/camera-capturing.js",
                "~/Scripts/vendor/datejs.js",
                "~/Scripts/utils/modules/index.js",
                "~/Scripts/logger/logdebug.js",
                "~/Scripts/selectors/new-client-selectors.js",
                "~/Scripts/pages/Clients/index.js",
                "~/Scripts/pages/Clients/new-client-autocomplete.js",
                "~/Scripts/pages/Clients/new-client-flow.js",
                "~/Scripts/pages/Clients/new-client-store.js",
                "~/Scripts/reducers/new-client-reducer.js",
                "~/Scripts/actions/new-client-actions.js",
                "~/Scripts/views/Clients/basic-information.js",
                "~/Scripts/views/Clients/address-information.js",
                "~/Scripts/views/Clients/contact-information.js",
                "~/Scripts/views/Clients/home-improvments.js",
                "~/Scripts/views/Clients/client-consents.js",
                "~/Scripts/utils/functionUtils.js",
                "~/Scripts/utils/logMiddleware.js",
                "~/Scripts/utils/objectUtils.js",
                "~/Scripts/utils/redux.js",
                "~/Scripts/utils/dob-selecter.js"));
            
            bundles.Add(new ScriptBundle("~/bundles/my-profile")
                .Include("~/Scripts/vendor/jquery.form.js")
                .Include("~/Scripts/utils/modules/index.js")
                .Include("~/Scripts/pages/MyProfile/index.js")
                .Include("~/Scripts/pages/MyProfile/form-handlers.js")
                .Include("~/Scripts/pages/MyProfile/category-handlers.js")
                .Include("~/Scripts/pages/MyProfile/postalCode-handlers.js")
                .Include("~/Scripts/pages/MyProfile/my-profile-state.js")
                .Include("~/Scripts/pages/MyProfile/postalCode-template.js")
                .Include("~/Scripts/pages/MyProfile/category-template.js")
            );

            bundles.Add(new ScriptBundle("~/bundles/new-equipment-information")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/utils/financial-functions.js")
                .Include("~/Scripts/loan-calculator.js")
                .Include("~/Scripts/utils/modules/index.js")
                .Include("~/Scripts/logger/log.js")
                .Include("~/Scripts/utils/financial-functions.module.js")
                .Include("~/Scripts/pages/newEquipment/rate-cards.js")
                .Include("~/Scripts/pages/newEquipment/rate-cards-ui.js")
                .Include("~/Scripts/pages/newEquipment/template.js")
                .Include("~/Scripts/pages/newEquipment/equipment.js")
                .Include("~/Scripts/pages/newEquipment/value-setters.js")
                .Include("~/Scripts/pages/newEquipment/custom-rate-card.js")
                .Include("~/Scripts/pages/newEquipment/index.js")
                .Include("~/Scripts/pages/newEquipment/state.js")
                .Include("~/Scripts/pages/newEquipment/rate-cards-init.js")
                .Include("~/Scripts/pages/newEquipment/validation.js")
            );

            bundles.Add(new ScriptBundle("~/bundles/additional-equipment-information")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/pages/additionalContractInfo/contract-information.js")
            );

            bundles.Add(new ScriptBundle("~/bundles/standalone-calculator")
                .Include("~/Scripts/utils/financial-functions.js")
                .Include("~/Scripts/vendor/datejs.js")
                .Include("~/Scripts/loan-calculator.js")
                .Include("~/Scripts/utils/modules/index.js")
                .Include("~/Scripts/logger/log.js")
                .Include("~/Scripts/pages/calculator/calculator-init.js")
                .Include("~/Scripts/pages/calculator/calculator-index.js")
                .Include("~/Scripts/pages/calculator/calculator-state.js")
                .Include("~/Scripts/pages/calculator/calculator-option.js")
                .Include("~/Scripts/pages/calculator/calculator-value-setters.js")
                .Include("~/Scripts/pages/calculator/calculator-ui.js")
                .Include("~/Scripts/pages/calculator/calculator-conversion.js")
                .Include("~/Scripts/pages/calculator/calculator-jcarousel.js")
                .Include("~/Scripts/utils/financial-functions.module.js"));

            bundles.Add(new ScriptBundle("~/bundles/onboarding")
                .Include("~/Scripts/utils/modules/index.js")
                .Include("~/Scripts/utils/objectUtils.js")
                .Include("~/Scripts/general-address-autocomplete.js")
                .Include("~/Scripts/vendor/jquery.form.js")
                .IncludeDirectory("~/Scripts/pages/onboarding", "*.js", true)
                .Include("~/Scripts/utils/dob-selecter.js"));
        }
    }
}
