using System.Collections.Generic;

namespace CSGeneration
{
    public class MemberDescription
    {
        protected MemberDescription()
        {
        }

        public static MemberDescription Method(string name, string returnType, IList<ArgumentDescription> arguments)
        {
            var memberDescription = new MemberDescription()
                                        {
                                            MemberType = MemberTypeValues.Method,
                                            Arguments = arguments,
                                            Name = name,
                                            Type = returnType,
                                        };

            return memberDescription;
        }

        public static MemberDescription Property(string name, string type, string referenceField, IList<AttributeDescription> attributes)
        {
            return new MemberDescription()
                       {
                           MemberType = MemberTypeValues.Property,
                           Name = name,
                           Type = type,
                           ReferenceField = referenceField,
                           Attributes = attributes,
                       };
        }

        public static MemberDescription Field(string propertyName, string aliasedType)
        {
            return new MemberDescription()
                       {
                           MemberType = MemberTypeValues.Field,
                           Name = propertyName,
                           Type = aliasedType,
                       };
        }

        public enum MemberTypeValues
        {
            Method,
            Property,
            Field,
        }

        public MemberTypeValues MemberType { get; protected set; }

        public virtual string Name { get; protected set; }

        public virtual string Type { get; protected set; }
        
        public virtual string ReferenceField { get; set; }

        public virtual IList<AttributeDescription> Attributes { get; protected set; }

        public virtual IList<ArgumentDescription> Arguments { get; protected set; }

        public class ArgumentDescription
        {
            public string Name { get; set; }
            public string Type { get; set; }
        }
    }
}