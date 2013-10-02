using System;
using System.Collections.Generic;

namespace CSGeneration
{
    public class TemplateOperations
    {
        private string debugOutput = "/*\n";

        public Action<string> Debug;
        
        public TemplateOperations()
        {
            Debug = (output) =>
                        {
                            debugOutput += output + "\n";
                        };

        }

        public void DebugFlush()
        {
            WriteLine(debugOutput + "\n*/");
            debugOutput = "/*";
        }

        public Action<string> StartFile { get; set; }

        public Action EndFile { get; set; }

        public delegate CSTemplate ClassSettings(
            string className, IList<MemberDescription> members, IList<string> baseTypes, IList<AttributeDescription> attributes, bool isInterface);

        public ClassSettings ClassGenerator { get; set; }

        public Func<string, string, CSTemplate> NamespaceGenerator { get; set; }

        public Action<string> Write { get; set; }

        public void WriteLine(string content)
        {
            Write(content + "\n");
        }

        public void CreateFile(string name, string content)
        {
            StartFile(name + ".generated.cs");
            Write(content);
            EndFile();
        }
    }
}