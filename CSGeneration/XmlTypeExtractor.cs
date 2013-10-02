using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CSGeneration
{
    public class XmlTypeExtractor
    {
        private readonly XmlSchemas schemas;
        private readonly Action<string> debug;
        private List<string> loadedSchemas;
        private XmlSchemaSet schemaSet;

        public IList<KeyValuePair<string, ComplexType>> ComplexTypes { get; set; }
        public IList<KeyValuePair<string, ComplexType>> Elements { get; set; }

        public XmlTypeExtractor(XmlSchemas schemas, Action<string> debug)
        {
            this.schemas = schemas;
            this.debug = debug;
            this.loadedSchemas = new List<string>();
            this.schemaSet = new XmlSchemaSet();        
            ComplexTypes = new List<KeyValuePair<string, ComplexType>>();
            Elements = new List<KeyValuePair<string, ComplexType>>();
        }

        public void ImportDataContracts()
        {
            debug("Importing data contracts: " + schemas.Count);

            foreach (XmlSchema schema in schemas)
            {
                ExtractTypes(schema);
            }

            foreach (XmlSchema schema in schemas)
            {
                extractElements(schema);
            }            
        }

        private void extractElements(XmlSchema schema)
        {
            var targetNamespaceUri = schema.TargetNamespace;

            foreach (XmlSchemaElement element in schema.Elements.Values)
            {
                if (element.SchemaType is XmlSchemaComplexType)
                {
                    processComplexType(targetNamespaceUri, element.SchemaType as XmlSchemaComplexType, element.Name, Elements);
                }
            }

            debug("Elements Found: " + Elements.Count);
            foreach (var elems in Elements)
            {
                debug("Elem: " + elems.Value.Name);
                debug(string.Join(", ", elems.Value.Properties.Select(_ => _.Name + ":" + _.Type).ToArray()));
            }
        }

        private void ExtractTypes(XmlSchema schema)
        {
            this.AddTypes(schema);
            foreach (XmlSchemaObject entry in schema.Includes)
            {
                XmlSchemaImport import = entry as XmlSchemaImport;
                if (import != null)
                {
                    ProcessSchemaImport(import);
                }
            }
        }

        private void ProcessSchemaImport(XmlSchemaImport import)
        {
            if (import.Schema == null)
            {
                if (import.SchemaLocation != null && !this.loadedSchemas.Contains(import.SchemaLocation))
                {
                    WebClient c = new WebClient();
                    byte[] schemaBytes = c.DownloadData(import.SchemaLocation);
                    using (MemoryStream ms = new MemoryStream(schemaBytes))
                    {
                        import.Schema = XmlSchema.Read(ms, null);
                    }

                    this.loadedSchemas.Add(import.SchemaLocation);
                }
            }

            if (import.Schema != null)
            {
                this.ExtractTypes(import.Schema);
            }
        }

        public class ElementDescription
        {
            public string Name { get; set; }

            public string Namespace { get; set; }

            public IList<ParameterDescription> Parameters { get; set; }

            public class ParameterDescription
            {
                public string Name { get; set; }

                public string Type { get; set; }
            }
        }

        private void AddTypes(XmlSchema schema)
        {
            var targetNamespaceUri = schema.TargetNamespace;

            debug("Schema adding types: " + schema.SchemaTypes.Values.Count);
            foreach (var schemaType in schema.SchemaTypes.Values)
            {
                debug("SCHEMA Type: " + schemaType.GetType().Name);

                if (schemaType is XmlSchemaComplexType)
                {
                    XmlSchemaComplexType schemaType1 = schemaType as XmlSchemaComplexType;
                    processComplexType(targetNamespaceUri, schemaType1, schemaType1.Name, ComplexTypes);
                }
            }

            debug("Collected " + ComplexTypes.Count + " complex types");

            return;
//
//            debug("Adding schema");
//            debug(schema.ToString());
//            new XmlTextReader(schema.ToString());
//            foreach (DictionaryEntry element in schema.Elements)
//            {                
//                var xmlSchemaElement = (XmlSchemaElement)element.Value;
//                if (xmlSchemaElement.ElementSchemaType.BaseXmlSchemaType == null)
//                    continue;
//
//
//                //var elemProps = DebugUtility.GetProperties(xmlSchemaElement.ElementSchemaType.);
//                //debug("Schema Element: " + elemProps);
//                //debug("SCHEMA VALUE TYPE: " + xmlSchemaElement.ElementSchemaType.Datatype.ValueType);
//            }
//            return;
//            MemoryStream ms = new MemoryStream();
//            schema.Write(ms);
//            ms.Position = 0;
//            XmlDocument doc = new XmlDocument();
//            doc.Load(ms);
//            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
//            nsManager.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");
//            
//            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
//            {
//                if (node.NodeType == XmlNodeType.Element)
//                {
//                    if (node.LocalName == "complexType")
//                    {
//                        ComplexTypes.Add(new KeyValuePair<string, XmlNode>(targetNamespaceUri, node));
//                    }
//                }
//            }

//            XmlWriter w = XmlWriter.Create(ms);
//            doc.WriteTo(w);
//            w.Flush();
//            ms.Position = 0;
//            this.schemaSet.Add(XmlSchema.Read(ms, null));            
        }

        void TraverseParticle(XmlSchemaParticle particle, ComplexTypeBuilder complexTypeBuilder)
        {
            if (particle is XmlSchemaElement)
            {
                XmlSchemaElement elem = particle as XmlSchemaElement;

                if (elem.RefName.IsEmpty)
                {
                    XmlSchemaComplexType complexType = elem.ElementSchemaType as XmlSchemaComplexType;
                    if (complexType != null)
                    {
                        debug("Content Name: " + complexType.QualifiedName);

                        // TODO: Do we need tto match on name as well as namespace?
                        Func<KeyValuePair<string, ComplexType>, bool> existingCTPredicate = _ => _.Key == complexType.QualifiedName.Namespace && _.Value.Name == complexType.QualifiedName.Name;
                        var isExisting = ComplexTypes.Any(existingCTPredicate);
                        debug("Looking up existing type with " + complexType.QualifiedName.Namespace + ":" +
                              complexType.QualifiedName.Name);
                        foreach (var ctype in ComplexTypes)
                        {
                            debug("Existing CT " + ctype.Key + ":" + ctype.Value.Name);
                        }

                        if (isExisting)
                        {
                            var existingComplexType = ComplexTypes.Single(existingCTPredicate);
                            debug("Found existing complex type " + existingComplexType.Value.Name);

                            complexTypeBuilder.AddComplexType(elem.Name, existingComplexType.Value, existingComplexType.Key);
                        }
                        else
                        {
                            TraverseParticle(complexType.ContentTypeParticle, complexTypeBuilder);
                        }
                    }
                    else
                    {
                        var xmlSchemaSimpleType = elem.ElementSchemaType as XmlSchemaSimpleType;                        

                        if (xmlSchemaSimpleType != null && xmlSchemaSimpleType.Datatype != null)
                        {
                            complexTypeBuilder.AddProperty(elem.Name, XsdTypeEvaluator.OverrideCLRType(xmlSchemaSimpleType.Datatype.ValueType, xmlSchemaSimpleType.QualifiedName.ToString()));                            
                        }
                        else
                        {
                            throw new NotImplementedException("Haven't implemented case of not having datatype. " + elem.ElementSchemaType.GetType().Name);
                        }
                    }
                }
                else
                {
                    throw new NotImplementedException("Haven't dealt with filled refname");
                }
            }
            else if (particle is XmlSchemaGroupBase)
            {
                //xs:all, xs:choice, xs:sequence
                XmlSchemaGroupBase baseParticle = particle as XmlSchemaGroupBase;
                foreach (XmlSchemaParticle subParticle in baseParticle.Items)
                    TraverseParticle(subParticle, complexTypeBuilder);
            }
        }

        private void processComplexType(string targetNamespaceUri, XmlSchemaComplexType schemaType, string name, IList<KeyValuePair<string, ComplexType>> typeBucket)
        {
            var complexTypeBuilder = ComplexTypeBuilder.Start(name);
            TraverseParticle(schemaType.ContentTypeParticle, complexTypeBuilder);
            var complexType = complexTypeBuilder.Build();
            typeBucket.Add(new KeyValuePair<string, ComplexType>(targetNamespaceUri, complexType));
        }

        private class ComplexTypeBuilder
        {
            private static ComplexType complexType;

            private ComplexTypeBuilder(string name)
            {
                complexType = new ComplexType()
                                  {
                                      Name = name,
                                  };
            }

            public static ComplexTypeBuilder Start(string name)
            {
                var builder = new ComplexTypeBuilder(name);
                
                return builder;
            }

            public virtual ComplexType Build()
            {
                return complexType;
            }

            public ComplexTypeBuilder AddProperty(string name, Type valueType)
            {
                complexType.Properties.Add(new ComplexType.ComplexTypeProperty() {Name = name, Type = valueType});
                return this;
            }

            public ComplexTypeBuilder AddComplexType(string name, ComplexType value, string namespaceUri)
            {
                var ns = NamespaceUtility.NamespaceName(namespaceUri);

                complexType.Properties.Add(new ComplexType.ComplexTypeProperty() {Name = name, ComplexType = ns + "." + value.Name });
                return this;
            }
        }
    }
}
