using ILRepacking;
using System;

namespace AVModTools.GamePatcher
{
    internal class ModLoaderPatcher
    {
        public static void PatchInModLoaderDLL(string path)
        {
            RepackOptions options = new RepackOptions();
            options.InputAssemblies = new string[] {
                $"{path}\\Backup\\AxiomVerge.exe",
                $"{path}\\AVModLoader.dll",
            };
            foreach (string assembly in options.InputAssemblies)
            {
                Console.WriteLine(assembly);
            }
            options.OutputFile = $"{path}\\AxiomVerge.exe";
            options.PauseBeforeExit = false;

            ILRepack repack = new ILRepack(options);
            repack.Repack();
        }
    }
}
