using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace CSGeneration
{
    public class DataContractWriter
    {
        private readonly TemplateOperations templateOperations;

        private AttributeDescription[] dataContractAttributes =
            new[]
                {
                    new AttributeDescription()
                        {
                            Name = "System.Diagnostics.DebuggerStepThroughAttribute",
                        },
                    new AttributeDescription()
                        {
                            Name = "System.CodeDom.Compiler.GeneratedCodeAttribute",
                            Arguments = new[]
                                            {
                                                new ArgumentDescription() {Value = "\"System.Runtime.Serialization\""},
                                                new ArgumentDescription() {Value = "\"4.0.0.0\""},
                                            }
                        },
                    new AttributeDescription()
                        {
                            Name = "System.Runtime.Serialization.DataContractAttribute",
                            Arguments = new[]
                                            {
                                                new ArgumentDescription()
                                                    {
                                                        Name = "Name",
                                                        Value = "\"CompositeType\"",
                                                    },
                                                new ArgumentDescription()
                                                    {
                                                        Name = "Namespace",
                                                        Value = "\"http://schemas.datacontract.org/2004/07/WcfService\"",
                                                    },
                                            }
                        },
                };

        public DataContractWriter(TemplateOperations templateOperations)
        {
            this.templateOperations = templateOperations;
        }

        public void OutputComplexType(string namespaceUri, ComplexType node)
        {
            var name = node.Name;

            templateOperations.StartFile(name + ".generated.cs");

            var members = new List<MemberDescription>();

            members.Add(MemberDescription.Field("extensionDataField", "System.Runtime.Serialization.ExtensionDataObject"));
            members.Add(MemberDescription.Property("ExtensionData", "System.Runtime.Serialization.ExtensionDataObject", "extensionData", null));

            addMembersForType(node, members);

            var nsSegments = namespaceUri.Split('/');
            var namespaceName = String.IsNullOrEmpty(nsSegments.Last())
                                    ? nsSegments[nsSegments.Length - 1]
                                    : nsSegments.Last();

            var baseTypes = new[] { "object", "System.Runtime.Serialization.IExtensibleDataObject" };
            var typeClass = templateOperations.ClassGenerator
                (name, members, baseTypes,
                 dataContractAttributes);

            templateOperations.WriteLine(templateOperations.NamespaceGenerator(namespaceName, typeClass.TransformText()).TransformText());

            templateOperations.EndFile();
        }

        private void addMembersForType(ComplexType node, List<MemberDescription> members)
        {
            foreach (var complexTypeProperty in node.Properties)
            {
                members.Add(MemberDescription.Field(
                    complexTypeProperty.Name + "Field", 
                    XsdTypeEvaluator.GetAlias(complexTypeProperty.Type, templateOperations.Debug)));

                members.Add(MemberDescription.Property(
                    complexTypeProperty.Name,
                    XsdTypeEvaluator.GetAlias(complexTypeProperty.Type, templateOperations.Debug),
                    null,
                        new[]
                            {
                                new AttributeDescription()
                                    {
                                        Name =
                                            "System.Runtime.Serialization.DataMemberAttribute"
                                    },
                            }));

            }
        }
    }
}