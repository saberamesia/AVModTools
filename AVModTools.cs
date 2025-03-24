using System;
using AVModTools.GamePatcher;
using Mono.Cecil;

namespace AVModTools
{
    internal class ModTools
    {
        public static async void InstallModLoader(string path)
        {
            try
            {
                System.IO.Directory.CreateDirectory($"{path}\\Backup");
                try
                {
                    System.IO.File.Copy($"{path}\\AxiomVerge.exe", $"{path}\\Backup\\AxiomVerge.exe");
                } catch (System.IO.IOException)
                {

                }

                ModLoaderPatcher.PatchInModLoaderDLL(path);
                var module = ModuleDefinition.ReadModule($"{path}\\AxiomVerge.exe", new ReaderParameters { ReadWrite = true });
                module = THGame.PatchInModInit(module);
                module.Write();
                module.Dispose();

                module = ModuleDefinition.ReadModule($"{path}\\AxiomVerge.exe", new ReaderParameters { ReadWrite = true });
                module.Characteristics
                module = THGame.PatchInPreGameUpdateHook(module);
                module = THBoss.PatchInBossDamageHook(module);
                module = THBoss.PatchInBossDeathHooks(module);
                module = THDoor.PatchInOnDoorEnterHook(module);
                module = THRoomTransition.PatchPostRoomTransitionHook(module);
                module = THAreaTransition.PatchPostAreaTransitionHook(module);
                module = THSpeedrunUI.PatchSpeedrunUIHooks(module);
                module = THHud.PatchInPreDrawHook(module);
                module = THFont.PatchDrawTimeColor(module);
                module = BaseGameModifier.PublicizeGameBinary(module);
                module.Write();
                module.Dispose();
            }
            catch (System.IO.FileNotFoundException e)
            {
                Console.WriteLine($"Unable to find base game file ${e.FileName}, aborting");
            }
        }
    }
}