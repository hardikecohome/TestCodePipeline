using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Integration.Services
{
    public static class PdfFormFields
    {
        //Customer Fields
        public static string FirstName = "FirstName";
        public static string LastName = "LastName";
        public static string DateOfBirth = "DateOfBirth";
        public static string FirstName2 = "FirstName_2";
        public static string LastName2 = "LastName_2";
        public static string DateOfBirth2 = "DateOfBirth_2";
        public static string InstallationAddress = "InstallationAddress";
        public static string MailingAddress = "MailingAddress";
        public static string City = "City";
        public static string Province = "Province";
        public static string PostalCode = "PostalCode";
        public static string HomePhone = "HomePhone";
        public static string BusinessPhone = "BusinessPhone";
        public static string EmailAddress = "EmailAddress";
        public static string IsMailingDifferent = "IsMailingDifferent";

        //Equipment Fields
        public static string IsFurnace = "IsFurnace";
        public static string IsAirConditioner = "IsAirConditioner";
        public static string IsBoiler = "IsBoiler";
        public static string IsWaterFiltration = "IsWaterFiltration";
        public static string IsOther1 = "IsOther1";
        public static string IsOther2 = "IsOther2";
        public static string FurnaceDetails = "FurnaceDetails";
        public static string AirConditionerDetails = "AirConditionerDetails";
        public static string BoilerDetails = "BoilerDetails";
        public static string WaterFiltrationDetails = "WaterFiltrationDetails";
        public static string OtherDetails1 = "OtherDetails1";
        public static string OtherDetails2 = "OtherDetails2";
        public static string FurnaceMonthlyRental = "FurnaceMonthlyRental";
        public static string AirConditionerMonthlyRental = "AirConditionerMonthlyRental";
        public static string BoilerMonthlyRental = "BoilerMonthlyRental";
        public static string WaterFiltrationMonthlyRental = "WaterFiltrationMonthlyRental";
        public static string OtherMonthlyRental1 = "OtherMonthlyRental1";
        public static string OtherMonthlyRental2 = "OtherMonthlyRental2";

        public static string MonthlyPayment = "MonthlyPayment";
        public static string TotalPayment = "TotalPayment";
        public static string TotalMonthlyPayment = "TotalMonthlyPayment";
        public static string Hst = "HST";

        public static string EquipmentQuantity = "EquipmentQuantity";
        public static string EquipmentDescription = "EquipmentDescription";
        public static string EquipmentCost = "EquipmentCost";

        //Payment Fields
        public static string EnbridgeAccountNumber = "Enbridge Account Number";
        public static string Ean = "EAN";
        public static string IsEnbridge = "IsEnbridge";
        public static string IsPAD = "IsPAD";
        public static string IsPAD1 = "IsPAD1";
        public static string IsPAD15 = "IsPAD15";
    }
}
