using Mono.Cecil;
using System.Collections.Generic;

namespace AVModTools.Extensions
{
    internal static class MethodExtensions
    {
        public static List<MethodDefinition> GetMethodsByName(this TypeDefinition typeDef, string methodName)
        {
            List<MethodDefinition> methodDefs = new List<MethodDefinition>();

            foreach (var method in typeDef.Methods)
            {
                if (method.Name == methodName)
                {
                    methodDefs.Add(method);
                }
            }

            return methodDefs;
        }

        public static List<MethodDefinition> GetConstructors(this TypeDefinition typeDef, string methodName)
        {
            List<MethodDefinition> methodDefs = new List<MethodDefinition>();

            foreach (var method in typeDef.Methods)
            {
                if (method.IsConstructor)
                {
                    methodDefs.Add(method);
                }
            }

            return methodDefs;
        }
    }
}
