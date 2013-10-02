namespace WcfService
{
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CompositeType", Namespace="http://schemas.datacontract.org/2004/07/WcfService")]
    public class CompType2 : object, System.Runtime.Serialization.IExtensibleDataObject
    {
    
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
    
        private string TestValueField;
    
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
        public virtual string TestValue
        {
            get
            {
            	return this.TestValueField;
            }
            set
            {
            	this.TestValueField = value;
            }
        }
    }
}
