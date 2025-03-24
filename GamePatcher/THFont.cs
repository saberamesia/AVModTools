using AVModTools.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

namespace AVModTools.GamePatcher
{
    internal class THFont
    {
        public static ModuleDefinition PatchDrawTimeColor(ModuleDefinition module)
        {
            var fontType = module.GetType("OuterBeyond.THFont");
            var drawMethod = fontType.GetMethodsByName("DrawTime")[0];

            var il = drawMethod.Body.GetILProcessor();

            var curr = drawMethod.Body.Instructions.First(); 
            while (curr != null)
            {
                var next = curr.Next;
                if (curr.OpCode == OpCodes.Call && ((MethodReference)curr.Operand).Name == "get_White")
                {
                    il.Replace(curr, il.Create(OpCodes.Ldarg_S, (byte)4));
                }

                curr = next;
            }

            return module;
        }
    }
}
