using System;
using System.Collections.Generic;

namespace CSGeneration
{
    public class ComplexType
    {
        public ComplexType()
        {
            Properties = new List<ComplexTypeProperty>();
        }

        public virtual string Name { get; set; }

        public IList<ComplexTypeProperty> Properties { get; set; }
 
        public class ComplexTypeProperty
        {
            public string Name { get; set; }

            public Type Type { get; set; }
        }
    }
}