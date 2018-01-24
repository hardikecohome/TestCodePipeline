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
            //Price of equipment(s) excl tax 
            public double PriceOfEquipment { get; set; }
            public double AdminFee { get; set; }
            public double DownPayment { get; set; }
        }

        public class Output
        {
            public double Hst { get; set; }
            //Price of equipment (incl tax)
            public double PriceOfEquipmentWithHst { get; set; }
            public double TotalAmountFinanced { get; set; }
            //TODO: why double, not decimal?
            //TODO: because methods from Financial class outputs value in double
            public double TotalMonthlyPayment { get; set; }
            public double TotalAllMonthlyPayments { get; set; }
            public double ResidualBalance { get; set; }
            public double TotalObligation { get; set; }
            public double TotalBorowingCost { get; set; }
        }

        public static Output Calculate(Input input)
        {
            var output = new Output();
            output.Hst = input.TaxRate/100*input.PriceOfEquipment;
            output.PriceOfEquipmentWithHst = output.Hst + input.PriceOfEquipment;
            output.TotalAmountFinanced = output.PriceOfEquipmentWithHst /*+ input.AdminFee*/ - input.DownPayment;
            output.TotalMonthlyPayment = input.CustomerRate == 0 && input.AmortizationTerm == 0 ? 0 :
                input.CustomerRate > 0 ? Math.Round(output.TotalAmountFinanced * Financial.Pmt(input.CustomerRate / 100 / 12, input.AmortizationTerm, -1), 2)
                : output.TotalAmountFinanced * Financial.Pmt(input.CustomerRate / 100 / 12, input.AmortizationTerm, -1);
            output.TotalAllMonthlyPayments = Math.Round(output.TotalMonthlyPayment*input.LoanTerm,2);
            if (input.LoanTerm != input.AmortizationTerm)
            {
                output.ResidualBalance = Math.Round(-Financial.PV(input.CustomerRate / 100 / 12, input.AmortizationTerm - input.LoanTerm, output.TotalMonthlyPayment) * (1 + input.CustomerRate/100/12),2);
            }
            output.TotalObligation = output.ResidualBalance + output.TotalAllMonthlyPayments + input.AdminFee;
            output.TotalBorowingCost = Math.Round(output.TotalObligation - output.TotalAmountFinanced - input.AdminFee,2);
            return output;
        }
    }
}
