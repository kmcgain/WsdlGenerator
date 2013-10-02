using System.Collections.Generic;
using System.Linq;

namespace CSGeneration
{
    public class AttributeDescription
    {
        public virtual string Name { get; set; }

        public virtual IList<ArgumentDescription> Arguments { get; set; }

        public virtual string FormattedArgs()
        {
            if (Arguments == null || !Arguments.Any())
            {
                return string.Empty;
            }

            return string.Join(", ", Arguments.Select(_ => _.ToString()).ToArray());
        }
    }
}