using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;

public class ControlFlowObfuscation
{
    private static ProtoRandom.ProtoRandom _random = new ProtoRandom.ProtoRandom(5);

    public static void Process(ModuleDefMD module)
    {
        foreach (TypeDef type in module.Types)
        {
            if (type.IsGlobalModuleType)
            {
                continue;
            }

            foreach (MethodDef meth in type.Methods)
            {
                if (meth.Name.StartsWith("get_") || meth.Name.StartsWith("set_"))
                {
                    continue;
                }

                if (!meth.HasBody || meth.IsConstructor)
                {
                    continue;
                }

                meth.Body.SimplifyBranches();
                ExecuteMethod(meth);
            }
        }

        foreach (TypeDef type in module.Types)
        {
            foreach (MethodDef meth in type.Methods.ToArray())
            {
                if (!meth.HasBody || !meth.Body.HasInstructions || meth.Body.HasExceptionHandlers)
                {
                    continue;
                }

                for (int i = 0; i < meth.Body.Instructions.Count - 2; i++)
                {
                    Instruction inst = meth.Body.Instructions[i + 1];

                    if (inst.OpCode.Equals(OpCodes.Call))
                    {
                        string str = inst.Operand.ToString();
                        
                        if (str == "System.Void Microsoft.VisualBasic.VBMath::Randomize()")
                        {
                            meth.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Ldstr, "a"));
                            meth.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Br_S, inst));
                            i += 2;
                        }
                        else
                        {
                            i++;
                        }
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }
    }

    private static void ExecuteMethod(MethodDef meth)
    {
        meth.Body.SimplifyMacros(meth.Parameters);
        List<Block> blocks = BlockParser.ParseMethod(meth);
        blocks = Randomize(blocks);
        meth.Body.Instructions.Clear();
        Local local = new Local(meth.Module.CorLibTypes.Int32);
        meth.Body.Variables.Add(local);
        Instruction target = Instruction.Create(OpCodes.Nop);
        Instruction instr = Instruction.Create(OpCodes.Br, target);

        foreach (Instruction instruction in Calc(0))
        {
            meth.Body.Instructions.Add(instruction);
        }

        meth.Body.Instructions.Add(Instruction.Create(OpCodes.Stloc, local));
        meth.Body.Instructions.Add(Instruction.Create(OpCodes.Br, instr));
        meth.Body.Instructions.Add(target);

        foreach (Block block in blocks.Where(block => block != blocks.Single(x => x.Number == blocks.Count - 1)))
        {
            meth.Body.Instructions.Add(Instruction.Create(OpCodes.Call, meth.Module.Import(typeof(Microsoft.VisualBasic.VBMath).GetMethod("Randomize", new Type[] { }))));
            meth.Body.Instructions.Add(Instruction.Create(OpCodes.Ldloc, local));

            foreach (Instruction instruction in Calc(block.Number))
            {
                meth.Body.Instructions.Add(instruction);
            }

            meth.Body.Instructions.Add(Instruction.Create(OpCodes.Ceq));
            Instruction instruction4 = Instruction.Create(OpCodes.Nop);
            meth.Body.Instructions.Add(Instruction.Create(OpCodes.Brfalse, instruction4));

            foreach (Instruction instruction in block.Instructions)
            {
                meth.Body.Instructions.Add(instruction);
            }

            foreach (Instruction instruction in Calc(block.Number + 1))
            {
                meth.Body.Instructions.Add(instruction);
            }

            meth.Body.Instructions.Add(Instruction.Create(OpCodes.Stloc, local));
            meth.Body.Instructions.Add(instruction4);
        }

        meth.Body.Instructions.Add(Instruction.Create(OpCodes.Ldloc, local));

        foreach (Instruction instruction in Calc(blocks.Count - 1))
        {
            meth.Body.Instructions.Add(instruction);
        }

        meth.Body.Instructions.Add(Instruction.Create(OpCodes.Ceq));
        meth.Body.Instructions.Add(Instruction.Create(OpCodes.Brfalse, instr));
        meth.Body.Instructions.Add(Instruction.Create(OpCodes.Br, blocks.Single(x => x.Number == blocks.Count - 1).Instructions[0]));
        meth.Body.Instructions.Add(instr);

        foreach (Instruction lastBlock in blocks.Single(x => x.Number == blocks.Count - 1).Instructions)
        {
            meth.Body.Instructions.Add(lastBlock);
        }
    }

    private static List<Block> Randomize(List<Block> input)
    {
        List<Block> ret = new List<Block>();

        foreach (var group in input)
        {
            ret.Insert(_random.GetRandomInt32(0, ret.Count), group);
        }

        return ret;
    }

    private static List<Instruction> Calc(int value)
    {
        return new List<Instruction> { Instruction.Create(OpCodes.Ldc_I4, value) };
    }

    public void AddJump(IList<Instruction> instrs, Instruction target)
    {
        instrs.Add(Instruction.Create(OpCodes.Br, target));
    }
}