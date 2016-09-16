using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DealnetPortal.Api.Models.Aspire
{
    [Serializable]
    public class Payload
    {
        public Lease Lease { get; set; }
        public LeaseDecision LeaseDecision { get; set; }
    }

    [Serializable]
    public class Lease
    {
        public Client Client { get; set; }
    }

    [Serializable]
    public class LeaseDecision
    {
        public string ExtAppId { get; set; }

        [XmlElement(ElementName = "BCL_App_Id")]
        public string BclAppId { get; set; }

        public Decision Decision { get; set; }

        [XmlElement(ElementName = "BCL_Cust_Id")]
        public string BclCustId { get; set; }

        [XmlElement(ElementName = "Decision_Date")]
        public string DecisionDate { get; set; }
    }

    [Serializable]
    public class Client
    {
        public Address Address { get; set; }
    }

    [Serializable]
    public class Decision
    {
        [XmlAttribute]
        public string Type { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [Serializable]
    public class Address
    {
        [XmlElement(ElementName = "Street_No")]
        public string StreetNo { get; set; }

        [XmlElement(ElementName = "Street_Name")]
        public string StreetName { get; set; }

        public string City { get; set; }

        public Province Province { get; set; }

        [XmlElement(ElementName = "Postalcode")]
        public string Postalcode { get; set; }

        public Country Country { get; set; }
    }

    [Serializable]
    public class Province
    {
        [XmlAttribute]
        public string Abbrev { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [Serializable]
    public class Country
    {
        [XmlAttribute]
        public string Abbrev { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}
