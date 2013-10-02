using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
        private bool generateServiceContract = true;
        private DataContractWriter dataContractWriter;

        private static readonly AttributeDescription generatedCodeAttribute =
            new AttributeDescription()
                {
                    Name =
                        "System.CodeDom.Compiler.GeneratedCodeAttribute",
                    Arguments = new[]
                                    {
                                        new ArgumentDescription()
                                            {
                                                Value = "\"System.ServiceModel\""
                                            },
                                        new ArgumentDescription()
                                            {Value = "\"4.0.0.0\""},
                                    }
                };

        public ServiceWriter(WebService service, TemplateOperations templateOperations)
        {            
            this.service = service;
            this.templateOperations = templateOperations;

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

            //debug("WsdlDocTypeSchemas: " + service.AllWsdlDocuments[0].Types.Schemas.Count);
            var schemaImporter = new XmlTypeExtractor(service.AllWsdlDocuments[0].Types.Schemas, debug);
            schemaImporter.ImportDataContracts();

            generateDataContracts(schemaImporter);

            foreach (var contract in service.AllContracts)
            {
                if (generateServiceContract)
                {
                    outputDebugInfoForContract(contract);

                    // Create service interface
                    createServiceInterface(contract.Name, contract.Operations, schemaImporter);

                    // Create service Channel
                    createServiceChannel(contract.Name);

                    // Create Service Client
                    foreach (var operation in contract.Operations)
                    {
                        // Create 
                    }


                    templateOperations.StartFile(contract.Name + ".generated.cs");

                    //var classGenerator = templateOperations.ClassGenerator(contract.Name, contract.Operations.Select(_ => operationMembers(_)), null);

                    //templateOperations.Write(templateOperations.NamespaceGenerator("Service", classGenerator.TransformText()).TransformText());

                    templateOperations.EndFile();
                }
            }

            templateOperations.DebugFlush();            
        }

        private void createServiceInterface(string name, OperationDescriptionCollection operations, XmlTypeExtractor schemaImporter)
        {
            var members = operations.Select(_ => operationMember(_, schemaImporter)).ToList();
            var classGenerator = templateOperations.ClassGenerator(name, members, null, new[]
                                                                                         {
                                                                                             generatedCodeAttribute,
                                                                                         }, true);
            templateOperations.CreateFile(name, classGenerator.TransformText());
        }

        private void createServiceChannel(string name)
        {
            var serviceInterfaceName = name;
            var className = serviceInterfaceName + "Channel";
            var classGenerator =
                templateOperations.ClassGenerator(className, null,
                                                  new[] {serviceInterfaceName, "System.ServiceModel.IClientChannel"},
                                                  new[]
                                                      {
                                                          generatedCodeAttribute,
                                                      }, true);            

            templateOperations.CreateFile(className, classGenerator.TransformText());
        }

        private void generateDataContracts(XmlTypeExtractor schemaImporter)
        {            
            templateOperations.Debug("Complex Types: " + schemaImporter.ComplexTypes.Count);
            foreach (var complexType in schemaImporter.ComplexTypes)
            {
                dataContractWriter.OutputComplexType(complexType.Key, complexType.Value);
            }
        }

        private MemberDescription operationMember(OperationDescription operationDescription, XmlTypeExtractor schemaImporter)
        {
            debug("Operation Description");
            debug(DebugUtility.GetProperties(operationDescription));

            var returnMessage = operationDescription.Messages.SingleOrDefault(_ => _.Body != null && _.Body.ReturnValue != null);

            string replyAction = null;

            string returnType = "void";
            if (returnMessage != null)
            {
                debug(operationDescription.Name + " returns");
                if (returnMessage.Direction != MessageDirection.Output)
                    throw new InvalidOperationException("Why is this not output");

                Func<KeyValuePair<string, ComplexType>, bool> existingElementPredicate = _ => _.Key == returnMessage.Body.WrapperNamespace && _.Value.Name == returnMessage.Body.WrapperName;
                if (!schemaImporter.Elements.Any(existingElementPredicate))
                {
                    throw new InvalidOperationException("Couldn't find the element definition");
                }
                var existingElement = schemaImporter.Elements.Single(existingElementPredicate);

                if (existingElement.Value.Properties.Count > 1)
                {
                    foreach (var property in existingElement.Value.Properties)
                    {
                        debug("return property: " + DebugUtility.GetProperties(property));
                    }
                    //throw new InvalidOperationException("A return message should not have more than one property");
                }
                else if (existingElement.Value.Properties.Any())
                {
                    var complexTypeProperty = existingElement.Value.Properties.Single();
                    
                    returnType = complexTypeProperty.ComplexType ?? 
                        XsdTypeEvaluator.GetAlias(complexTypeProperty.Type, debug);
                }
            }
            
            var memberDescription = MemberDescription.Method(operationDescription.Name, returnType, null);
            
            return memberDescription;            
        }

        private void outputDebugInfoForContract(ContractDescription contract)
        {
            debug("Behaviours: " + contract.Behaviors.Count);

            foreach (var contractBehavior in contract.Behaviors)
            {
                debug(contractBehavior.ToString());
            }

            debug("Operations: " + contract.Operations.Count);

            foreach (var operation in contract.Operations)
            {
                debug("Operation: " + operation.Name);

                foreach (var message in operation.Messages)
                {
                    debug("Next Message\n");
                    debug("\tDirection: " + message.Direction);
                    debug("\tMessage Body");
                    debug("\tParams: " + message.Body.Parts.Count);
                    foreach (var part in message.Body.Parts)
                    {
                        debug("\t\tName: " + part.Name);
                        debug("\t\tType: " + part.Type);
                        debug("\t\tMultiple: " + part.Multiple);
                    }

                    debug("\tReturns");
                    if (message.Body.ReturnValue != null)
                    {
                        var retDebug = DebugUtility.GetProperties(message.Body.ReturnValue).Replace("\n", "\n\t\t");
                        debug("\t\t" + retDebug);
                    }
                }
            }

            debug("Wsdl Documents: " + service.AllWsdlDocuments.Count);
            debug("Wsdl types:" + service.AllWsdlDocuments[0].Types.Schemas.Count);
        }        
    }
}