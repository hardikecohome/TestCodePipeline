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
        public static string Sin = "SIN";
        public static string DriverLicense = "DriverLicense";
        public static string Dl = "DL";
        public static string Sin2 = "SIN_2";
        public static string City = "City";
        public static string Province = "Province";
        public static string PostalCode = "PostalCode";
        public static string HomePhone = "HomePhone";
        public static string HomePhone2 = "HomePhone_2";
        public static string CellPhone = "CellPhone";
        public static string CellPhone2 = "CellPhone_2";
        public static string BusinessPhone = "BusinessPhone";
        public static string BusinessOrCellPhone = "BusinessOrCellPhone";
        public static string EmailAddress = "EmailAddress";
        public static string EmailAddress2 = "EmailAddress_2";
        public static string IsMailingDifferent = "IsMailingDifferent";
        public static string SuiteNo = "SuiteNo";                

        //Equipment Fields
        public static string IsFurnace = "IsFurnace";
        public static string IsAirConditioner = "IsAirConditioner";
        public static string IsBoiler = "IsBoiler";
        public static string IsWaterFiltration = "IsWaterFiltration";
        public static string IsOther1 = "IsOther1";
        public static string IsOther2 = "IsOther2";
        public static string IsOtherBase = "IsOther";
        public static string FurnaceDetails = "FurnaceDetails";
        public static string AirConditionerDetails = "AirConditionerDetails";
        public static string BoilerDetails = "BoilerDetails";
        public static string WaterFiltrationDetails = "WaterFiltrationDetails";
        public static string OtherDetails1 = "OtherDetails1";
        public static string OtherDetails2 = "OtherDetails2";
        public static string OtherDetailsBase = "OtherDetails";
        public static string FurnaceMonthlyRental = "FurnaceMonthlyRental";
        public static string AirConditionerMonthlyRental = "AirConditionerMonthlyRental";
        public static string BoilerMonthlyRental = "BoilerMonthlyRental";
        public static string WaterFiltrationMonthlyRental = "WaterFiltrationMonthlyRental";
        public static string OtherMonthlyRental1 = "OtherMonthlyRental1";
        public static string OtherMonthlyRental2 = "OtherMonthlyRental2";
        public static string OtherMonthlyRentalBase = "OtherMonthlyRental";

        public static string MonthlyPayment = "MonthlyPayment";
        public static string CustomerRate = "CustomerRate";
        public static string TotalPayment = "TotalPayment";
        public static string TotalMonthlyPayment = "TotalMonthlyPayment";
        public static string Hst = "HST";
        public static string DownPayment = "DownPayment";
        public static string AdmeenFee = "AdmeenFee";
        public static string LoanTotalCashPrice = "LoanTotalCashPrice";
        public static string LoanAmountFinanced = "LoanAmountFinanced";
        public static string LoanTotalObligation = "LoanTotalObligation";
        public static string LoanBalanceOwing = "LoanBalanceOwing";
        public static string LoanTotalBorowingCost = "LoanTotalBorowingCost";

        public static string RequestedTerm = "RequestedTerm";
        public static string AmortizationTerm = "AmortizationTerm";
        public static string DeferralTerm = "DeferralTerm";
        public static string YesDeferral = "YesDeferral";
        public static string NoDeferral = "NoDeferral";

        public static string EquipmentQuantity = "EquipmentQuantity";
        public static string EquipmentDescription = "EquipmentDescription";
        public static string EquipmentCost = "EquipmentCost";

        public static string InstallDate = "InstallDate";

        public static string HouseSize = "HouseSize";

        //Payment Fields
        public static string EnbridgeAccountNumber = "EnbridgeAccountNumber";
        public static string Ean = "EAN";
        public static string IsEnbridge = "IsEnbridge";
        public static string IsPAD = "IsPAD";
        public static string IsPAD1 = "IsPAD1";
        public static string IsPAD15 = "IsPAD15";
        public static string BankNumber = "BankNumber";
        public static string TransitNumber = "TransitNumber";
        public static string AccountNumber = "AccountNumber";
        public static string Bn = "BN";
        public static string Tn = "TN";
        public static string An = "AN";


        //Dealer and SalesRep fields
        public static string SalesRep = "SalesRep";
        public static string DealerName = "DealerName";
        public static string DealerAddress = "DealerAddress";
        public static string DealerPhone = "DealerPhone";

        //For Signed Installation Certificate
        public static string CustomerName = "CustomerName";
        public static string CustomerName2 = "CustomerName_2";
        public static string InstallerName = "InstallerName";
        public static string InstallationDate = "InstallationDate";
        public static string EquipmentModel = "EquipmentModel";
        public static string EquipmentSerialNumber = "EquipmentSerialNumber";

        public static string IsExistingEquipmentRental = "IsExistingEquipmentRental";
        public static string IsExistingEquipmentNoRental = "IsExistingEquipmentNoRental";
        public static string ExistingEquipmentRentalCompany = "ExistingEquipmentRentalCompany";
        public static string ExistingEquipmentMake = "ExistingEquipmentMake";
        public static string ExistingEquipmentModel = "ExistingEquipmentModel";
        public static string ExistingEquipmentSerialNumber = "ExistingEquipmentSerialNumber";
        public static string ExistingEquipmentGeneralCondition = "ExistingEquipmentGeneralCondition";
        public static string ExistingEquipmentAge = "ExistingEquipmentAge";
    }
}
