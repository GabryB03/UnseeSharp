using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Collections.Generic;
using System.Linq;

public class AntiManipulation
{
    public static void Process(ModuleDefMD module)
    {
        ModuleDefMD typeModule = ModuleDefMD.Load(typeof(AntiManipulationRuntime).Module);
        MethodDef cctor = module.EntryPoint.DeclaringType.FindOrCreateStaticConstructor();
        TypeDef typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(AntiManipulationRuntime).MetadataToken));
        IEnumerable<IDnlibDef> members = InjectHelper.Inject(typeDef, module.EntryPoint.DeclaringType, module);
        var init = (MethodDef)members.Single(method => method.Name == "RunAll");
        module.EntryPoint.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, init));

        foreach (TypeDef type in module.Types)
        {
            if (type.IsGlobalModuleType)
            {
                continue;
            }

            foreach (MethodDef method in type.Methods)
            {
                if (!method.HasBody)
                {
                    continue;
                }

                if (method.IsConstructor)
                {
                    method.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Nop));
                    method.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, init));
                }
            }
        }

        foreach (MethodDef md in module.GlobalType.Methods)
        {
            if (md.Name != ".ctor")
            {
                continue;
            }

            module.GlobalType.Remove(md);
            break;
        }
    }
}