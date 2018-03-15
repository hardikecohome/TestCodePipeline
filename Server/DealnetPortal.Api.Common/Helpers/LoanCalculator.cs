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
            public bool IsOldClarityDeal { get; set; }
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

            public double LoanTotalCashPrice { get; set; }
        }

        public static Output Calculate(Input input)
        {            
            var output = new Output();
            output.Hst = input.TaxRate/100*input.PriceOfEquipment;
            output.PriceOfEquipmentWithHst = output.Hst + input.PriceOfEquipment;
            var customerRate = input.CustomerRate / 100 / 12;
            var mco = 0.0;
            if (input.IsClarity == true)
            {
                if (input.IsOldClarityDeal)
                {
                    output.TotalAmountFinanced = input.PriceOfEquipment /*+ input.AdminFee*/ - input.DownPayment;
                    //output.PriceOfEquipmentWithHst /*+ input.AdminFee*/ - input.DownPayment;
                    output.TotalMonthlyPayment = customerRate == 0 && input.AmortizationTerm == 0 ? 0 :
                        customerRate > 0 ? Math.Round(output.TotalAmountFinanced * Financial.Pmt(customerRate, input.AmortizationTerm, -1), 2)
                            : output.TotalAmountFinanced * Financial.Pmt(customerRate, input.AmortizationTerm, -1);
                    output.TotalAllMonthlyPayments = Math.Round(output.TotalMonthlyPayment, 2) * input.LoanTerm;
                    mco = output.TotalMonthlyPayment;
                }
                else
                {
                    const double clarityPaymentFactor = 0.010257;
                    output.TotalMonthlyPayment = output.PriceOfEquipmentWithHst;                             
                    output.PriceOfEquipmentWithHst = output.TotalMonthlyPayment / clarityPaymentFactor;
                    output.TotalAmountFinanced = output.PriceOfEquipmentWithHst + input.AdminFee - input.DownPayment;
                    mco = output.TotalAmountFinanced * clarityPaymentFactor;
                    output.TotalAllMonthlyPayments = Math.Round(mco, 2) * input.LoanTerm;                    
                }
            }
            else
            {
                output.TotalAmountFinanced = input.PriceOfEquipment /*+ input.AdminFee*/ - input.DownPayment;//output.PriceOfEquipmentWithHst /*+ input.AdminFee*/ - input.DownPayment;
                output.TotalMonthlyPayment = customerRate == 0 && input.AmortizationTerm == 0 ? 0 :
                    customerRate > 0 ? Math.Round(output.TotalAmountFinanced * Financial.Pmt(customerRate, input.AmortizationTerm, -1), 2)
                        : output.TotalAmountFinanced * Financial.Pmt(customerRate, input.AmortizationTerm, -1);
                output.TotalAllMonthlyPayments = Math.Round(output.TotalMonthlyPayment, 2) * input.LoanTerm;
                mco = output.TotalMonthlyPayment;
            }
            output.LoanTotalCashPrice = output.TotalAmountFinanced - input.AdminFee + input.DownPayment;
            if (input.LoanTerm != input.AmortizationTerm)
            {
                output.ResidualBalance = Math.Round(-Financial.PV(customerRate, input.AmortizationTerm - input.LoanTerm, mco) * (1 + customerRate),2);
            }
            output.TotalObligation = output.ResidualBalance + output.TotalAllMonthlyPayments + input.AdminFee;
            output.TotalBorowingCost = Math.Round(output.TotalObligation - output.TotalAmountFinanced - input.AdminFee,2);
            return output;
        }
    }
}
