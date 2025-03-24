
using AVModTools.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Linq;

namespace AVModTools.GamePatcher
{
    internal class THBoss
    {
        public static ModuleDefinition PatchInBossDeathHooks(ModuleDefinition module)
        {
            var typeDef = module.GetType("OuterBeyond.THBoss");
            var deathStartMethod = typeDef.GetMethodsByName("BeginDeath")[0];
            var dieMethod = typeDef.GetMethodsByName("Die")[0];

            var deathStartIl = deathStartMethod.Body.GetILProcessor();
            var deathStartHookOps = Util.GenerateHookOpCodes(module, "OnBossDeathStart", 0, deathStartIl);

            var first = deathStartMethod.Body.Instructions.First();

            foreach (Instruction instr in deathStartHookOps)
            {
                deathStartIl.InsertBefore(first, instr);
            }

            var dieIl = dieMethod.Body.GetILProcessor();
            var dieHookOps = Util.GenerateHookOpCodes(module, "OnBossDeath", 0, dieIl);

            first = dieMethod.Body.Instructions.First();

            foreach (Instruction instr in dieHookOps)
            {
                dieIl.InsertBefore(first, instr);
            }

            return module;
        }

        public static ModuleDefinition PatchInBossDamageHook(ModuleDefinition module)
        {
            var gameType = module.GetType("OuterBeyond.THGame");
            var typeDef = module.GetType("OuterBeyond.THBoss");
            var retType = module.GetType("OuterBeyond.THDamageResult");
            var weaponType = module.GetType("OuterBeyond.THWeapon");
            var characterType = module.GetType("OuterBeyond.THCharacter");
            var collisionResultRefType = module.GetType("OuterBeyond.THCollisionResults").MakeByReferenceType();
            var modLoaderType = module.GetType("OuterBeyond.Mod.ModLoader");
            var creatureType = module.GetType("OuterBeyond.THCreature");

            MethodDefinition hookMethod = null;
            MethodDefinition baseMethod = null;

            foreach (MethodDefinition methodDef in modLoaderType.GetMethods())
            {
                if (methodDef.Name == "OnDamageBoss")
                {
                    hookMethod = methodDef;
                }
            }

            if (hookMethod == null)
            {
                Console.WriteLine("Unable to find boss damage hook in mod loader class");
                return module;
            }

            foreach (MethodDefinition methodDef in creatureType.GetMethods())
            {
                if (methodDef.Name == "TakeDamage")
                {
                    baseMethod = methodDef;
                }
            }

            if (baseMethod == null)
            {
                Console.WriteLine("Unable to find base TakeDamage method in THCreature class");
                return module;
            }

            var method = new MethodDefinition("TakeDamage",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual, retType);

            ParameterDefinition[] paramDefs = {
                new ParameterDefinition("weapon", ParameterAttributes.None, weaponType),
                new ParameterDefinition("attacker", ParameterAttributes.None, characterType),
                new ParameterDefinition("collisionResults", ParameterAttributes.None, collisionResultRefType),
            };

            foreach (ParameterDefinition paramDef in paramDefs)
            {
                method.Parameters.Add(paramDef);
            }

            var ilProcessor = method.Body.GetILProcessor();

            var hookOpCodes = Util.GenerateHookOpCodes(module, "OnDamageBoss", 3, ilProcessor);

            // Crafting a method from scratch for an existing class using Cecil is a nightmare (:
            foreach (Instruction instr in hookOpCodes)
            {
                ilProcessor.Append(instr);
            }

            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0));
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_1));
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_2));
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_3));
            ilProcessor.Append(ilProcessor.Create(OpCodes.Call, baseMethod));
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));

            typeDef.Methods.Add(method);

            return module;
        }

    }
}
