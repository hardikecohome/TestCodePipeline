using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace DealnetPortal.Api.Common.Helpers
{
    public static class LoanCalculator
    {
        public class Input
        {
            public double TaxRate { get; set; } 
            public int LoanTerm { get; set; }
            public int AmortizationTerm { get; set; }
            public double CustomerRate { get; set; }
            public double EquipmentCashPrice { get; set; }
            public double AdminFee { get; set; }
            public double DownPayment { get; set; }
        }

        public class Output
        {
            public double Hst { get; set; }
            public double TotalCashPrice { get; set; }
            public double TotalAmountFinanced { get; set; }
            //TODO: why double, not decimal?
            public double TotalMonthlyPayment { get; set; }
            public double TotalAllMonthlyPayments { get; set; }
            public double ResidualBalance { get; set; }
            public double TotalObligation { get; set; }
            public double TotalBorowingCost { get; set; }
        }

        public static Output Calculate(Input input)
        {
            var output = new Output();
            output.Hst = input.TaxRate/100*input.EquipmentCashPrice;
            output.TotalCashPrice = output.Hst + input.EquipmentCashPrice;
            output.TotalAmountFinanced = output.TotalCashPrice + input.AdminFee - input.DownPayment;
            output.TotalMonthlyPayment = output.TotalAmountFinanced*Financial.Pmt(input.CustomerRate/100/12, input.AmortizationTerm, -1);
            output.TotalAllMonthlyPayments = output.TotalMonthlyPayment*input.LoanTerm;
            if (input.LoanTerm != input.AmortizationTerm)
            {
                output.ResidualBalance = -Financial.PV(input.CustomerRate / 100 / 12, input.AmortizationTerm - input.LoanTerm, output.TotalMonthlyPayment) * (1 + input.CustomerRate/100/12);
            }
            output.TotalObligation = output.ResidualBalance + output.TotalAllMonthlyPayments;
            output.TotalBorowingCost = output.TotalObligation - output.TotalAmountFinanced;
            return output;
        }
    }
}
