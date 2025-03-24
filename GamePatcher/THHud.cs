using AVModTools.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

namespace AVModTools.GamePatcher
{
    internal class THHud
    {
        public static ModuleDefinition PatchInPreDrawHook(ModuleDefinition module)
        {
            var hudType = module.GetType("OuterBeyond.THHUD");
            var drawMethod = hudType.GetMethodsByName("Draw")[0];

            var il = drawMethod.Body.GetILProcessor();
            var curr = drawMethod.Body.Instructions.First();

            while (curr.OpCode != OpCodes.Ret)
            {
                curr = curr.Next;
            }

            var brToFix = curr.Previous;
            var target = curr.Next;

            var hookOps = Util.GenerateHookOpCodes(module, "PreHudDraw", 1, il);
            foreach ( var hookOp in hookOps )
            {
                il.InsertBefore(target, hookOp);
            }

            il.Replace(brToFix, il.Create(brToFix.OpCode, curr.Next));

            return module;
        }
    }
}
