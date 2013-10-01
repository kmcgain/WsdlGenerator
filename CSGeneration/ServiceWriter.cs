using System;
using System.CodeDom;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Xml.Schema;
using System.Xml.XPath;

namespace CSGeneration
{
    public class ServiceWriter
    {
        private WebService service;
        private readonly TemplateOperations templateOperations;
        private Action<string> debug;
        private string debugOutput;
        private bool generateServiceContract = false;
        private DataContractWriter dataContractWriter;

        public ServiceWriter(WebService service, TemplateOperations templateOperations)
        {
            this.service = service;
            this.templateOperations = templateOperations;
            debugOutput = "";

            debug = templateOperations.Debug;

            dataContractWriter = new DataContractWriter(templateOperations);
        }

        public void WriteToOutput()
        {
            debug("Endpoints: " + service.AllEndpoints.Count);
            foreach (var endpoint in service.AllEndpoints)
            {
                debug("Endpoint:" + endpoint.Name);                
            }

            foreach (var binding in service.AllBindings)
            {
                debug("Binding: " + binding.Name);
                debug("namespaceUri: " + binding.Namespace);

                var bindingElems = binding.CreateBindingElements();
                debug("Binding Elems: " + bindingElems.Count);

                foreach (var bindingElem in bindingElems)
                {
                    debug("binding type: " + bindingElem.GetType().Name);
                }
            }

            foreach (var contract in service.AllContracts)
            {
                //outputDebugInfoForContract(contract);

                
                var schemaImporter = new XmlTypeExtractor(service.AllWsdlDocuments[0].Types.Schemas, debug);
                schemaImporter.ImportDataContracts();

                foreach (var complexType in schemaImporter.ComplexTypes)
                {
                    dataContractWriter.OutputComplexType(complexType.Key, complexType.Value);
                }

                if (generateServiceContract)
                {
                    templateOperations.StartFile(contract.Name + ".generated.cs");

                    //var classGenerator = templateOperations.ClassGenerator(contract.Name, contract.Operations.Select(_ => operationMembers(_)), null);

                    //templateOperations.Write(templateOperations.NamespaceGenerator("Service", classGenerator.TransformText()).TransformText());

                    templateOperations.EndFile();
                }
            }

            templateOperations.DebugFlush();            
        }

//        private IList<MemberDescription> operationMembers(OperationDescription operationDescription)
//        {
//            operationDescription.Name
//        }

        private void outputDebugInfoForContract(ContractDescription contract)
        {
            debug("Behaviours: " + contract.Behaviors.Count);

            foreach (var contractBehavior in contract.Behaviors)
            {
                debug(contractBehavior.ToString());
            }

            foreach (var operation in contract.Operations)
            {
                debug("Operation: " + operation.Name);

                foreach (var message in operation.Messages)
                {
                    debug("\tDirection: " + message.Direction);
                    debug("\tMessage Body");
                    debug("\tParams: " + message.Body.Parts.Count);
                    foreach (var part in message.Body.Parts)
                    {
                        debug("\t\tName: " + part.Name);
                        debug("\t\tType: " + part.Type);
                        debug("\t\tMultiple: " + part.Multiple);
                    }

                    debug("Returns");
                    if (message.Body.ReturnValue != null)
                    {
                        debug("\t\tName: " + message.Body.ReturnValue.Name);
                        debug("\t\tType: " + message.Body.ReturnValue.Type);
                        debug("\t\tMultiple: " + message.Body.ReturnValue.Multiple);
                    }
                }
            }

            debug("Wsdl Documents: " + service.AllWsdlDocuments.Count);
            debug("Wsdl types:" + service.AllWsdlDocuments[0].Types.Schemas.Count);
        }        
    }
}