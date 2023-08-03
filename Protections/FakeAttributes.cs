using dnlib.DotNet;
using dnlib.DotNet.Emit;

public class FakeAttributes
{
    private static string[] _attributes = new string[]
    {
        "YanoAttribute",
        "Xenocode.Client.Attributes.AssemblyAttributes.ProcessedByXenocode",
        "PoweredByAttribute",
        "ObfuscatedByGoliath",
        "NineRays.Obfuscator.Evaluation",
        "NetGuard",
        "dotNetProtector",
        "DotNetPatcherPackerAttribute",
        "DotNetPatcherObfuscatorAttribute",
        "DotfuscatorAttribute",
        "CryptoObfuscator.ProtectedWithCryptoObfuscatorAttribute",
        "ProtectedWithCryptoObfuscatorAttribute",
        "ProcessedByXenocode",
        "BabelObfuscatorAttribute",
        "BabelAttribute",
        "AssemblyInfoAttribute",
        "ConfusedByAttribute",
        "();\t",
        "EMyPID_8234_",
        "ZYXDNGuarder",
        "SecureTeam.Attributes.ObfuscatedByAgileDotNetAttribute",
        "SmartAssembly.Attributes.PoweredByAttribute",
        "Protected",
        "<ObfuscatedByDotNetPatcher>",
        "OrangeHeapAttribute",
        ".NETGuard",
        "NineRays.Obfuscator.SoftwareWatermarkAttribute",
        "SecureTeam.Attributes.ObfuscatedByCliSecureAttribute",
        "Reactor",
        "CryptoObfuscator",
        "WTF",
        "OiCuntJollyGoodDayYeHavin_____________________________________________________",
        "OiCuntJollyGoodDayYeHavin",
        "Borland.Vcl.Types",
        "Borland.Eco.Interfaces",
        "Oliviay",
        "____KILL",
        "CheckRuntime",
        "Sixxpack",
        "CodeWallTrialVersion",
        "Protected_By_Attribute'00'NETSpider.Attribute"
    };

    public static void Process(ModuleDefMD module)
    {
        foreach (string attribute in _attributes)
        {
            TypeRef typeRef = module.Assembly.ManifestModule.CorLibTypes.GetTypeRef("System", "Attribute");
            TypeDefUser typeDefUser = new TypeDefUser("", attribute, typeRef);
            module.Assembly.ManifestModule.Types.Add(typeDefUser);
            MethodDefUser methodDefUser = new MethodDefUser(".ctor", MethodSig.CreateInstance(module.Assembly.ManifestModule.CorLibTypes.Void, module.Assembly.ManifestModule.CorLibTypes.String), 0, MethodAttributes.Public);
            methodDefUser.Body = new CilBody();
            methodDefUser.Body.MaxStack = 1;
            methodDefUser.Body.Instructions.Add(OpCodes.Ldarg_0.ToInstruction());
            methodDefUser.Body.Instructions.Add(OpCodes.Call.ToInstruction(new MemberRefUser(module.Assembly.ManifestModule, ".ctor", MethodSig.CreateInstance(module.Assembly.ManifestModule.CorLibTypes.Void), typeRef)));
            methodDefUser.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
            typeDefUser.Methods.Add(methodDefUser);
            CustomAttribute customAttribute = new CustomAttribute(methodDefUser);
            customAttribute.ConstructorArguments.Add(new CAArgument(module.Assembly.ManifestModule.CorLibTypes.String, ""));
            module.Assembly.ManifestModule.CustomAttributes.Add(customAttribute);
        }
    }
}