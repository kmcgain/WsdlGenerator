namespace WcfService
{
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CompositeType", Namespace="http://schemas.datacontract.org/2004/07/WcfService")]
    public class CompositeType : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
    
        private bool BoolValueField;
    
        private System.DateTime DateTimeValueField;
    
        private System.Guid GuidValueField;
    
        private string StringValueField;
    
        public virtual System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
            	return this.extensionDataField;
            }
            set
            {
            	this.extensionDataField = value;
            }
        }
    
        [System.Runtime.Serialization.DataMemberAttribute()]
        public virtual bool BoolValue
        {
            get
            {
            	return this.BoolValueField;
            }
            set
            {
            	this.BoolValueField = value;
            }
        }
    
        [System.Runtime.Serialization.DataMemberAttribute()]
        public virtual System.DateTime DateTimeValue
        {
            get
            {
            	return this.DateTimeValueField;
            }
            set
            {
            	this.DateTimeValueField = value;
            }
        }
    
        [System.Runtime.Serialization.DataMemberAttribute()]
        public virtual System.Guid GuidValue
        {
            get
            {
            	return this.GuidValueField;
            }
            set
            {
            	this.GuidValueField = value;
            }
        }
    
        [System.Runtime.Serialization.DataMemberAttribute()]
        public virtual string StringValue
        {
            get
            {
            	return this.StringValueField;
            }
            set
            {
            	this.StringValueField = value;
            }
        }
    }
}
