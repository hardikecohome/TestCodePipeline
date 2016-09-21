﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Integration.Services.ESignature.EOriginalTypes
{
    /// /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.eoriginal.com/AddSigBlocksInstructions")]
    [System.Xml.Serialization.XmlRootAttribute("addSigBlocks", Namespace = "http://www.eoriginal.com/AddSigBlocksInstructions", IsNullable = false)]
    public partial class AddSigBlocks : TransformationInstructions
    {

        private SigBlock[] sigBlockListField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("sigBlock", IsNullable = false)]
        public SigBlock[] sigBlockList
        {
            get
            {
                return this.sigBlockListField;
            }
            set
            {
                this.sigBlockListField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.eoriginal.com/AddSigBlocksInstructions")]
    public partial class SigBlock
    {

        private string nameField;

        private string lowerLeftXField;

        private string lowerLeftYField;

        private string upperRightXField;

        private string upperRightYField;

        private object itemField;

        private string signerNameField;

        private CustomProperty[] customPropertyField;

        /// <remarks/>
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string lowerLeftX
        {
            get
            {
                return this.lowerLeftXField;
            }
            set
            {
                this.lowerLeftXField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string lowerLeftY
        {
            get
            {
                return this.lowerLeftYField;
            }
            set
            {
                this.lowerLeftYField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string upperRightX
        {
            get
            {
                return this.upperRightXField;
            }
            set
            {
                this.upperRightXField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string upperRightY
        {
            get
            {
                return this.upperRightYField;
            }
            set
            {
                this.upperRightYField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("anchor", typeof(anchor))]
        [System.Xml.Serialization.XmlElementAttribute("page", typeof(string), DataType = "integer")]
        public object Item
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }

        /// <remarks/>
        public string signerName
        {
            get
            {
                return this.signerNameField;
            }
            set
            {
                this.signerNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("customProperty")]
        public CustomProperty[] customProperty
        {
            get
            {
                return this.customPropertyField;
            }
            set
            {
                this.customPropertyField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.eoriginal.com/TransformationInstructionsSet")]
    public partial class anchor
    {

        private string pageField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string page
        {
            get
            {
                return this.pageField;
            }
            set
            {
                this.pageField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.eoriginal.com/AddSigBlocksInstructions")]
    public partial class CustomProperty
    {

        private string nameField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }    

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.eoriginal.com/TransformationInstructionsSet")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.eoriginal.com/TransformationInstructionsSet", IsNullable = false)]
    public partial class transformationInstructionSet
    {

        private TransformationInstructions[] transformationInstructionsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("transformationInstructions")]
        public TransformationInstructions[] transformationInstructions
        {
            get
            {
                return this.transformationInstructionsField;
            }
            set
            {
                this.transformationInstructionsField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.eoriginal.com/TypeSettingValues")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.eoriginal.com/TypeSettingValues", IsNullable = false)]
    public enum font
    {

        /// <remarks/>
        TimesRoman,

        /// <remarks/>
        Helvetica,

        /// <remarks/>
        Courier,

        /// <remarks/>
        Symbol,

        /// <remarks/>
        ZapfDingbats,

        /// <remarks/>
        Arial,

        /// <remarks/>
        ArialBold,

        /// <remarks/>
        ArialBoldItalic,

        /// <remarks/>
        ArialItalic,

        /// <remarks/>
        Brush,

        /// <remarks/>
        Campaign,

        /// <remarks/>
        Chancery,

        /// <remarks/>
        ChopinScript,

        /// <remarks/>
        FreeMono,

        /// <remarks/>
        FreeMonoBold,

        /// <remarks/>
        FreeMonoBoldOblique,

        /// <remarks/>
        FreeMonoOblique,

        /// <remarks/>
        FreeSans,

        /// <remarks/>
        FreeSansBold,

        /// <remarks/>
        FreeSansBoldOblique,

        /// <remarks/>
        FreeSansOblique,

        /// <remarks/>
        FreeSerif,

        /// <remarks/>
        FreeSerifBold,

        /// <remarks/>
        FreeSerifBoldItalic,

        /// <remarks/>
        FreeSerifItalic,

        /// <remarks/>
        OldScript,

        /// <remarks/>
        WaltDisney,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.eoriginal.com/AddSignatureInstructions")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.eoriginal.com/AddSignatureInstructions", IsNullable = false)]
    public partial class textAppearance
    {

        private string textField;

        private object itemField;

        private FontStyle fontStyleField;

        private string fontSizeField;

        private string colorField;

        private Alignment horizontalAlignmentField;

        private bool horizontalAlignmentFieldSpecified;

        private verticalAlignment verticalAlignmentField;

        private bool verticalAlignmentFieldSpecified;

        /// <remarks/>
        public string text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("customFont", typeof(string), Namespace = "http://www.eoriginal.com/TypeSettingValues")]
        [System.Xml.Serialization.XmlElementAttribute("font", typeof(font), Namespace = "http://www.eoriginal.com/TypeSettingValues")]
        public object Item
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }

        /// <remarks/>
        public FontStyle fontStyle
        {
            get
            {
                return this.fontStyleField;
            }
            set
            {
                this.fontStyleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")]
        public string fontSize
        {
            get
            {
                return this.fontSizeField;
            }
            set
            {
                this.fontSizeField = value;
            }
        }

        /// <remarks/>
        public string color
        {
            get
            {
                return this.colorField;
            }
            set
            {
                this.colorField = value;
            }
        }

        /// <remarks/>
        public Alignment horizontalAlignment
        {
            get
            {
                return this.horizontalAlignmentField;
            }
            set
            {
                this.horizontalAlignmentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool horizontalAlignmentSpecified
        {
            get
            {
                return this.horizontalAlignmentFieldSpecified;
            }
            set
            {
                this.horizontalAlignmentFieldSpecified = value;
            }
        }

        /// <remarks/>
        public verticalAlignment verticalAlignment
        {
            get
            {
                return this.verticalAlignmentField;
            }
            set
            {
                this.verticalAlignmentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool verticalAlignmentSpecified
        {
            get
            {
                return this.verticalAlignmentFieldSpecified;
            }
            set
            {
                this.verticalAlignmentFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.eoriginal.com/AddSignatureInstructions")]
    public partial class MultimediaSignature
    {

        private string signatureDataField;

        private string mimeTypeField;

        private string fileNameField;

        /// <remarks/>
        public string signatureData
        {
            get
            {
                return this.signatureDataField;
            }
            set
            {
                this.signatureDataField = value;
            }
        }

        /// <remarks/>
        public string mimeType
        {
            get
            {
                return this.mimeTypeField;
            }
            set
            {
                this.mimeTypeField = value;
            }
        }

        /// <remarks/>
        public string fileName
        {
            get
            {
                return this.fileNameField;
            }
            set
            {
                this.fileNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.eoriginal.com/AddSignatureInstructions")]
    public partial class Signature
    {

        private object itemField;

        private ItemChoiceType itemElementNameField;

        private object item1Field;

        private Item1ChoiceType item1ElementNameField;

        private string borderTopTextField;

        private string borderBottomTextField;

        private string reasonField;

        private string locationField;

        private string contactField;

        private MultimediaSignature multimediaSignatureField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("imageAppearance", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("invisibleAppearance", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("sigStringAppearance", typeof(sigStringAppearance))]
        [System.Xml.Serialization.XmlElementAttribute("textAppearance", typeof(textAppearance))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemElementName")]
        public object Item
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ItemChoiceType ItemElementName
        {
            get
            {
                return this.itemElementNameField;
            }
            set
            {
                this.itemElementNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("invisiblePlacement", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("sigBlockPlacement", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("signaturePlacement", typeof(signaturePlacement))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("Item1ElementName")]
        public object Item1
        {
            get
            {
                return this.item1Field;
            }
            set
            {
                this.item1Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Item1ChoiceType Item1ElementName
        {
            get
            {
                return this.item1ElementNameField;
            }
            set
            {
                this.item1ElementNameField = value;
            }
        }

        /// <remarks/>
        public string borderTopText
        {
            get
            {
                return this.borderTopTextField;
            }
            set
            {
                this.borderTopTextField = value;
            }
        }

        /// <remarks/>
        public string borderBottomText
        {
            get
            {
                return this.borderBottomTextField;
            }
            set
            {
                this.borderBottomTextField = value;
            }
        }

        /// <remarks/>
        public string reason
        {
            get
            {
                return this.reasonField;
            }
            set
            {
                this.reasonField = value;
            }
        }

        /// <remarks/>
        public string location
        {
            get
            {
                return this.locationField;
            }
            set
            {
                this.locationField = value;
            }
        }

        /// <remarks/>
        public string contact
        {
            get
            {
                return this.contactField;
            }
            set
            {
                this.contactField = value;
            }
        }

        /// <remarks/>
        public MultimediaSignature multimediaSignature
        {
            get
            {
                return this.multimediaSignatureField;
            }
            set
            {
                this.multimediaSignatureField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.eoriginal.com/AddSignatureInstructions")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.eoriginal.com/AddSignatureInstructions", IsNullable = false)]
    public partial class sigStringAppearance
    {

        private string foregroundColorField;

        private bool encryptedField;

        private string encryptionKeyField;

        private Alignment horizontalAlignmentField;

        private bool horizontalAlignmentFieldSpecified;

        private verticalAlignment verticalAlignmentField;

        private bool verticalAlignmentFieldSpecified;

        private string valueField;

        public sigStringAppearance()
        {
            this.foregroundColorField = "Black";
            this.encryptedField = false;
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("Black")]
        public string foregroundColor
        {
            get
            {
                return this.foregroundColorField;
            }
            set
            {
                this.foregroundColorField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool encrypted
        {
            get
            {
                return this.encryptedField;
            }
            set
            {
                this.encryptedField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string encryptionKey
        {
            get
            {
                return this.encryptionKeyField;
            }
            set
            {
                this.encryptionKeyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Alignment horizontalAlignment
        {
            get
            {
                return this.horizontalAlignmentField;
            }
            set
            {
                this.horizontalAlignmentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool horizontalAlignmentSpecified
        {
            get
            {
                return this.horizontalAlignmentFieldSpecified;
            }
            set
            {
                this.horizontalAlignmentFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public verticalAlignment verticalAlignment
        {
            get
            {
                return this.verticalAlignmentField;
            }
            set
            {
                this.verticalAlignmentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool verticalAlignmentSpecified
        {
            get
            {
                return this.verticalAlignmentFieldSpecified;
            }
            set
            {
                this.verticalAlignmentFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.eoriginal.com/AddSignatureInstructions")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.eoriginal.com/AddSignatureInstructions", IsNullable = false)]
    public partial class signaturePlacement
    {

        private string lowerLeftXField;

        private string lowerLeftYField;

        private string upperRightXField;

        private string upperRightYField;

        private string pageField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string lowerLeftX
        {
            get
            {
                return this.lowerLeftXField;
            }
            set
            {
                this.lowerLeftXField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string lowerLeftY
        {
            get
            {
                return this.lowerLeftYField;
            }
            set
            {
                this.lowerLeftYField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string upperRightX
        {
            get
            {
                return this.upperRightXField;
            }
            set
            {
                this.upperRightXField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string upperRightY
        {
            get
            {
                return this.upperRightYField;
            }
            set
            {
                this.upperRightYField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string page
        {
            get
            {
                return this.pageField;
            }
            set
            {
                this.pageField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.eoriginal.com/AddSignatureInstructions")]
    [System.Xml.Serialization.XmlRootAttribute("addSignature", Namespace = "http://www.eoriginal.com/AddSignatureInstructions", IsNullable = false)]
    public partial class AddSignature : TransformationInstructions
    {

        private string tokenNameField;

        private string tokenPasswordField;

        private Signature[] signatureListField;

        /// <remarks/>
        public string tokenName
        {
            get
            {
                return this.tokenNameField;
            }
            set
            {
                this.tokenNameField = value;
            }
        }

        /// <remarks/>
        public string tokenPassword
        {
            get
            {
                return this.tokenPasswordField;
            }
            set
            {
                this.tokenPasswordField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("signature", IsNullable = false)]
        public Signature[] signatureList
        {
            get
            {
                return this.signatureListField;
            }
            set
            {
                this.signatureListField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.eoriginal.com/AddTextDataInstructions")]
    [System.Xml.Serialization.XmlRootAttribute("addTextData", Namespace = "http://www.eoriginal.com/AddTextDataInstructions", IsNullable = false)]
    public partial class AddTextData : TransformationInstructions
    {

        private TextData[] textDataListField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("textData", IsNullable = false)]
        public TextData[] textDataList
        {
            get
            {
                return this.textDataListField;
            }
            set
            {
                this.textDataListField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.eoriginal.com/AddTextDataInstructions")]
    public partial class TextData
    {

        private object[] itemsField;

        private string textField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("customProperty", typeof(CustomProperty))]
        [System.Xml.Serialization.XmlElementAttribute("fieldName", typeof(string))]
        public object[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }

        /// <remarks/>
        public string text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.eoriginal.com/AddTextFieldInstructions")]
    public partial class OptionList
    {

        private string displayField;

        private string valueField;

        /// <remarks/>
        public string display
        {
            get
            {
                return this.displayField;
            }
            set
            {
                this.displayField = value;
            }
        }

        /// <remarks/>
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.eoriginal.com/AddTextFieldInstructions")]
    public partial class RadioButton
    {

        private string nameField;

        private string valueField;

        private string lowerLeftXField;

        private string lowerLeftYField;

        private string upperRightXField;

        private string upperRightYField;

        /// <remarks/>
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string lowerLeftX
        {
            get
            {
                return this.lowerLeftXField;
            }
            set
            {
                this.lowerLeftXField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string lowerLeftY
        {
            get
            {
                return this.lowerLeftYField;
            }
            set
            {
                this.lowerLeftYField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string upperRightX
        {
            get
            {
                return this.upperRightXField;
            }
            set
            {
                this.upperRightXField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string upperRightY
        {
            get
            {
                return this.upperRightYField;
            }
            set
            {
                this.upperRightYField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.eoriginal.com/AddTextFieldInstructions")]
    public partial class CustomTextProperty
    {

        private string nameField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.eoriginal.com/AddTextFieldInstructions")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.eoriginal.com/AddTextFieldInstructions", IsNullable = false)]
    public partial class radioButtonGroup
    {

        private string nameField;

        private RadioButton[] radioButtonField;

        private object itemField;

        private CustomTextProperty[] customPropertyField;

        /// <remarks/>
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("radioButton")]
        public RadioButton[] radioButton
        {
            get
            {
                return this.radioButtonField;
            }
            set
            {
                this.radioButtonField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("anchor", typeof(anchor))]
        [System.Xml.Serialization.XmlElementAttribute("page", typeof(string), DataType = "integer")]
        public object Item
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("customProperty")]
        public CustomTextProperty[] customProperty
        {
            get
            {
                return this.customPropertyField;
            }
            set
            {
                this.customPropertyField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.eoriginal.com/AddTextFieldInstructions")]
    [System.Xml.Serialization.XmlRootAttribute("addTextFields", Namespace = "http://www.eoriginal.com/AddTextFieldInstructions", IsNullable = false)]
    public partial class AddTextFields : TransformationInstructions
    {

        private object[] textFieldListField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("TextFieldBase", IsNullable = false)]
        public object[] textFieldList
        {
            get
            {
                return this.textFieldListField;
            }
            set
            {
                this.textFieldListField = value;
            }
        }
    }
}
