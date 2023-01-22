using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;

public class AntiDe4Dot
{
    public static void Process(ModuleDefMD module)
    {
        TypeDef typedef = new TypeDefUser("", "YouAreSkid", module.CorLibTypes.GetTypeRef("System", "Attribute"));
        module.Types.Add(typedef);
        typedef.Interfaces.Add(new InterfaceImplUser(typedef));
        typedef.Interfaces.Add(new InterfaceImplUser(module.GlobalType));

        Instruction jumpTo = module.EntryPoint.Body.Instructions[0];

        module.EntryPoint.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, module.Import(typeof(System.Reflection.Assembly).GetMethod("GetExecutingAssembly", new Type[] { }))));
        module.EntryPoint.Body.Instructions.Insert(1, Instruction.Create(OpCodes.Call, module.Import(typeof(System.Reflection.Assembly).GetMethod("GetCallingAssembly", new Type[] { }))));
        module.EntryPoint.Body.Instructions.Insert(2, Instruction.Create(OpCodes.Call, module.Import(typeof(System.Reflection.Assembly).GetMethod("op_Inequality", new Type[] { typeof(System.Reflection.Assembly), typeof(System.Reflection.Assembly) }))));
        module.EntryPoint.Body.Instructions.Insert(3, new Instruction(OpCodes.Brfalse_S, jumpTo));
        module.EntryPoint.Body.Instructions.Insert(4, Instruction.Create(OpCodes.Call, module.Import(typeof(System.Diagnostics.Process).GetMethod("GetCurrentProcess", new Type[] { }))));
        module.EntryPoint.Body.Instructions.Insert(5, Instruction.Create(OpCodes.Callvirt, module.Import(typeof(System.Diagnostics.Process).GetMethod("Kill", new Type[] { }))));
        module.EntryPoint.Body.Instructions.Insert(6, Instruction.Create(OpCodes.Ret));
    }
}