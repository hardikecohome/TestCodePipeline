﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Integration.Services
{
    static class AspireUdfFields
    {
        public static string NumberOfEmployee = "Number Of Employee";
        public static string AuthorizedConsent = "Authorized Consent";
        public static string OptIn = "Opt In";
        public static string HomeOwner = "HomeOwner";        
        //Installation Address
        public static string InstallationAddress = "Installation Address";
        public static string InstallationAddressCity = "Installation Address City";
        public static string InstallationAddressCountry = "Installation Address Country";
        public static string InstallationAddressPostalCode = "Installation Address Postal Code";
        public static string InstallationAddressState = "Installation Address State";
        //Mailing Address
        public static string MailingAddress = "Mailing Address";
        public static string MailingAddressCity = "Mailing Address City";
        public static string MailingAddressCountry = "Mailing Address Country";
        public static string MailingAddressPostalCode = "Mailing Address Postal Code";
        public static string MailingAddressState = "Mailing Address State";
        //Previous Address
        public static string PreviousAddress = "Previous Address";
        public static string PreviousAddressCity = "Previous Address City";
        public static string PreviousAddressCountry = "Previous Address Country";
        public static string PreviousAddressPostalCode = "Previous Address Postal Code";
        public static string PreviousAddressState = "Previous Address State";

        public static string EstimatedMoveInDate = "Estimated move in date";

        public static string AllowCommunicate = "AllowCommunicate";
        public static string ContactViaEmail = "Contact Via Email";
        public static string ContactViaText = "Contact Via Text";
        public static string ContactViaPhone = "Contact Via Phone";

        public static string CustomerType = "Customer Type";
    }
}
