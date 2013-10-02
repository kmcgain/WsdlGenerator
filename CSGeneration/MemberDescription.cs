using System.Collections.Generic;

namespace CSGeneration
{
    public class MemberDescription
    {
        public enum MethodScopeValues
        {
            Public,
            Private,
            Protected,
            None,
        }

        protected MemberDescription()
        {
            Scope = MethodScopeValues.Public;
            Arguments = new List<ArgumentDefinition>();
            IsVirtual = true;
        }

        public static MemberDescription Method(string name, string returnType, IList<ArgumentDefinition> arguments, IList<AttributeDescription> attributes, MethodScopeValues methodScope, bool isVirtual, bool hasBody)
        {
            var memberDescription = new MemberDescription()
                                        {
                                            MemberType = MemberTypeValues.Method,
                                            Arguments = arguments,
                                            Name = name,
                                            Type = returnType,
                                            Scope = methodScope,
                                            IsVirtual = isVirtual,
                                            Attributes = attributes,
                                            HasBody = hasBody,
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

        public virtual MethodScopeValues Scope { get; set; }

        public virtual bool IsVirtual { get; set; }
        
        public virtual bool HasBody { get; private set; }

        public virtual IList<AttributeDescription> Attributes { get; set; }

        public virtual IList<ArgumentDefinition> Arguments { get; protected set; }

        public virtual string Body { get; private set; }

        public virtual bool HasBaseCall { get; private set; }

        public class ArgumentDefinition
        {
            public string Name { get; set; }
            public string Type { get; set; }
        }

        public virtual void SetBody(string body)
        {            
            Body = body;
            HasBody = !string.IsNullOrEmpty(body);
        }

        public virtual void SetBaseCall(IList<ArgumentDescription> baseArgs)
        {
            HasBaseCall = baseArgs != null;

            if (!HasBaseCall)
            {
                return;
            }

            BaseCallArgs = baseArgs;
        }

        public IList<ArgumentDescription> BaseCallArgs { get; private set; }
    }
}