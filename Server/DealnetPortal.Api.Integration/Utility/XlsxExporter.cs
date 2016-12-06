using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Domain;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace DealnetPortal.Api.Integration.Utility
{
    public static class XlsxExporter
    {
        public static void Export(IEnumerable<ContractDTO> contracts, Stream stream)
        {
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.Add("Reports");
                worksheet.Cells[1, 1].Value = "Contract #";
                worksheet.Cells[1, 2].Value = "Customer";
                worksheet.Cells[1, 3].Value = "Status";
                worksheet.Cells[1, 4].Value = "Email";
                worksheet.Cells[1, 5].Value = "Phone";
                worksheet.Cells[1, 6].Value = "Date";
                worksheet.Cells[1, 7].Value = "Equipment";
                worksheet.Cells[1, 8].Value = "Value";
                var counter = 1;
                foreach (var contract in contracts)
                {
                    counter++;
                    worksheet.Cells[counter, 1].Value = contract.Details?.TransactionId ?? contract.Id.ToString();
                    worksheet.Cells[counter, 2].Value = contract.PrimaryCustomer?.FirstName + " " + contract.PrimaryCustomer?.LastName;
                    worksheet.Cells[counter, 3].Value = contract.ContractState.GetEnumDescription();
                    worksheet.Cells[counter, 4].Value =
                        contract.PrimaryCustomer?.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress;
                    worksheet.Cells[counter, 5].Value = contract.PrimaryCustomer?.Phones?.FirstOrDefault(ph => ph.PhoneType == PhoneType.Cell)?.PhoneNum ?? contract.PrimaryCustomer?.Phones?.FirstOrDefault(ph => ph.PhoneType == PhoneType.Home)?.PhoneNum;
                    worksheet.Cells[counter, 6].Value = contract.LastUpdateTime?.ToString(CultureInfo.CurrentCulture);
                    worksheet.Cells[counter, 7].Value = contract.Equipment?.NewEquipment?.Select(eq => eq.Type).ConcatWithComma();
                    worksheet.Cells[counter, 8].Value = $"$ {contract.Equipment?.ValueOfDeal:0.00}";
                }
                worksheet.Cells[1, 1, 1, 8].Style.Font.Bold = true;
                worksheet.Cells[1, 1, 1, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                for (var i = 1; i <= 8; i++)
                {
                    worksheet.Column(i).Width = worksheet.Column(i).Width + 1;
                }
                package.Save();
            }
        }
    }
}
