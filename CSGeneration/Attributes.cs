using System;

namespace CSGeneration
{
    public static class Attributes
    {
        public static readonly AttributeDescription GeneratedCodeAttribute =
            new AttributeDescription()
                {
                    Name =
                        "System.CodeDom.Compiler.GeneratedCodeAttribute",
                    Arguments = new[]
                                    {
                                        new ArgumentDescription()
                                            {
                                                Value = "\"System.ServiceModel\""
                                            },
                                        new ArgumentDescription()
                                            {Value = "\"4.0.0.0\""},
                                    }
                };

        public static AttributeDescription ServiceContractAttribute(string name)
        {
            return new AttributeDescription()
                       {
                           Name = "System.ServiceModel.ServiceContractAttribute",
                           Arguments = new[]
                                           {
                                               new ArgumentDescription()
                                                   {
                                                       Name = "ConfigurationName", 
                                                       Value = String.Format("\"{0}\"", name)
                                                   },
                                           }
                       };
        }

        public static AttributeDescription OperationContractAttribute(string replyAction, string action)
        {
            return new AttributeDescription()
                       {
                           Name = "System.ServiceModel.OperationContractAttribute",
                           Arguments = new[]
                                           {
                                               new ArgumentDescription()
                                                   {
                                                       Name = "Action",
                                                       Value = String.Format("\"{0}\"", action),
                                                   },
                                               new ArgumentDescription()
                                                   {
                                                       Name = "ReplyAction",
                                                       Value = String.Format("\"{0}\"", replyAction),
                                                   },
                                           }
                       };
        }
    }
}