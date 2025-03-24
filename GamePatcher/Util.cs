using AVModTools.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;

namespace AVModTools.GamePatcher
{
    internal class Util
    {
        public static OpCode[] argCodes =
        {
            OpCodes.Ldarg_1,
            OpCodes.Ldarg_2,
            OpCodes.Ldarg_3,
        };

        public static MethodDefinition DebugPrint(ModuleDefinition module)
        {
            TypeDefinition debugType = module.GetType("OuterBeyond.THDebug");
            var printDefs = debugType.GetMethodsByName("Print");
            foreach (MethodDefinition printDef in printDefs)
            {
                if (printDef.Parameters.Count == 2)
                {
                    return printDef;
                }
            }

            return null;
        }

        public static List<Instruction> GenerateHookOpCodes(ModuleDefinition module, string hookName, uint argCount, ILProcessor ilProcessor)
        {
            List<Instruction> instructions = new List<Instruction>();

            var gameType = module.GetType("OuterBeyond.THGame");
            var modLoaderType = module.GetType("OuterBeyond.Mod.ModLoader");

            FieldReference mGameField = (new FieldReference("mGame", gameType, gameType)).Resolve();
            FieldReference mModLoaderField = (new FieldReference("modLoader", modLoaderType, gameType)).Resolve();

            MethodDefinition hookDef = modLoaderType.GetMethodsByName(hookName)[0];

            if (hookDef == null)
            {
                Console.WriteLine($"Unable to find hook method {hookName} in mod loader class");
                return instructions;
            }

            instructions.Add(ilProcessor.Create(OpCodes.Ldsfld, mGameField));
            instructions.Add(ilProcessor.Create(OpCodes.Ldfld, mModLoaderField));
            for (int i = 0; i < argCodes.Length; i++)
            {
                if ( i == argCount ) { break; }
                instructions.Add(ilProcessor.Create(argCodes[i]));
            }
            instructions.Add(ilProcessor.Create(OpCodes.Callvirt, hookDef));

            return instructions;
        }
    }
}
