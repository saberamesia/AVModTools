using AVModTools.GamePatcher;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace AVModTools.Extensions
{
    internal static class ModuleExtension
    {
        public static ModuleDefinition QuickPatchBefore(
            this ModuleDefinition module, Instruction target, TypeDefinition targetType, MethodDefinition methodToPatch,
            string hookName, uint argCount) {

            var il = methodToPatch.Body.GetILProcessor();

            var hookOps = Util.GenerateHookOpCodes(module, hookName, argCount, il);

            foreach ( var hookOp in hookOps )
            {
                il.InsertBefore(target, hookOp);
            }

            return module;
        }
    }
}
