using Mono.Cecil.Cil;
using Mono.Cecil;
using System;
using AVModTools.Extensions;

namespace AVModTools.GamePatcher
{
    internal class THAreaTransition
    {
        public static ModuleDefinition PatchPostAreaTransitionHook(ModuleDefinition module)
        {
            var roomTransitionType = module.GetType("OuterBeyond.THAreaTransition");
            var updateMethod = roomTransitionType.GetMethodsByName("Update")[0];

            if (updateMethod == null)
            {
                Console.WriteLine("Unable to get THAreaTransition.Update method");
                return module;
            }

            var il = updateMethod.Body.GetILProcessor();
            bool foundPause = false;

            foreach (Instruction instruction in updateMethod.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Callvirt && (
                    (MethodDefinition)instruction.Operand).Name == "SetPaused")
                {
                    var prev = instruction.Previous;
                    if (prev.OpCode == OpCodes.Ldc_I4_0)
                    {
                        if (!foundPause)
                        {
                            foundPause = true;
                        }
                        else
                        {
                            var hookOpCodes = Util.GenerateHookOpCodes(module, "PostAreaTransition", 0, il);
                            var next = instruction.Next;
                            foreach (Instruction hookInstruction in hookOpCodes)
                            {
                                il.InsertBefore(next, hookInstruction);
                            }
                            break;
                        }
                    }
                }
            }

            return module;
        }
    }
}
