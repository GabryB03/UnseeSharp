using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Collections.Generic;
using System;
using System.Text;

public static class StringEncryption
{
    private static string theKey = "";
    private static ProtoRandom.ProtoRandom _random = new ProtoRandom.ProtoRandom(5);
    private static char[] _characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

    public static void Process(ModuleDefMD module)
    {
        theKey = _random.GetRandomString(_characters, _random.GetRandomInt32(8, 16));
        MethodDef DecryptMethod = Inject(module);

        foreach (TypeDef type in module.Types)
        {
            foreach (MethodDef method in type.Methods)
            {
                if (method.Body == null)
                {
                    continue;
                }

                method.Body.SimplifyBranches();

                for (int i = 0; i < method.Body.Instructions.Count; i++)
                {
                    if (method.Body.Instructions[i].OpCode == OpCodes.Ldstr)
                    {
                        method.Body.Instructions[i].OpCode = OpCodes.Nop;
                        method.Body.Instructions.Insert(i + 1, new Instruction(OpCodes.Call, module.Import(typeof(System.Text.Encoding).GetMethod("get_Unicode", new Type[] { }))));
                        method.Body.Instructions.Insert(i + 2, new Instruction(OpCodes.Ldstr, Convert.ToBase64String(Encoding.Unicode.GetBytes(method.Body.Instructions[i].Operand.ToString()))));
                        method.Body.Instructions.Insert(i + 3, new Instruction(OpCodes.Call, module.Import(typeof(System.Convert).GetMethod("FromBase64String", new Type[] { typeof(string) }))));
                        method.Body.Instructions.Insert(i + 4, new Instruction(OpCodes.Callvirt, module.Import(typeof(System.Text.Encoding).GetMethod("GetString", new Type[] { typeof(byte[]) }))));
                        i += 4;
                    }
                }
            }
        }

        foreach (TypeDef type in module.Types)
        {
            foreach (MethodDef method in type.Methods)
            {
                if (method == DecryptMethod)
                {
                    continue;
                }

                if (!method.HasBody)
                {
                    continue;
                }

                for (int i = 0; i < method.Body.Instructions.Count(); i++)
                {
                    if (method.Body.Instructions[i].OpCode == OpCodes.Ldstr)
                    {
                        string oldStr = method.Body.Instructions[i].Operand.ToString();

                        method.Body.Instructions[i].Operand = Encrypt(oldStr);
                        method.Body.Instructions.Insert(i + 1, new Instruction(OpCodes.Call, DecryptMethod));

                        i++;
                    }
                }

                method.Body.SimplifyBranches();
                method.Body.OptimizeBranches();
            }
        }

        /*Instruction jumpTo = DecryptMethod.Body.Instructions[0];

        DecryptMethod.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, module.Import(typeof(System.Reflection.Assembly).GetMethod("GetExecutingAssembly", new Type[] { }))));
        DecryptMethod.Body.Instructions.Insert(1, Instruction.Create(OpCodes.Call, module.Import(typeof(System.Reflection.Assembly).GetMethod("GetCallingAssembly", new Type[] { }))));
        DecryptMethod.Body.Instructions.Insert(2, Instruction.Create(OpCodes.Call, module.Import(typeof(System.Reflection.Assembly).GetMethod("op_Inequality", new Type[] { typeof(System.Reflection.Assembly), typeof(System.Reflection.Assembly) }))));
        DecryptMethod.Body.Instructions.Insert(3, new Instruction(OpCodes.Brfalse_S, jumpTo));
        DecryptMethod.Body.Instructions.Insert(4, Instruction.Create(OpCodes.Call, module.Import(typeof(System.Diagnostics.Process).GetMethod("GetCurrentProcess", new Type[] { }))));
        DecryptMethod.Body.Instructions.Insert(5, Instruction.Create(OpCodes.Callvirt, module.Import(typeof(System.Diagnostics.Process).GetMethod("Kill", new Type[] { }))));
        DecryptMethod.Body.Instructions.Insert(6, Instruction.Create(OpCodes.Ldstr, ""));
        DecryptMethod.Body.Instructions.Insert(7, Instruction.Create(OpCodes.Ret));*/
    }

    private static string Encrypt(string input)
    {
        System.Security.Cryptography.RijndaelManaged AES = new System.Security.Cryptography.RijndaelManaged();
        byte[] hash = new byte[32];
        byte[] temp = new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(Encoding.Unicode.GetBytes(theKey));
        Array.Copy(temp, 0, hash, 0, 16);
        Array.Copy(temp, 0, hash, 15, 16);
        AES.Key = hash;
        AES.Mode = System.Security.Cryptography.CipherMode.ECB;
        byte[] buffer = Encoding.Unicode.GetBytes(input);
        return Convert.ToBase64String(AES.CreateEncryptor().TransformFinalBlock(buffer, 0, buffer.Length));
    }

    private static MethodDef Inject(ModuleDef asmDef)
    {
        TypeDef typeDef = ModuleDefMD.Load(typeof(StringDecryptor).Module).ResolveTypeDef(MDToken.ToRID(typeof(StringDecryptor).MetadataToken));
        TypeDef theClass = new TypeDefUser("StringEncryptionNoSkid", asmDef.CorLibTypes.Object.TypeDefOrRef);
        theClass.Attributes = TypeAttributes.Public | TypeAttributes.AutoLayout | TypeAttributes.Class | TypeAttributes.AnsiClass;
        asmDef.Types.Add(theClass);
        IEnumerable<IDnlibDef> members = InjectHelper.Inject(typeDef, theClass, asmDef);
        MethodDef initMethod = (MethodDef)members.Single(methodddd => methodddd.Name == "Decrypt");

        foreach (Instruction instr in initMethod.Body.Instructions)
        {
            if (instr.OpCode.Equals(OpCodes.Ldstr))
            {
                if ((string)instr.Operand == "k")
                {
                    instr.Operand = theKey;
                }
            }
        }

        initMethod.Name = "UNSEESHARP_OBFUSCATOR_STRING_ENCRYPTION_KEY_OBFUSCATION";
        return initMethod;
    }
}