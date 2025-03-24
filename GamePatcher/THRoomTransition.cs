using AVModTools.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;

namespace AVModTools.GamePatcher
{
    internal class THRoomTransition
    {
        public static ModuleDefinition PatchPostRoomTransitionHook(ModuleDefinition module)
        {
            var roomTransitionType = module.GetType("OuterBeyond.THRoomTransition");
            var updateMethod = roomTransitionType.GetMethodsByName("Update")[0];

            if (updateMethod == null)
            {
                Console.WriteLine("Unable to get THRoomTransition.Update method");
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
                        } else
                        {
                            var hookOpCodes = Util.GenerateHookOpCodes(module, "PostRoomTransition", 0, il);
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
