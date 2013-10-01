using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CSGeneration
{
    public static class DebugUtility
    {
        public static string GetProperties(object obj)
        {            
            PropertyInfo[] _PropertyInfos = obj.GetType().GetProperties();

            var sb = new StringBuilder();

            foreach (var info in _PropertyInfos)
            {
                var value = info.GetValue(obj, null);

                var output = value == null ? info.Name : info.Name + ": " + value.ToString();
                sb.AppendLine(output);
            }

            return sb.ToString();
        }
    }
}
