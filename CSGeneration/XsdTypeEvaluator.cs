using System;
using System.Collections.Generic;

namespace CSGeneration
{
    public static class XsdTypeEvaluator
    {
        private static readonly Dictionary<Type, string> aliases =
            new Dictionary<Type, string>()
                {
                    { typeof(byte), "byte" },
                    { typeof(sbyte), "sbyte" },
                    { typeof(short), "short" },
                    { typeof(ushort), "ushort" },
                    { typeof(int), "int" },
                    { typeof(uint), "uint" },
                    { typeof(long), "long" },
                    { typeof(ulong), "ulong" },
                    { typeof(float), "float" },
                    { typeof(double), "double" },
                    { typeof(decimal), "decimal" },
                    { typeof(object), "object" },
                    { typeof(bool), "bool" },
                    { typeof(char), "char" },
                    { typeof(string), "string" }
                };

        private static readonly Dictionary<string, Type> typeOverrides = 
        new Dictionary<string, Type>()
            {
                { "http://schemas.microsoft.com/2003/10/Serialization/:guid", typeof(Guid)},
            };

        public static string GetAlias(Type clrType, Action<string> debug)
        {
            if (aliases.ContainsKey(clrType))
            {
                return aliases[clrType];
            }

            return clrType.FullName;
        }

        public static Type OverrideCLRType(Type type, string fullyQualifiedTypeName)
        {
            if (typeOverrides.ContainsKey(fullyQualifiedTypeName))
            {
                return typeOverrides[fullyQualifiedTypeName];
            }

            return type;
        }

        // TODO: Need to replace this, however .NET 3.5 hides it's own version so I'm stuck with this. Without using the XmlSerializer to do all of the generation.        
        public static Type GetCLRType(string xsdType, Action<string> debug)
        {
            // This is so hacky
            if (xsdType == "ser:guid")
                return typeof(Guid);

            // TODO: Is 0 a valid value for all primitive types? Not datetime
            try
            {
                var convertFrom = XsdConvert.ConvertFrom("0", xsdType);
                debug("ConvertedType: " + convertFrom.GetType());
                return convertFrom.GetType();
            }
            catch (InvalidOperationException e)
            {
                var convertFrom = XsdConvert.ConvertFrom("2013-01-01", xsdType);
                debug("ConvertedType: " + convertFrom.GetType());
                return convertFrom.GetType();
            }
        }

    }
}