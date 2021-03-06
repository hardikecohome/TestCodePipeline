﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Models;

namespace DealnetPortal.Web.Infrastructure.Extensions
{
    public static class MvcHelpersExtensions
    {
        public static Dictionary<RateCardType, SelectList> ConvertToAmortizationSelectList(this List<RateCardViewModel> list)
        {
            var result = list
                .GroupBy(type => type.CardType, val => new { Amortization = Convert.ToInt32(val.AmortizationTerm), Loan = Convert.ToInt32(val.LoanTerm) })
                .ToDictionary(k => k.Key, v =>
                {
                    var distincted = v.Distinct().Select(value => new SelectListItem { Value = value.Amortization.ToString(), Text = $"{value.Loan} / {value.Amortization}" });

                    return new SelectList(distincted, "Value", "Text");
                });

            return result;
        }

        public static Dictionary<RateCardType, SelectList> ConvertToDeferralSelectList(this List<RateCardViewModel> list)
        {
            var result = list
                .GroupBy(type => type.CardType, val => Convert.ToInt32(val.DeferralPeriod))
                .ToDictionary(k => k.Key, v =>
                {
                    var distincted = v.Distinct().Select(value => new SelectListItem { Value = value.ToString(), Text = value == 0 ? Resources.Resources.Promo : value + " " + (value == 1 ? Resources.Resources.Month : Resources.Resources.Months) });

                    return new SelectList(distincted, "Value", "Text");
                });

            return result;
        }
    }
}