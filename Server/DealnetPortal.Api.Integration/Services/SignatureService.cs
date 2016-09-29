using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Integration.ServiceAgents.ESignature;
using DealnetPortal.Api.Models;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using DealnetPortal.Utilities;

namespace DealnetPortal.Api.Integration.Services
{
    public class SignatureService : ISignatureService
    {
        private readonly IESignatureServiceAgent _signatureServiceAgent;
        private readonly IContractRepository _contractRepository;
        private readonly ILoggingService _loggingService;
        private readonly IFileRepository _fileRepository;

        //private List<string> _fieldNames = new List<string>()
        //{
        //    "First Name", "Last Name"
        //}

        public SignatureService(IESignatureServiceAgent signatureServiceAgent, IContractRepository contractRepository,
            IFileRepository fileRepository, ILoggingService loggingService)
        {
            _signatureServiceAgent = signatureServiceAgent;
            _contractRepository = contractRepository;
            _loggingService = loggingService;
            _fileRepository = fileRepository;
        }

        public IList<Alert> ProcessContract(int contractId, string ownerUserId)
        {
            IList<Alert> alerts = new List<Alert>();

            // Get contract
            var contract = _contractRepository.GetContractAsUntracked(contractId, ownerUserId);
            if (contract != null)
            {
                _loggingService.LogInfo($"Started eSignature processing for contract [{contractId}]");
                var fields = PrepareFormFields(contract);
                _loggingService.LogInfo($"{fields.Count} fields collected");


            }
            else
            {
                var errorMsg = $"Can't get contract [{contractId}] for processing";
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = "eSignature error",
                    Message = errorMsg
                });
                _loggingService.LogError(errorMsg);
            }

            return alerts;
        }

        private Dictionary<string, string> PrepareFormFields(Contract contract)
        {
            var fields = new Dictionary<string, string>();

            FillHomeOwnerFieilds(fields, contract);
            FillApplicantsFieilds(fields, contract);
            FillEquipmentFieilds(fields, contract);
            FillPaymentFieilds(fields, contract);

            return fields;
        }

        private void FillHomeOwnerFieilds(Dictionary<string, string> formFields, Contract contract)
        {
            if (contract.PrimaryCustomer != null)
            {
                formFields[PdfFormFields.FirstName] = contract.PrimaryCustomer.FirstName;
                formFields[PdfFormFields.LastName] = contract.PrimaryCustomer.LastName;
                formFields[PdfFormFields.DateOfBirth] = contract.PrimaryCustomer.DateOfBirth.ToShortDateString();
                if (contract.PrimaryCustomer.Locations?.Any() ?? false)
                {
                    var mainAddress =
                        contract.PrimaryCustomer?.Locations?.FirstOrDefault(
                            l => l.AddressType == AddressType.MainAddress);
                    if (mainAddress != null)
                    {
                        formFields[PdfFormFields.InstallationAddress] = mainAddress.Street;
                        formFields[PdfFormFields.City] = mainAddress.City;
                        formFields[PdfFormFields.Province] = mainAddress.State;
                        formFields[PdfFormFields.PostalCode] = mainAddress.PostalCode;
                    }
                    var mailAddress =
                        contract.PrimaryCustomer?.Locations?.FirstOrDefault(
                            l => l.AddressType == AddressType.MailAddress);
                    if (mailAddress != null)
                    {
                        formFields[PdfFormFields.IsMailingDifferent] = "true";
                        formFields[PdfFormFields.MailingAddress] =
                            $"{mailAddress.Street}, {mailAddress.City}, {mailAddress.State}, {mailAddress.PostalCode}";
                    }
                }
            }

            //TODO: re-implement after refactoring
            if (contract.ContactInfo != null)
            {
                if (!string.IsNullOrEmpty(contract.ContactInfo.EmailAddress))
                {
                    formFields[PdfFormFields.EmailAddress] = contract.ContactInfo.EmailAddress;
                }
                if (contract.ContactInfo.Phones?.Any() ?? false)
                {
                    var homePhone = contract.ContactInfo.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Home);
                    var cellPhone = contract.ContactInfo.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Cell);
                    var businessPhone =
                        contract.ContactInfo.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Business);

                    if (homePhone == null)
                    {
                        homePhone = cellPhone;
                        cellPhone = null;
                    }
                    if (homePhone != null)
                    {
                        formFields[PdfFormFields.HomePhone] = homePhone.PhoneNum;
                    }
                    if (cellPhone != null)
                    {
                        //formFields[PdfFormFields.Ce] = cellPhone.PhoneNum;
                    }
                    if (businessPhone != null)
                    {
                        formFields[PdfFormFields.BusinessPhone] = businessPhone.PhoneNum;
                    }
                }
            }
        }

        private void FillApplicantsFieilds(Dictionary<string, string> formFields, Contract contract)
        {
            if (contract.SecondaryCustomers?.Any() ?? false)
            {
                var addApplicant = contract.SecondaryCustomers.First();
                formFields[PdfFormFields.FirstName2] = addApplicant.FirstName;
                formFields[PdfFormFields.LastName2] = addApplicant.LastName;
                formFields[PdfFormFields.DateOfBirth2] = addApplicant.DateOfBirth.ToShortDateString();
            }
        }

        private void FillEquipmentFieilds(Dictionary<string, string> formFields, Contract contract)
        {
            if (contract.Equipment?.NewEquipment?.Any() ?? false)
            {
                var newEquipments = contract.Equipment.NewEquipment;
                var fstEq = newEquipments.First();

                formFields[PdfFormFields.EquipmentQuantity] = fstEq.Quantity.ToString();
                formFields[PdfFormFields.EquipmentDescription] = fstEq.Description.ToString();
                formFields[PdfFormFields.EquipmentCost] = fstEq.Cost.ToString(CultureInfo.CurrentCulture);
                formFields[PdfFormFields.MonthlyPayment] = fstEq.MonthlyCost.ToString(CultureInfo.CurrentCulture);

                foreach (var eq in newEquipments)
                {
                    switch (eq.Type)
                    {
                //        new EquipmentType { Description = "Air Conditioner", Type = "ECO1" },
                //new EquipmentType { Description = "Boiler", Type = "ECO2" },
                //new EquipmentType { Description = "Doors", Type = "ECO3" },
                //new EquipmentType { Description = "Fireplace", Type = "ECO4" },
                //new EquipmentType { Description = "Furnace", Type = "ECO5" },
                //new EquipmentType { Description = "HWT", Type = "ECO6" },
                //new EquipmentType { Description = "Plumbing", Type = "ECO7" },
                //new EquipmentType { Description = "Roofing", Type = "ECO9" },
                //new EquipmentType { Description = "Siding", Type = "ECO10" },
                //new EquipmentType { Description = "Tankless Water Heater", Type = "ECO11" },
                //new EquipmentType { Description = "Windows", Type = "ECO13" },
                //new EquipmentType { Description = "Sunrooms", Type = "ECO38" },
                //new EquipmentType { Description = "Air Handler", Type = "ECO40" },
                //new EquipmentType { Description = "Flooring", Type = "ECO42" },
                //new EquipmentType { Description = "Porch Enclosure", Type = "ECO43" },
                //new EquipmentType { Description = "Water Treatment System", Type = "ECO44" },
                //new EquipmentType { Description = "Heat Pump", Type = "ECO45" },
                //new EquipmentType { Description = "HRV", Type = "ECO46" },
                //new EquipmentType { Description = "Bathroom", Type = "ECO47" },
                //new EquipmentType { Description = "Kitchen", Type = "ECO48" },
                //new EquipmentType { Description = "Hepa System", Type = "ECO49" },
                //new EquipmentType { Description = "Unknown", Type = "ECO50" },
                //new EquipmentType { Description = "Security System", Type = "ECO52" },
                //new EquipmentType { Description = "Basement Repair", Type = "ECO55" }
                    }
                }

            }
            if (contract.Equipment != null)
            {
                formFields[PdfFormFields.TotalPayment] =
                    contract.Equipment.TotalMonthlyPayment.ToString(CultureInfo.CurrentCulture);
            }

            //Equipment Fields
            //public static string IsFurnace = "Is Furnace";
            //public static string IsAirConditioner = "Is Air Conditioner";
            //public static string IsBoiler = "Is Boiler";
            //public static string IsWaterFiltration = "Is Water Filtration";
            //public static string IsOther1 = "Is Other1";
            //public static string IsOther2 = "Is Other2";
            //public static string FurnaceDetails = "FurnaceDetails";
            //public static string AirConditionerDetails = "AirConditionerDetails";
            //public static string BoilerDetails = "BoilerDetails";
            //public static string WaterFiltrationDetails = "BoilerDetails";
            //public static string OtherDetails1 = "OtherDetails1";
            //public static string OtherDetails2 = "OtherDetails2";
            //public static string FurnaceMonthlyRental = "FurnaceMonthlyRental";
            //public static string AirConditionerMonthlyRental = "AirConditionerMonthlyRental";
            //public static string BoilerMonthlyRental = "BoilerMonthlyRental";
            //public static string WaterFiltrationMonthlyRental = "BoilerMonthlyRental";
            //public static string OtherMonthlyRental1 = "OtherMonthlyRental1";
            //public static string OtherMonthlyRental2 = "OtherMonthlyRental2";

            //public static string MonthlyPayment = "Monthly Payment";
            //public static string TotalPayment = "Total Payment";

            //public static string EquipmentQuantity = "Equipment Quantity";
            //public static string EquipmentDescription = "Equipment Description";
            //public static string EquipmentCost = "Equipment Cost";
        }

        private void FillPaymentFieilds(Dictionary<string, string> formFields, Contract contract)
        {
            if (contract.PaymentInfo != null)
            {
                if (contract.PaymentInfo.PaymentType == PaymentType.Enbridge)
                {
                    formFields[PdfFormFields.IsEnbridge] = "true";
                    formFields[PdfFormFields.EnbridgeAccountNumber] = contract.PaymentInfo.EnbridgeGasDistributionAccount;
                }
                else
                {
                    formFields[PdfFormFields.IsPAD] = "true";
                    formFields[contract.PaymentInfo.PrefferedWithdrawalDate == WithdrawalDateType.First ? PdfFormFields.IsPAD1 : PdfFormFields.IsPAD15] = "true";                    
                }                
            }        
        }
    }
}
