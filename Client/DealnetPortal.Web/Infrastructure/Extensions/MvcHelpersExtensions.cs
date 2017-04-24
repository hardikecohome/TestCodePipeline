using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Web.Infrastructure.Extensions
{
    public static class MvcHelpersExtensions
    {
        public static Dictionary<RateCardType, SelectList> ConvertToAmortizationSelectList(this List<RateCardDTO> list)
        {
            var result = list
                .GroupBy(type => type.CardType, val => Convert.ToInt32(val.AmortizationTerm))
                .ToDictionary(k => k.Key, v =>
                {
                    var distincted = v.Distinct().Select(value => new SelectListItem { Value = value.ToString(), Text = $"{value} / {value}"});

                    return new SelectList(distincted, "Value", "Text");
                });

            return result;
        }

        public static Dictionary<RateCardType, SelectList> ConvertToDeferralSelectList(this List<RateCardDTO> list)
        {
            var result = list
                .GroupBy(type => type.CardType, val => Convert.ToInt32(val.DeferralPeriod))
                .ToDictionary(k => k.Key, v =>
                {
                    var distincted = v.Distinct().Select(value => new SelectListItem { Value = value.ToString(), Text = string.Format(Resources.Resources.FormatMonth, value) });

                    return new SelectList(distincted, "Value", "Text");
                });

            return result;
        }
    }
}