using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace AVModTools.GamePatcher
{
    internal class Program
    {
        public static ModuleDefinition PatchInitializeCallIntoRunGame(ModuleDefinition module, TypeDefinition gameType)
        {
            TypeDefinition programType = module.GetType("OuterBeyond.Program");
            MethodDefinition initializeMods = null;
            MethodDefinition runGameDef = null;
            foreach (MethodDefinition methodDef in gameType.Methods)
            {
                if (methodDef.Name == "InitializeMods")
                {
                    initializeMods = methodDef;
                    break;
                }
            }

            foreach (MethodDefinition methodDef in programType.Methods)
            {
                if (methodDef.Name == "RunGame")
                {
                    runGameDef = methodDef;
                    break;
                }
            }

            if (initializeMods == null || runGameDef == null)
            {
                Console.WriteLine("Unable to find at least one of InitializeMods or THGame constructor");
                return module;
            }

            var il = runGameDef.Body.GetILProcessor();

            var insertInstruction = runGameDef.Body.Instructions.First();
            while (insertInstruction.OpCode != OpCodes.Callvirt) { insertInstruction = insertInstruction.Next; }
            il.InsertBefore(insertInstruction, il.Create(OpCodes.Callvirt, initializeMods));
            il.InsertBefore(insertInstruction, il.Create(OpCodes.Ldloc_0));

            return module;
        }

    }
}
