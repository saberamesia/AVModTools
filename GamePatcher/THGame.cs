using AVModTools.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace AVModTools.GamePatcher
{
    internal class THGame
    {
        public static TypeDefinition GetGameType(ModuleDefinition module)
        {
            return module.GetType("OuterBeyond.THGame");
        }

        static ModuleDefinition PatchInInitializeMods(ModuleDefinition module)
        {
            var gameType = GetGameType(module);
            var modLoaderType = module.GetType("OuterBeyond.Mod.ModLoader");
            MethodDefinition modLoaderCtor = null;
            MethodDefinition loadModsDef = null;
            var modLoaderField = new FieldDefinition("modLoader", FieldAttributes.Public, modLoaderType);
            gameType.Fields.Add(modLoaderField);

            foreach (MethodDefinition methodDef in modLoaderType.Methods)
            {
                if (methodDef.IsConstructor)
                {
                    modLoaderCtor = methodDef;
                }
                else if (methodDef.Name == "LoadMods")
                {
                    loadModsDef = methodDef;
                }
            }

            if (modLoaderCtor == null)
            {
                Console.WriteLine("Error finding ModLoader constructor");
                return module;
            }

            var method = new MethodDefinition("InitializeMods",
                MethodAttributes.Public | MethodAttributes.HideBySig, module.TypeSystem.Void);

            var il = method.Body.GetILProcessor();

            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Newobj, modLoaderCtor));
            il.Append(il.Create(OpCodes.Stfld, modLoaderField));
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldfld, modLoaderField));
            il.Append(il.Create(OpCodes.Callvirt, loadModsDef));
            il.Append(il.Create(OpCodes.Ret));

            gameType.Methods.Add(method);

            return module;
        }

        public static ModuleDefinition PatchInModInit(ModuleDefinition module)
        {
            var gameType = GetGameType(module);
            module = PatchInInitializeMods(module);
            module = Program.PatchInitializeCallIntoRunGame(module, gameType);

            return module;
        }

        public static ModuleDefinition PatchInPreGameUpdateHook(ModuleDefinition module)
        {
            var gameType = GetGameType(module);
            var updateMethod = gameType.GetMethodsByName("Update")[0];
            var target = updateMethod.Body.Instructions.First();

            module.QuickPatchBefore(target, gameType, updateMethod, "PreGameUpdate", 0);

            return module;
        }
    }
}
