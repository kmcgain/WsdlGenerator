using System.Xml.Linq;
using System.Xml.Serialization;

namespace CSGeneration
{
    public static class XsdConvert
    {
        private static XmlSerializer serializer = new XmlSerializer(typeof(XmlValueWrapper));


        public static object ConvertFrom(string value, string xsdType)
        {
            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
            XNamespace xsd = "http://www.w3.org/2001/XMLSchema";
            XNamespace ser = "http://schemas.microsoft.com/2003/10/Serialization/";
            XDocument doc = new XDocument(
                new XElement("XmlValueWrapper",
                             new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                             new XAttribute(XNamespace.Xmlns + "xs", xsd),
                             new XAttribute(XNamespace.Xmlns + "ser", ser),
                             new XElement("Value",
                                          new XAttribute(xsi + "type", xsdType),
                                          new XText(value))
                    )
                );
                
            using (var reader = doc.CreateReader())
            {
                XmlValueWrapper wrapper = (XmlValueWrapper)serializer.Deserialize(reader);                    
                return wrapper.Value;
            }
        }

    }
}