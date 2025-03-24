using Mono.Cecil;
using System;

using Mono.Cecil.Cil;

namespace AVModTools.GamePatcher
{
    internal static class BaseGameModifier
    {
        internal static ModuleDefinition module;

        internal static void publicizeMethods(ModuleDefinition module, TypeDefinition type)
        {
            foreach(MethodDefinition method in type.Methods)
            {
                //Console.WriteLine($"{method.DeclaringType.Name} {method.Name}");
                if (method.DeclaringType.Name == "THXMLDataLoader" && method.Name == "Deserialize")
                {
                    while (fixGameTypeReference(module, method)) { };
                }

                if (!method.IsPublic && !method.IsPrivate)
                {
                    method.IsPublic = true;
                    Console.WriteLine($"making method ${type.Name}.${method.Name} public");
                }
            }
        }

        public static ModuleDefinition PublicizeGameBinary(ModuleDefinition module)
        {
            foreach (TypeDefinition typeDefinition in module.Types)
            {
                Console.WriteLine($"type: {typeDefinition.ToString()}");
                if (typeDefinition.IsNotPublic)
                {
                    typeDefinition.IsPublic = true;
                    typeDefinition.IsNotPublic = false;
                    Console.WriteLine($"Making type {typeDefinition.Name} public");
                }
                publicizeMethods(module,typeDefinition);
            }

            return module;
        }

        public static bool fixGameTypeReference(ModuleDefinition module, MethodDefinition method)
        {
            Console.WriteLine($"attempting to fix type references for method {method.Name}");
            var il = method.Body.GetILProcessor();

            foreach (Instruction i in method.Body.Instructions)
            {
                if (i.OpCode == OpCodes.Ldsfld && i.Operand.ToString().Contains("mGame")) {
                    TypeDefinition thGameType = module.GetType("OuterBeyond.THGame");

                    il.Replace(i.Next, il.Create(OpCodes.Call, module.ImportReference(typeof(Type).GetMethod("GetTypeFromHandle"))));
                    il.Replace(i, il.Create(OpCodes.Ldtoken, thGameType));

                    return true;
                }
            }

            return false;
        }

        internal static ModuleDefinition repairGameReference(ModuleDefinition module)
        {
            module = PublicizeGameBinary(module);
            return module;
        }
    }
}
