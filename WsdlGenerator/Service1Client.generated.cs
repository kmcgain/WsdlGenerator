[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public class Service1Client : System.ServiceModel.ClientBase<IService1>, IService1
{

    public Service1Client()
    {
    }

    public Service1Client(string endpointConfigurationName) :
			base(endpointConfigurationName)
    {
    }

    public Service1Client(string endpointConfigurationName, string remoteAddress) :
			base(endpointConfigurationName, remoteAddress)
    {
    }

    public Service1Client(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) :
			base(endpointConfigurationName, remoteAddress)
    {
    }

    public Service1Client(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
			base(binding, remoteAddress)
    {
    }

    public string GetData(int value, int value2, WcfService.CompType2 comp)
    {
        return base.Channel.GetData(value, value2, comp);
    }

    public WcfService.CompositeType GetDataUsingDataContract(WcfService.CompositeType composite)
    {
        return base.Channel.GetDataUsingDataContract(composite);
    }
}