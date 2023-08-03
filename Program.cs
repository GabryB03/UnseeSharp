using AsmResolver.DotNet;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using System;
using System.IO;
using System.Text;

public class Program
{
    public static void Main()
    {
        try
        {
            Console.Title = "UnseeSharp";
            string path = "";

            while (!File.Exists(path) || !Path.GetExtension(path).ToLower().Equals(".exe"))
            {
                Logger.LogInfo("Please, insert the path of the executable file to obfuscate: ");
                path = Console.ReadLine();

                if (!File.Exists(path))
                {
                    Logger.LogError("This file does not exist. Please, try again.");
                }
                else if (!Path.GetExtension(path).ToLower().Equals(".exe"))
                {
                    Logger.LogError("This file has an invalid extension. Please, try again.");
                }
            }

            string midPath = path.Substring(0, path.Length - 4);
            string stringsPath = midPath + "-strings.exe";
            string outputPath = midPath + "-obfuscated.exe";

            ModuleDefMD module = ModuleDefMD.Load(path);
            ModuleWriterOptions options = new ModuleWriterOptions(module);
            options.Logger = DummyLogger.NoThrowInstance;
            options.MetadataOptions.Flags = MetadataFlags.KeepOldMaxStack | MetadataFlags.PreserveAll;
            options.Cor20HeaderOptions.Flags = dnlib.DotNet.MD.ComImageFlags.ILOnly;

            ImportProtection.Process(module);
            MoveEntryPoint.Process(module);
            AntiManipulation.Process(module);
            AntiDe4Dot.Process(module);
            NumberObfuscation.Process(module);
            ConstantsConfusion.Process(module);
            StringEncryption.Process(module);
            ConstantMelter.Process(module);
            SuperControlFlowObfuscation.Process(module);
            ControlFlowObfuscation.Process(module);
            AntiILDasm.Process(module);
            StackUnderflow.Process(module);
            LimitedCallProtection.Process(module);
            LimitedIntegerConfusion.Process(module);
            Renamer.Process(module);
            FakeAttributes.Process(module);
            OpCodesProtection.Process(module);

            module.Write(stringsPath, options);
            module.Dispose();

            ModuleDefinition newModule = ModuleDefinition.FromFile(stringsPath);
            HideStrings.Process(newModule);

            newModule.Write(outputPath);
            System.IO.File.Delete(stringsPath);
            byte[] theBytes = System.IO.File.ReadAllBytes(outputPath);
            System.IO.File.Delete(outputPath);
            System.IO.File.WriteAllBytes(outputPath, ReplaceBytes(theBytes, Encoding.ASCII.GetBytes("This program cannot be run in DOS mode."), new ProtoRandom.ProtoRandom(5).GetRandomByteArray(39)));
            byte[] MD5Bytes = System.Security.Cryptography.MD5.Create().ComputeHash(System.IO.File.ReadAllBytes(outputPath));

            using (FileStream stream = new FileStream(outputPath, FileMode.Append))
            {
                stream.Write(MD5Bytes, 0, MD5Bytes.Length);
            }

            Logger.LogSuccess("Succesfully obfuscated your file. Press ENTER to exit.");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to obfuscate your file:\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.Source);
            Console.ReadLine();
        }
    }

    private static byte[] ReplaceBytes(byte[] src, byte[] search, byte[] repl)
    {
        if (repl == null)
        {
            return src;
        }

        int index = FindBytes(src, search);

        if (index < 0)
        {
            return src;
        }

        byte[] dst = new byte[src.Length - search.Length + repl.Length];

        Buffer.BlockCopy(src, 0, dst, 0, index);
        Buffer.BlockCopy(repl, 0, dst, index, repl.Length);
        Buffer.BlockCopy(src, index + search.Length, dst, index + repl.Length, src.Length - (index + search.Length));

        return dst;
    }

    private static int FindBytes(byte[] src, byte[] find)
    {
        for (int i = 0; i < src.Length - find.Length + 1; i++)
        {
            if (src[i] == find[0])
            {
                for (int m = 1; m < find.Length; m++)
                {
                    if (src[i + m] != find[m])
                    {
                        break;
                    }

                    if (m == find.Length - 1)
                    {
                        return i;
                    }
                }
            }
        }

        return -1;
    }
}