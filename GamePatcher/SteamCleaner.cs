using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace AVModTools.GamePatcher
{
    internal class SteamCleaner
    {
        public static ModuleDefinition CleanProgramMain(ModuleDefinition module)
        {
            bool runGenerated = false;
            bool retFound = false;
            foreach (TypeDefinition type in module.Types)
            {
                if (type.Name == "Program")
                {
                    foreach (MethodDefinition method in type.Methods)
                    {
                        if (method.Name == "Main")
                        {
                            var il = method.Body.GetILProcessor();
                            var curr = method.Body.Instructions[0];
                            var next = curr.Next;

                            while (next != null)
                            {
                                var newCurr = next;

                                if (curr.OpCode == OpCodes.Ret)
                                {
                                    if (retFound)
                                    {
                                        curr = next;
                                        next = curr.Next;
                                        continue;
                                    }
                                    retFound = true;
                                }

                                if (curr.OpCode == OpCodes.Nop)
                                {
                                    curr = next;
                                    next = curr.Next;
                                    continue;
                                }

                                if (curr.OpCode == OpCodes.Call)
                                {
                                    if (curr.Operand.GetType() == typeof(MethodDefinition))
                                    {
                                        var operand = (MethodDefinition)curr.Operand;
                                        var name = operand.Name;
                                        Console.WriteLine($"operand name: {name}");
                                        if (operand.Name == "Initialize")
                                        {
                                            curr = curr.Next;
                                            next = curr.Next;
                                            continue;
                                        } else if (operand.Name == "RunGame" && !runGenerated)
                                        {
                                            runGenerated = true;
                                            curr = next;
                                            next = curr.Next;
                                            continue;
                                        }
                                    }
                                }

                                Console.WriteLine($"Removing operation {curr}");
                                il.Remove(curr);

                                curr = newCurr;
                                next = curr.Next;
                            }

                            method.Body.MaxStackSize = 2;
                            break;
                        }
                    }

                    break;
                }
            }

            return module;
        }
    }
}
