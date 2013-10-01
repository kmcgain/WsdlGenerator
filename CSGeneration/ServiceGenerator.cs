using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Web;

namespace CSGeneration
{
    public class ServiceGenerator
    {
        // Wrap the service path into a WCF endpoint address
        public static string GetServiceReferences(string servicePath, string @namespace)
        {
            return GenerateCSCodeForService(@namespace, new EndpointAddress(servicePath));
        }

        // Code to generate the proxy
        public static string GenerateCSCodeForService(string gennamespace, EndpointAddress metadataAddress)
        {
            var mexClient = new MetadataExchangeClient(metadataAddress.Uri, MetadataExchangeClientMode.HttpGet);
            mexClient.ResolveMetadataReferences = true;

            var metadata = mexClient.GetMetadata(metadataAddress.Uri, MetadataExchangeClientMode.HttpGet);
            var metadataSet = new MetadataSet(metadata.MetadataSections);

            var importer = new WsdlImporter(metadataSet);

            var importAllContracts = importer.ImportAllContracts();
            var codeDomProvider = CodeDomProvider.CreateProvider("CSharp");
            var output = new StringWriter();
            return string.Empty;
            //importer.ImportAllBindings()
            //return codeDomExperiment(gennamespace, codeDomProvider, output, importAllContracts);
//            var generator = new ServiceContractGenerator();
//            generator.Options = ServiceContractGenerationOptions.TypedMessages
//                //| ServiceContractGenerationOptions.AsynchronousMethods
//                //| ServiceContractGenerationOptions.ClientClass
//                //| ServiceContractGenerationOptions.EventBasedAsynchronousMethods
//                //| ServiceContractGenerationOptions.InternalTypes
//                ;
//            // This line enables a namespace wrapping all classes
//            generator.NamespaceMappings.Add("*", gennamespace);
//            //generator.NamespaceMappings.Add("?", gennamespace); // Contract descript namespace?
//            importer.State.Remove(typeof (XsdDataContractExporter)); // This doesn't do anything in the sample
//
//            var importer2 = new XsdDataContractImporter();
//            var options2 = new ImportOptions();
//            options2.EnableDataBinding = true; // HERE we implement INotifyPropertyChanged
//            options2.GenerateInternal = false; // Control if class is internal
//            importer2.Options = options2;
//            importer2.Options.Namespaces.Add("*", gennamespace);
//            // Not used in sample but presumably allows array types to be mapped to a specific ienumerable<>
//            importer2.Options.ReferencedCollectionTypes.Add(typeof (ObservableCollection<>));
//
//            // Here we override the default importer with our own,
//            importer.State.Add(typeof (XsdDataContractImporter), importer2);
//
//
//            Collection<ContractDescription> collection = importer.ImportAllContracts();
//            importer.ImportAllEndpoints();
//            foreach (ContractDescription description in collection)
//            {
//                generator.GenerateServiceContractType(description);
//            }
//            if (generator.Errors.Count != 0)
//            {
//                generator.Errors.ToList().ForEach(
//                    mce => Console.WriteLine("{0}: {1}", mce.IsWarning ? "Warning" : "Error", mce.Message));
//                throw new Exception("There were errors during code compilation.");
//            }
//            //new WcfSilverlightCodeGenerationExtension().ClientGenerated(generator);
//            var options = new CodeGeneratorOptions();
//
//            var provider = CodeDomProvider.CreateProvider("C#");
//
//            var sb = new StringBuilder();
//            var sw = new StringWriter(sb);
//
//            using (var writer = new IndentedTextWriter(sw))
//            {
//                provider.GenerateCodeFromCompileUnit(generator.TargetCompileUnit, writer, options);
//            }
//
//            return sb.ToString();
        }

//        private static string codeDomExperiment(string gennamespace, CodeDomProvider codeDomProvider, StringWriter output,
//                                                Collection<ContractDescription> importAllContracts)
//        {
//            var nameSpaceElem = new System.CodeDom.CodeNamespace(gennamespace);
//
//            foreach (var contractDescription in importAllContracts)
//            {
//                var codeTypeDeclaration = new System.CodeDom.CodeTypeDeclaration(contractDescription.Name);
//
//                foreach (var operationDescription in contractDescription.Operations)
//                {
//                    var member = new CodeTypeMember();
//                    member.Name = operationDescription.Name;
//                    codeTypeDeclaration.Members.Add(member);
//                    //var codeMemberProperty = getCodeProperty(operationDescription);
//                    //codeTypeDeclaration.Members.Add(codeMemberProperty);
//                }
//
//                nameSpaceElem.Types.Add(codeTypeDeclaration);
//            }
//            codeDomProvider.GenerateCodeFromNamespace(nameSpaceElem, new IndentedTextWriter(output),
//                                                      new CodeGeneratorOptions()
//                                                          {
//                                                              IndentString = "    ",
//                                                              BracingStyle = "C",
//                                                              VerbatimOrder = true,
//                                                              BlankLinesBetweenMembers = true,
//                                                          });
//
//            return output.ToString();
//        }
//
//        private static CodeMemberProperty getCodeProperty(OperationDescription operationDescription)
//        {
//            var codeMemberProperty = new System.CodeDom.CodeMemberProperty()
//                                         {
//                                             HasGet = true,
//                                             HasSet = true,
//                                             Name = operationDescription.Name,
//                                             Type = new CodeTypeReference(typeof (string))
//                                         };
//            codeMemberProperty.GetStatements.Add(
//                new CodeMethodReturnStatement(new CodeFieldReferenceExpression(
//                                                  new CodeThisReferenceExpression(), operationDescription.Name)));
//            codeMemberProperty.Attributes = MemberAttributes.Public | MemberAttributes.Final;
//
//            codeMemberProperty.SetStatements.Add(
//                new CodeAssignStatement(
//                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),
//                                                     operationDescription.Name),
//                    new CodePropertySetValueReferenceExpression()));
//            return codeMemberProperty;
//        }
    }

}