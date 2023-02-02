/// Source & Credits to: https://github.com/Sato-Isolated/MindLated/blob/master/Protection/LocalF/L2F.cs

using dnlib.DotNet.Emit;
using dnlib.DotNet;
using System.Collections.Generic;
using System.Linq;

public class LocalsToFields
{
    private static Dictionary<Local, FieldDef> _convertedLocals = new Dictionary<Local, FieldDef>();

    public static void Process(ModuleDef module)
    {
        foreach (TypeDef type in module.Types.Where(x => x != module.GlobalType))
        {
            foreach (MethodDef meth in type.Methods.Where(x => x.HasBody && x.Body.HasInstructions && !x.IsConstructor))
            {
                _convertedLocals = new Dictionary<Local, FieldDef>();
                ConvertAll(module, meth);
            }
        }
    }

    private static void ConvertAll(ModuleDef module, MethodDef meth)
    {
        foreach (Instruction instruction in meth.Body.Instructions)
        {
            if (instruction.Operand == null)
            {
                continue;
            }

            if (instruction.Operand.GetType() != typeof(Local))
            {
                continue;
            }

            Local local = (Local)instruction.Operand;
            FieldDef def;

            if (!_convertedLocals.ContainsKey(local))
            {
                def = new FieldDefUser(NameGenerator.GenerateName(), new FieldSig(local.Type), FieldAttributes.Public | FieldAttributes.Static);
                module.GlobalType.Fields.Add(def);
                _convertedLocals.Add(local, def);
            }
            else
            {
                def = _convertedLocals[local];
            }

            OpCode eq = null;

            switch (instruction.OpCode.Code)
            {
                case Code.Ldloc:
                    eq = OpCodes.Ldsfld;
                    break;
                case Code.Ldloc_S:
                    eq = OpCodes.Ldsfld;
                    break;
                case Code.Ldloc_0:
                    eq = OpCodes.Ldsfld;
                    break;
                case Code.Ldloc_1:
                    eq = OpCodes.Ldsfld;
                    break;
                case Code.Ldloc_2:
                    eq = OpCodes.Ldsfld;
                    break;
                case Code.Ldloc_3:
                    eq = OpCodes.Ldsfld;
                    break;
                case Code.Ldloca:
                    eq = OpCodes.Ldsflda;
                    break;
                case Code.Ldloca_S:
                    eq = OpCodes.Ldsflda;
                    break;
                case Code.Stloc:
                    eq = OpCodes.Stsfld;
                    break;
                case Code.Stloc_0:
                    eq = OpCodes.Stsfld;
                    break;
                case Code.Stloc_1:
                    eq = OpCodes.Stsfld;
                    break;
                case Code.Stloc_2:
                    eq = OpCodes.Stsfld;
                    break;
                case Code.Stloc_3:
                    eq = OpCodes.Stsfld;
                    break;
                case Code.Stloc_S:
                    eq = OpCodes.Stsfld;
                    break;
            }

            instruction.OpCode = eq;
            instruction.Operand = def;
        }
    }
}