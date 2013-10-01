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

        public XmlTypeExtractor(XmlSchemas schemas, Action<string> debug)
        {
            this.schemas = schemas;
            this.debug = debug;
            this.loadedSchemas = new List<string>();
            this.schemaSet = new XmlSchemaSet();        
            ComplexTypes = new List<KeyValuePair<string, ComplexType>>();
        }

        public void ImportDataContracts()
        {
            debug("Importing data contracts");

            foreach (XmlSchema schema in schemas)
            {
                ProcessSchema(schema);
            }
        }

        private void ProcessSchema(XmlSchema schema)
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
                this.ProcessSchema(import.Schema);
            }
        }

        void TraverseParticle(XmlSchemaParticle particle, ComplexTypeBuilder complexTypeBuilder)
        {
            if (particle is XmlSchemaElement)
            {
                XmlSchemaElement elem = particle as XmlSchemaElement;

                if (elem.RefName.IsEmpty)
                {
                    XmlSchemaComplexType complexType = elem.ElementSchemaType as XmlSchemaComplexType;
                    if (complexType != null && complexType.Name == null)
                    {
                        TraverseParticle(complexType.ContentTypeParticle, complexTypeBuilder);
                    }
                    else
                    {
                        var xmlSchemaSimpleType = elem.ElementSchemaType as XmlSchemaSimpleType;                        

                        if (xmlSchemaSimpleType.Datatype != null)
                        {
                            complexTypeBuilder.AddProperty(elem.Name, XsdTypeEvaluator.OverrideCLRType(xmlSchemaSimpleType.Datatype.ValueType, xmlSchemaSimpleType.QualifiedName.ToString()));                            
                        }
                        else
                        {
                            throw new NotImplementedException("Haven't implemented case of not having datatype");
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

            public void AddProperty(string name, Type valueType)
            {
                complexType.Properties.Add(new ComplexType.ComplexTypeProperty() {Name = name, Type = valueType});
            }
        }

        private void AddTypes(XmlSchema schema)
        {
            var targetNamespaceUri = schema.TargetNamespace;

            foreach (var schemaType in schema.SchemaTypes.Values)
            {
                // Here we are ignoring simple types assuming we don't need to import them
                // We may need to cater for specific validation automatically.
                var xmlSchemaComplexType = schemaType as XmlSchemaComplexType;
                if (xmlSchemaComplexType == null) continue;

                var complexTypeBuilder = ComplexTypeBuilder.Start(xmlSchemaComplexType.Name);
                TraverseParticle(xmlSchemaComplexType.ContentTypeParticle, complexTypeBuilder);
                var complexType = complexTypeBuilder.Build();
                ComplexTypes.Add(new KeyValuePair<string, ComplexType>(targetNamespaceUri, complexType));
            }

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
    }
}
