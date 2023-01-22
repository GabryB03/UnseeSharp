using dnlib.DotNet;
using System.Collections.Generic;
using System.Text;
using System;

public class Renamer
{
    private static char[] _characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

    public static void Process(ModuleDefMD module)
    {
        int toGenerate = 10;

        foreach (TypeDef type in module.Types)
        {
            foreach (FieldDef field in type.Fields)
            {
                if (field.IsRuntimeSpecialName || (field.IsLiteral && field.DeclaringType.IsEnum))
                {
                    continue;
                }

                toGenerate++;
            }

            foreach (EventDef theEvent in type.Events)
            {
                if (theEvent.IsRuntimeSpecialName)
                {
                    continue;
                }

                toGenerate++;
            }

            foreach (PropertyDef property in type.Properties)
            {
                if (property.IsRuntimeSpecialName)
                {
                    continue;
                }

                toGenerate++;
            }

            foreach (MethodDef method in type.Methods)
            {
                if (method.Name == "AntiDebug" || method.FullName.Contains("UNSEESHARP_OBFUSCATOR_STRING_ENCRYPTION_KEY_OBFUSCATION") || method.FullName.ToLower().Contains("stringpoolingobfuscation_") || method.IsConstructor || method.IsFamily || method.IsRuntimeSpecialName || method.DeclaringType.IsForwarder || method.HasOverrides || method.IsVirtual)
                {
                    continue;
                }

                toGenerate++;

                foreach (Parameter parameter in method.Parameters)
                {
                    toGenerate++;
                }
            }

            if (type.IsRuntimeSpecialName || type.IsSpecialName || type.Interfaces.Count > 0 || type.IsForwarder || type.IsGlobalModuleType || type.IsWindowsRuntime || (type.BaseType != null && type.BaseType.FullName.ToLower().Contains("form")))
            {
                continue;
            }

            toGenerate += 2;
        }

        List<string> strings = new List<string>();
        int currentGenerated = 0;
        int currentLength = 1;
        bool canContinue = true;

        while (canContinue)
        {
            if (currentLength == 1)
            {
                foreach (char str in _characters)
                {
                    strings.Add(str.ToString());
                    currentGenerated++;
                }

                currentLength = 2;
            }
            else
            {
                string str = "", finalStr = "";

                for (int i = 0; i < currentLength; i++)
                {
                    str += _characters[0];
                }

                for (int i = 0; i < currentLength; i++)
                {
                    finalStr += _characters[_characters.Length - 1];
                }

                strings.Add(str);
                currentGenerated++;

                while (canContinue)
                {
                    if (str == finalStr)
                    {
                        break;
                    }
                    else
                    {
                        char lastCharacter = str[str.Length - 1];

                        for (int i = 0; i < _characters.Length; i++)
                        {
                            if (lastCharacter == _characters[i])
                            {
                                lastCharacter = _characters[i + 1];
                                break;
                            }
                        }

                        string newStr = str.Substring(0, str.Length - 1);
                        newStr += lastCharacter;
                        str = newStr;

                        if (currentLength >= 3 && str.StartsWith("9") && str.EndsWith("9"))
                        {
                            str = "";

                            for (int i = 0; i < currentLength; i++)
                            {
                                str += "a";
                            }
                        }

                        List<Tuple<int, char>> modifications = new List<Tuple<int, char>>();

                        for (int i = 0; i < str.Length; i++)
                        {
                            if (str[i] == _characters[_characters.Length - 1])
                            {
                                if (i != 0)
                                {
                                    modifications.Add(new Tuple<int, char>(i, _characters[0]));
                                }

                                if (i != 0)
                                {
                                    if (str[i - 1] == _characters[_characters.Length - 1])
                                    {
                                        modifications.Add(new Tuple<int, char>(i - 1, _characters[0]));
                                    }
                                    else
                                    {
                                        int theIndex = 0;

                                        for (int j = 0; j < _characters.Length; j++)
                                        {
                                            if (_characters[j] == str[i - 1])
                                            {
                                                theIndex = j;
                                                break;
                                            }
                                        }

                                        modifications.Add(new Tuple<int, char>(i - 1, _characters[theIndex + 1]));
                                    }
                                }

                                break;
                            }
                        }

                        foreach (Tuple<int, char> modification in modifications)
                        {
                            StringBuilder sb = new StringBuilder(str);
                            sb[modification.Item1] = modification.Item2;
                            str = sb.ToString();
                        }

                        strings.Add(str);
                        currentGenerated++;
                        
                        if (currentGenerated >= toGenerate + 50)
                        {
                            canContinue = false;
                        }
                    }
                }

                currentLength++;
            }
        }

        int current = 0;

        foreach (TypeDef type in module.Types)
        {
            foreach (FieldDef field in type.Fields)
            {
                if (field.IsRuntimeSpecialName || (field.IsLiteral && field.DeclaringType.IsEnum))
                {
                    continue;
                }

                field.Name = strings[current];
                current++;
            }

            foreach (EventDef theEvent in type.Events)
            {
                if (theEvent.IsRuntimeSpecialName)
                {
                    continue;
                }

                theEvent.Name = strings[current];
                current++;
            }

            foreach (PropertyDef property in type.Properties)
            {
                if (property.IsRuntimeSpecialName)
                {
                    continue;
                }

                property.Name = strings[current];
                current++;
            }

            foreach (MethodDef method in type.Methods)
            {
                if (method.Name == "AntiDebug" || method.FullName.Contains("UNSEESHARP_OBFUSCATOR_STRING_ENCRYPTION_KEY_OBFUSCATION") || method.FullName.ToLower().Contains("stringpoolingobfuscation_") || method.IsConstructor || method.IsFamily || method.IsRuntimeSpecialName || method.DeclaringType.IsForwarder || method.HasOverrides || method.IsVirtual)
                {
                    continue;
                }

                method.Name = strings[current];
                current++;

                foreach (Parameter parameter in method.Parameters)
                {
                    parameter.Name = strings[current];
                    current++;
                }
            }

            if (type.IsRuntimeSpecialName || type.IsSpecialName || type.Interfaces.Count > 0 || type.IsForwarder || type.IsGlobalModuleType || type.IsWindowsRuntime || (type.BaseType != null && type.BaseType.FullName.ToLower().Contains("form")))
            {
                continue;
            }

            type.Name = strings[current];
            current++;
            type.Namespace = strings[current];
            current++;
        }

        foreach (TypeDef type in module.Types)
        {
            foreach (MethodDef method in type.Methods)
            {
                if (method.Name.Equals("AntiDebug"))
                {
                    method.Name = "StringPoolingObfuscation_Skid";
                }
            }
        }

        module.Name = strings[current];
        current++;
        module.Assembly.Name = strings[current];
        strings.Clear();
        GC.Collect();
    }
}