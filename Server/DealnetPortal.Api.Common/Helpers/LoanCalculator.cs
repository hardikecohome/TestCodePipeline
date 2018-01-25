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
            //can be as input data (for clarity program)
            public bool? IsClarity { get; set; }
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
            var customerRate = input.CustomerRate / 100 / 12;
            if (input.IsClarity == true)
            {
                output.TotalMonthlyPayment = output.PriceOfEquipmentWithHst;
                output.TotalAmountFinanced = customerRate == 0.0 ? 0.0 :
                    (1.0 - Math.Pow(1 + customerRate,
                                   -input.AmortizationTerm)) / customerRate;
                output.TotalAmountFinanced *= output.TotalMonthlyPayment;
                output.PriceOfEquipmentWithHst = output.TotalAmountFinanced - input.AdminFee + input.DownPayment;
            }
            else
            {
                output.TotalAmountFinanced = input.PriceOfEquipment /*+ input.AdminFee*/ - input.DownPayment;//output.PriceOfEquipmentWithHst /*+ input.AdminFee*/ - input.DownPayment;
                output.TotalMonthlyPayment = customerRate == 0 && input.AmortizationTerm == 0 ? 0 :
                    customerRate > 0 ? Math.Round(output.TotalAmountFinanced * Financial.Pmt(customerRate, input.AmortizationTerm, -1), 2)
                        : output.TotalAmountFinanced * Financial.Pmt(customerRate, input.AmortizationTerm, -1);
            }            
            output.TotalAllMonthlyPayments = Math.Round(output.TotalMonthlyPayment*input.LoanTerm,2);
            if (input.LoanTerm != input.AmortizationTerm)
            {
                output.ResidualBalance = Math.Round(-Financial.PV(customerRate, input.AmortizationTerm - input.LoanTerm, output.TotalMonthlyPayment) * (1 + customerRate),2);
            }
            output.TotalObligation = output.ResidualBalance + output.TotalAllMonthlyPayments + input.AdminFee;
            output.TotalBorowingCost = Math.Round(output.TotalObligation - output.TotalAmountFinanced - input.AdminFee,2);
            return output;
        }
    }
}
