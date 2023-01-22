using System.Text;
using System;

public static class StringDecryptor
{
    public static string Decrypt(string input)
    {
        if (System.Reflection.Assembly.GetExecutingAssembly() != System.Reflection.Assembly.GetCallingAssembly())
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
            return "";
        }

        if ((bool)Type.GetType("System.Reflection.Assembly").GetMethod("op_Inequality").Invoke(null, new object[] { Type.GetType("System.Reflection.Assembly").GetMethod("GetExecutingAssembly").Invoke(null, null), Type.GetType("System.Reflection.Assembly").GetMethod("GetCallingAssembly").Invoke(null, null) }))
        {
            Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
            return "";
        }

        System.Security.Cryptography.RijndaelManaged AES = new System.Security.Cryptography.RijndaelManaged();
        byte[] hash = new byte[32];
        byte[] temp = new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(Encoding.Unicode.GetBytes("k"));
        Array.Copy(temp, 0, hash, 0, 16);
        Array.Copy(temp, 0, hash, 15, 16);
        AES.Key = hash;
        AES.Mode = System.Security.Cryptography.CipherMode.ECB;
        byte[] buffer = (byte[]) Type.GetType("System.Convert").GetMethod("FromBase64String").Invoke(null, new object[] { input });
        return Encoding.Unicode.GetString(AES.CreateDecryptor().TransformFinalBlock(buffer, 0, buffer.Length));
    }
}