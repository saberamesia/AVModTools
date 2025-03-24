using AVModTools.Extensions;
using Mono.Cecil;
using System.Linq;
using System.Reflection;

namespace AVModTools.GamePatcher
{
    internal class THDoor
    {
        public static ModuleDefinition PatchInOnDoorEnterHook(ModuleDefinition module)
        {
            var targetType = module.GetType($"OuterBeyond.THDoor");
            var methodToPatch = targetType.GetMethodsByName("EnterDoor")[0];
            var target = methodToPatch.Body.Instructions.First();

            module.QuickPatchBefore(target, targetType, methodToPatch, "OnDoorEnter", 0);

            return module;
        }
    }
}
