using System;
using System.Linq;

namespace CSGeneration
{
    public static class NamespaceUtility
    {
        public static string NamespaceName(string namespaceUri)
        {
            var nsSegments = namespaceUri.Split('/');
            var namespaceName = String.IsNullOrEmpty(nsSegments.Last())
                                    ? nsSegments[nsSegments.Length - 1]
                                    : nsSegments.Last();
            return namespaceName;
        }
    }
}