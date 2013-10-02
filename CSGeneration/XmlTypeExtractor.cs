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
                extractTypes(schema);
            }

            foreach (XmlSchema schema in schemas)
            {
                extractElements(schema);
            }            
        }

        private void extractElements(XmlSchema schema)
        {
            var targetNamespaceUri = schema.TargetNamespace;

            debug("Adding elements for " + schema.TargetNamespace);
            addElements(schema, targetNamespaceUri);

            foreach (XmlSchemaObject entry in schema.Includes)
            {
                XmlSchemaImport import = entry as XmlSchemaImport;
                if (import != null)
                {
                    debug("Importing schema: " + import.Namespace);
                    ProcessSchemaImport(import, extractElements);
                }
            }
        }

        private void addElements(XmlSchema schema, string targetNamespaceUri)
        {
            debug("adding elements: " + schema.Elements.Count);
            debug("adding elements items: " + schema.Items.Count);

            foreach (XmlSchemaObject item in schema.Items)
            {
                var element = item as XmlSchemaElement;
                if (element != null && element.SchemaType is XmlSchemaComplexType)
                {
                    processComplexType(targetNamespaceUri, element.SchemaType as XmlSchemaComplexType, element.Name, Elements);
                }

            }
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
                debug("Elem: " + elems.Key + ":" + elems.Value.Name);
                debug(string.Join(", ", elems.Value.Properties.Select(_ => _.Name + ":" + _.Type).ToArray()));
            }
        }

        private void extractTypes(XmlSchema schema)
        {
            debug("Extracting types from: " + schema.TargetNamespace);
            this.AddTypes(schema);

            debug("Include Count :" + schema.Includes.Count);
            foreach (XmlSchemaObject entry in schema.Includes)
            {
                XmlSchemaImport import = entry as XmlSchemaImport;
                if (import != null)
                {
                    debug("Processing schema: " + import.Namespace);
                    ProcessSchemaImport(import, extractTypes);
                }
            }
        }

        private void ProcessSchemaImport(XmlSchemaImport import, Action<XmlSchema> extractImport)
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

                    debug("Compiling schema");
                    import.Schema.Compile(null);
                    debug("Schema downloaded:" + import.SchemaLocation);
                    this.loadedSchemas.Add(import.SchemaLocation);
                }
            }

            if (import.Schema != null)
            {
                extractImport(import.Schema);
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
            debug("Schema elements foud: " + schema.Items.Count);

            foreach (var schemaType in schema.SchemaTypes.Values)
            {
                // TODO: Do we need to bring in includes here?
                debug("SCHEMA Type: " + schemaType.GetType().Name);

                if (schemaType is XmlSchemaComplexType)
                {
                    XmlSchemaComplexType schemaType1 = schemaType as XmlSchemaComplexType;        
                    
                    // Only process if we haven't already found it.
                    if (!ComplexTypes.Any(_ => _.Key == schemaType1.QualifiedName.Namespace && _.Value.Name == schemaType1.QualifiedName.Name))
                    {
                        processComplexType(schemaType1.QualifiedName.Namespace, schemaType1, schemaType1.Name, ComplexTypes);    
                    }
                }
            }

            debug("Collected " + ComplexTypes.Count + " complex types");         
        }

        private bool isParentWsdlDefinition(XmlSchema schema)
        {
            return false;
        }

        void TraverseParticle(XmlSchemaParticle particle, ComplexTypeBuilder complexTypeBuilder)
        {
            debug("Traversing Particle");

            if (particle is XmlSchemaElement)
            {
                XmlSchemaElement elem = particle as XmlSchemaElement;

                if (elem.RefName.IsEmpty)
                {
                    debug("Empty refname");

                    XmlSchemaComplexType complexType = elem.ElementSchemaType as XmlSchemaComplexType;
                    if (complexType != null)
                    {
                        debug("Content Name: " + complexType.QualifiedName);

                        // TODO: Do we need tto match on name as well as namespace?
                        Func<KeyValuePair<string, ComplexType>, bool> existingCTPredicate = _ => _.Key == complexType.QualifiedName.Namespace && _.Value.Name == complexType.QualifiedName.Name;
                        var isExisting = ComplexTypes.Any(existingCTPredicate);
                        debugExistingComplexTypes(complexType);

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
                        debug("Trying simple type");
                        var xmlSchemaSimpleType = 
                            elem.ElementSchemaType != null
                            ? elem.ElementSchemaType as XmlSchemaSimpleType
                            : elem.SchemaType as XmlSchemaSimpleType;
                        debug("EST" + elem.ElementSchemaType);
                        debug("ST" + elem.SchemaType);
                        if (xmlSchemaSimpleType != null && xmlSchemaSimpleType.Datatype != null)
                        {
                            debug("Cast successfull");
                            if (xmlSchemaSimpleType.QualifiedName == null)
                            {
                                throw new NotImplementedException("No qualified name for simple type");
                            }

                            debug("Adding simple type");
                            complexTypeBuilder.AddProperty(elem.Name, XsdTypeEvaluator.OverrideCLRType(xmlSchemaSimpleType.Datatype.ValueType, xmlSchemaSimpleType.QualifiedName.ToString()));                            
                        }
                        else
                        {
                            debug("Couldn't cast to simple type: ");
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
                {
                    if (subParticle is XmlSchemaGroupBase || subParticle is XmlSchemaElement)
                    {
                        debug("Sub particle found: " + subParticle.GetType());
                        TraverseParticle(subParticle, complexTypeBuilder);
                    }
                }
            }
            else
            {
                debug("Current stack: " + new System.Diagnostics.StackTrace());
                debug(DebugUtility.GetProperties(particle));                                
                throw new NotImplementedException("Particle is not group of element");
            }
        }

        private void debugExistingComplexTypes(XmlSchemaComplexType complexType)
        {
            debug("Looking up existing type with " + complexType.QualifiedName.Namespace + ":" +
                  complexType.QualifiedName.Name);
            foreach (var ctype in ComplexTypes)
            {
                debug("Existing CT " + ctype.Key + ":" + ctype.Value.Name);
                foreach (var property in ctype.Value.Properties)
                {
                    debug("prop: " + property.Name);
                    if (property.Type != null)
                    {
                        debug("type: " + property.Type.Name);
                    }
                    if (property.ComplexType != null)
                    {
                        debug("ctype: " + property.ComplexType);
                    }
                }
            }
        }

        private void processComplexType(string targetNamespaceUri, XmlSchemaComplexType schemaType, string name, IList<KeyValuePair<string, ComplexType>> typeBucket)
        {
            debug("Processing Complex Type: " + targetNamespaceUri + ":" + name);
            debug("Particle: " + schemaType.Particle);
            debug("ContentParticle: " + schemaType.ContentTypeParticle.MaxOccurs);

            var particle = (schemaType.ContentTypeParticle is XmlSchemaElement ||
                            schemaType.ContentTypeParticle is XmlSchemaGroupBase)
                               ? schemaType.ContentTypeParticle
                               : schemaType.Particle;

            var complexTypeBuilder = ComplexTypeBuilder.Start(name);
            TraverseParticle(particle, complexTypeBuilder);
            var complexType = complexTypeBuilder.Build();

            // TODO: Here we have created the same complex type many times and we are hiding the issue
            // By throwing it away. This is most likely caused because we deal with items directly (uncompiled resources) but also Elements (compiled resources)
            if (!typeBucket.Any(_ => _.Key == targetNamespaceUri && _.Value.Name == complexType.Name))
            {
                typeBucket.Add(new KeyValuePair<string, ComplexType>(targetNamespaceUri, complexType));
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
