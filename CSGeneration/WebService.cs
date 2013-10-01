using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Web.Services.Description;

namespace CSGeneration
{
    public class WebService
    {
        public WebService(string path)
        {
            var metadataAddress = new EndpointAddress(path);
            var mexClient = new MetadataExchangeClient(metadataAddress.Uri, MetadataExchangeClientMode.HttpGet);
            mexClient.ResolveMetadataReferences = true;

            var metadata = mexClient.GetMetadata(metadataAddress.Uri, MetadataExchangeClientMode.HttpGet);
            var metadataSet = new MetadataSet(metadata.MetadataSections);

            var importer = new WsdlImporter(metadataSet);



            AllWsdlDocuments = importer.WsdlDocuments;
            AllContracts = importer.ImportAllContracts();
            AllBindings = importer.ImportAllBindings();
            AllEndpoints = importer.ImportAllEndpoints();

            //AllContracts.First().Operations.First().

        }

        public ServiceDescriptionCollection AllWsdlDocuments { get; set; }

        public Collection<ContractDescription> AllContracts { get; set; }

        public Collection<System.ServiceModel.Channels.Binding> AllBindings { get; set; }
        
        public ServiceEndpointCollection AllEndpoints { get; set; }
    }
}