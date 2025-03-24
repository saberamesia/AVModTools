using AVModTools.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

namespace AVModTools.GamePatcher
{
    internal class THSpeedrunUI
    {
        public static void InsertDebugPrintBefore(ModuleDefinition module, ILProcessor ilProcessor, Instruction target, string message)
        {
            var printMethod = Util.DebugPrint(module);
            ilProcessor.InsertBefore(target, ilProcessor.Create(OpCodes.Ldc_I4, 500));
            ilProcessor.InsertBefore(target, ilProcessor.Create(OpCodes.Ldstr, message));
            ilProcessor.InsertBefore(target, ilProcessor.Create(OpCodes.Call, printMethod));
        }

        public static ModuleDefinition PatchSpeedrunUIHooks(ModuleDefinition module)
        {
            var speedrunUIType = module.GetType("OuterBeyond.THSpeedrunUI");
            MethodDefinition drawMethod = speedrunUIType.GetMethodsByName("Draw")[0];

            var ilProcessor = drawMethod.Body.GetILProcessor();

            foreach (Instruction instr in drawMethod.Body.Instructions)
            {
                if (instr.OpCode == OpCodes.Ldc_I4_S && (sbyte)instr.Operand == 24)
                {
                    ilProcessor.Replace(instr, ilProcessor.Create(OpCodes.Ldc_I4, 8766));
                    break;
                }
            }

            var mainFontCount = 0;
            Instruction markedInstruction = null;
            Instruction targetInstruction = null;
            foreach (Instruction instr in drawMethod.Body.Instructions)
            {
                if (instr.OpCode == OpCodes.Ldfld &&
                    ((FieldDefinition)instr.Operand).Name == "mMainFont")
                {
                    if (mainFontCount < 1)
                    {
                        mainFontCount++;
                        continue;
                    }
                    var prev = instr.Previous;
                    markedInstruction = prev;

                    FieldReference mInGame = new FieldReference("mInGame", module.TypeSystem.Boolean, speedrunUIType).Resolve();
                    ilProcessor.InsertBefore(prev, ilProcessor.Create(OpCodes.Ldarg_0));
                    targetInstruction = prev.Previous;
                    ilProcessor.InsertBefore(prev, ilProcessor.Create(OpCodes.Ldfld, mInGame));
                    ilProcessor.InsertBefore(prev, ilProcessor.Create(OpCodes.Brfalse_S, prev));

                    
                    var preDrawOpCodes = Util.GenerateHookOpCodes(module, "PreSpeedrunUIDraw", 1, ilProcessor);
                    foreach (Instruction hookOp in preDrawOpCodes)
                    {
                        ilProcessor.InsertBefore(prev, hookOp);
                    }

                    break;
                }
            }

            var foundBranchCount = 0;
            while (foundBranchCount < 2)
            {
                foreach (Instruction instr in drawMethod.Body.Instructions)
                {
                    if ((instr.OpCode == OpCodes.Blt_S || instr.OpCode == OpCodes.Brfalse_S) &&
                        (Instruction)instr.Operand == markedInstruction
                        )
                    {
                        foundBranchCount++;
                        ilProcessor.Replace(instr, ilProcessor.Create(instr.OpCode, targetInstruction));
                        break;
                    }
                }
            }

            return module;
        }
    }
}
