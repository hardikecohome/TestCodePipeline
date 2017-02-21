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
                var worksheet = package.Workbook.Worksheets.Add(Resources.Resources.Reports);
                worksheet.Cells[1, 1].Value = Resources.Resources.Contract + " #";
                worksheet.Cells[1, 2].Value = Resources.Resources.Customer;
                worksheet.Cells[1, 3].Value = Resources.Resources.Status;
                worksheet.Cells[1, 4].Value = Resources.Resources.Type;
                worksheet.Cells[1, 5].Value = Resources.Resources.Email;
                worksheet.Cells[1, 6].Value = Resources.Resources.Phone;
                worksheet.Cells[1, 7].Value = Resources.Resources.Date;
                worksheet.Cells[1, 8].Value = Resources.Resources.Equipment;
                worksheet.Cells[1, 9].Value = Resources.Resources.SalesRep;
                worksheet.Cells[1, 10].Value = Resources.Resources.Value;
                var counter = 1;
                foreach (var contract in contracts)
                {
                    counter++;
                    worksheet.Cells[counter, 1].Value = contract.Details?.TransactionId ?? contract.Id.ToString();
                    worksheet.Cells[counter, 2].Value = contract.PrimaryCustomer?.FirstName + " " + contract.PrimaryCustomer?.LastName;
                    worksheet.Cells[counter, 3].Value = contract.Details?.Status ?? contract.ContractState.GetEnumDescription();
                    worksheet.Cells[counter, 4].Value = contract.Equipment?.AgreementType.GetEnumDescription();
                    worksheet.Cells[counter, 5].Value =
                        contract.PrimaryCustomer?.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress;
                    worksheet.Cells[counter, 6].Value = contract.PrimaryCustomer?.Phones?.FirstOrDefault(ph => ph.PhoneType == PhoneType.Cell)?.PhoneNum ?? contract.PrimaryCustomer?.Phones?.FirstOrDefault(ph => ph.PhoneType == PhoneType.Home)?.PhoneNum;
                    worksheet.Cells[counter, 7].Value = contract.LastUpdateTime?.ToString(CultureInfo.CurrentCulture);
                    worksheet.Cells[counter, 8].Value = contract.Equipment?.NewEquipment?.Select(eq => eq.TypeDescription).ConcatWithComma();
                    worksheet.Cells[counter, 9].Value = contract.Equipment?.SalesRep;
                    if (contract.Equipment?.ValueOfDeal != null)
                    {
                        worksheet.Cells[counter, 10].Value = Math.Round(contract.Equipment.ValueOfDeal.Value, 2);
                    }
                }
                worksheet.Cells[1, 1, 1, 10].Style.Font.Bold = true;
                worksheet.Cells[1, 1, 1, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                for (var i = 1; i <= 10; i++)
                {
                    worksheet.Column(i).Width = worksheet.Column(i).Width + 1;
                }
                package.Save();
            }
        }
    }
}
