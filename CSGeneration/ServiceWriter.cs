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

        public ServiceWriter(WebService service, TemplateOperations templateOperations)
        {            
            this.service = service;
            this.templateOperations = templateOperations;

            debug = templateOperations.Debug;

            dataContractWriter = new DataContractWriter(templateOperations);
        }

        public void WriteToOutput()
        {
            try
            {
                var schemaImporter = new XmlTypeExtractor(service.AllWsdlDocuments[0].Types.Schemas, debug);
                schemaImporter.ImportDataContracts();

                generateDataContracts(schemaImporter);

                foreach (var contract in service.AllContracts)
                {
                    if (generateServiceContract)
                    {
                        outputDebugInfoForContract(contract);

                        createServiceInterface(contract.Name, contract.Operations, schemaImporter);
                        createServiceChannel(contract.Name);
                        createServiceClient(contract.Name, contract.Operations, schemaImporter);
                    }
                }
            }
            finally
            {
                templateOperations.DebugFlush();
            }
        }

        private void createServiceClient(string name, OperationDescriptionCollection operations, XmlTypeExtractor schemaImporter)
        {
            var interfaceMembers = operations.Select(_ => operationMember(_, schemaImporter)).ToList();
            foreach (var memberDescription in interfaceMembers)
            {
                memberDescription.Attributes = new List<AttributeDescription>();
                memberDescription.Scope = MemberDescription.MethodScopeValues.Public;
                // TODO: Extract to template/make use of arguments properly
                memberDescription.SetBody("return base.Channel." +
                                          memberDescription.Name +
                                          "(" +
                                          string.Join(", ", memberDescription.Arguments.Select(_ => _.Name).ToArray()) +
                                          ");");           
            }
            
            var baseTypes = new[] {"System.ServiceModel.ClientBase<" + name + ">", name};
            string className = name.Remove(0, 1) + "Client";

            var members = new List<MemberDescription>();

            var endpointConfigArg = new MemberDescription.ArgumentDefinition() {Name = "endpointConfigurationName", Type = "string",};
            var remoteAddressArg = new MemberDescription.ArgumentDefinition() { Name = "remoteAddress", Type = "string", };
            var remoteAddressStrongArg = new MemberDescription.ArgumentDefinition() { Name = "remoteAddress", Type = "System.ServiceModel.EndpointAddress", };
            var bindingArg = new MemberDescription.ArgumentDefinition() { Name = "binding", Type = "System.ServiceModel.Channels.Binding", };
            
            members.Add(MemberDescription.Method(className, null, null, null,
                                                 MemberDescription.MethodScopeValues.Public, false, true));
            var constructor2 = MemberDescription.Method(className, null, new[] {endpointConfigArg,}, null, MemberDescription.MethodScopeValues.Public, false, true);
            constructor2.SetBaseCall(new[] { new ArgumentDescription() { Value = "endpointConfigurationName" }, });
            members.Add(constructor2);

            var constructor3 = MemberDescription.Method(className, null, new[] {endpointConfigArg, remoteAddressArg}, null, MemberDescription.MethodScopeValues.Public, false, true);
            constructor3.SetBaseCall(new[] { new ArgumentDescription() { Value = "endpointConfigurationName" }, new ArgumentDescription() {Value = "remoteAddress"}, });
            members.Add(constructor3);
            
            var constructor4 = MemberDescription.Method(className, null, new[] {endpointConfigArg, remoteAddressStrongArg}, null, MemberDescription.MethodScopeValues.Public, false, true);
            constructor4.SetBaseCall(new[] { new ArgumentDescription() { Value = "endpointConfigurationName" }, new ArgumentDescription() { Value = "remoteAddress" }, });
            members.Add(constructor4);

            var constructor5 = MemberDescription.Method(className, null, new[] {bindingArg, remoteAddressStrongArg}, null, MemberDescription.MethodScopeValues.Public, false, true);
            constructor5.SetBaseCall(new[] { new ArgumentDescription() { Value = "binding" }, new ArgumentDescription() { Value = "remoteAddress" }, });
            members.Add(constructor5);

            members.AddRange(interfaceMembers);
            var classGenerator = templateOperations.ClassGenerator(
                className,
                members,
                baseTypes,
                new[]
                    {
                        Attributes.GeneratedCodeAttribute,
                    },
                false);
            templateOperations.CreateFile(className, classGenerator.TransformText());
        }

        private void createServiceInterface(string name, OperationDescriptionCollection operations, XmlTypeExtractor schemaImporter)
        {
            var members = operations.Select(_ => operationMember(_, schemaImporter)).ToList();
            var classGenerator = templateOperations.ClassGenerator(
                name,
                members,
                null,
                new[]
                    {
                        Attributes.GeneratedCodeAttribute,
                        Attributes.ServiceContractAttribute(name),
                    },
                true);
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
                                                          Attributes.GeneratedCodeAttribute,
                                                      }, true);            

            templateOperations.CreateFile(className, classGenerator.TransformText());
        }

        private void generateDataContracts(XmlTypeExtractor schemaImporter)
        {            
            templateOperations.Debug("Complex Types: " + schemaImporter.ComplexTypes.Count);
            foreach (var complexType in schemaImporter.ComplexTypes)
            {
                debug("Writing Complex Type: " + complexType.Key + " " + complexType.Value.Name);
                dataContractWriter.OutputComplexType(complexType.Key, complexType.Value);
            }
        }

        private MemberDescription operationMember(OperationDescription operationDescription, XmlTypeExtractor schemaImporter)
        {
            debug("Operation Description");
            debug(DebugUtility.GetProperties(operationDescription));

            if (operationDescription.Messages.Count > 2)
            {
                throw new NotImplementedException("We only expect 2 messages per operation. Args + return");
            }

            var returnMessage = operationDescription.Messages.SingleOrDefault(_ => _.Body != null && _.Body.ReturnValue != null);

            string replyAction = null;            
            string returnType = "void";
            if (returnMessage != null)
            {
                replyAction = returnMessage.Action;
                returnType = getReturnType(operationDescription, schemaImporter, returnMessage);
            }

            // Get arguments
            MessageDescription argsMessage = operationDescription.Messages.SingleOrDefault(_ => _.Body.ReturnValue == null);
            var action = argsMessage.Action;

            var args = getArguments(schemaImporter, argsMessage);

            var memberDescription = MemberDescription.Method(operationDescription.Name, returnType, args, new []{Attributes.OperationContractAttribute(replyAction, action)}, MemberDescription.MethodScopeValues.None, false, false);
            
            return memberDescription;            
        }

        private List<MemberDescription.ArgumentDefinition> getArguments(XmlTypeExtractor schemaImporter, MessageDescription argsMessage)
        {
            var args = new List<MemberDescription.ArgumentDefinition>();
            var existingElement = getExistingElement(schemaImporter, argsMessage);
            foreach (var property in existingElement.Value.Properties)
            {
                args.Add(new MemberDescription.ArgumentDefinition()
                             {
                                 Name = property.Name,
                                 Type = property.ComplexType
                                        ?? XsdTypeEvaluator.GetAlias(property.Type, debug),
                             });
            }
            return args;
        }

        private string getReturnType(OperationDescription operationDescription, XmlTypeExtractor schemaImporter,
                                     MessageDescription returnMessage)
        {
            string returnType = "void";
            debug(operationDescription.Name + " returns");
            if (returnMessage.Direction != MessageDirection.Output)
                throw new InvalidOperationException("Why is this not output");

            debug("Return Message Wrapper: " + returnMessage.Body.WrapperName + " " +
                  returnMessage.Body.WrapperNamespace);
            
            var existingElement = getExistingElement(schemaImporter, returnMessage);

            if (existingElement.Value.Properties.Count > 1)
            {
                foreach (var property in existingElement.Value.Properties)
                {
                    debug("return property: " + DebugUtility.GetProperties(property));
                }
                throw new InvalidOperationException("A return message should not have more than one property");
            }
            else if (existingElement.Value.Properties.Any())
            {
                debug("Found return property");
                var complexTypeProperty = existingElement.Value.Properties.Single();

                returnType = complexTypeProperty.ComplexType ??
                             XsdTypeEvaluator.GetAlias(complexTypeProperty.Type, debug);
            }

            debug("return type is: " + returnType);
            return returnType;
        }

        private static KeyValuePair<string, ComplexType> getExistingElement(XmlTypeExtractor schemaImporter, MessageDescription message)
        {            
            Func<KeyValuePair<string, ComplexType>, bool> existingElementPredicate =
                _ => _.Key == message.Body.WrapperNamespace && _.Value.Name == message.Body.WrapperName;
            if (!schemaImporter.Elements.Any(existingElementPredicate))
            {
                throw new InvalidOperationException("Couldn't find the element definition");
            }
            var existingElement = schemaImporter.Elements.Single(existingElementPredicate);
            return existingElement;
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