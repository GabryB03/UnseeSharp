using System.Diagnostics;
using System.Runtime;
using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Management;

public class AntiManipulationRuntime
{
    [DllImport("psapi.dll")]
    public static extern int EmptyWorkingSet(IntPtr hwProc);

    [DllImport("kernel32.dll")]
    public static extern IntPtr LoadLibrary(string dllToLoad);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

    [DllImport("kernel32.dll")]
    public static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern int OutputDebugString(string str);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool CloseHandle(IntPtr handle);

    [DllImport("kernel32.dll")]
    public static extern bool IsDebuggerPresent();

    [DllImport("ntdll.dll", SetLastError = true, ExactSpelling = true)]
    private static extern IntPtr NtQueryInformationProcess([In] IntPtr ProcessHandle, int ProcessInformationClass, out IntPtr ProcessInformation, [In] int ProcessInformationLength, [Optional] out int ReturnLength);

    internal enum MemoryProtection
    {
        ExecuteReadWrite = 0x40,
    }
    public static unsafe void CopyBlock(void* destination, void* source, uint byteCount)
    {
    }
    public static unsafe void InitBlock(void* startAddress, byte value, uint byteCount)
    {
    }
    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, [MarshalAs(UnmanagedType.U4)] MemoryProtection flNewProtect, [MarshalAs(UnmanagedType.U4)] out MemoryProtection lpflOldProtect);

    [DllImport("kernel32.dll")]
    private static extern unsafe bool VirtualProtect(byte* lpAddress, int dwSize, uint flNewProtect, out uint lpflOldProtect);

    public static void RunAll()
    {
        /*if (System.Reflection.Assembly.GetExecutingAssembly() != System.Reflection.Assembly.GetCallingAssembly())
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
            return;
        }

        if ((bool)Type.GetType("System.Reflection.Assembly").GetMethod("op_Inequality").Invoke(null, new object[] { Type.GetType("System.Reflection.Assembly").GetMethod("GetExecutingAssembly").Invoke(null, null), Type.GetType("System.Reflection.Assembly").GetMethod("GetCallingAssembly").Invoke(null, null) }))
        {
            Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
            return;
        }*/

        string assemblyLocation = (string)Type.GetType("System.Reflection.Assembly").GetMethod("GetExecutingAssembly").Invoke(null, null).GetType().GetProperty("Location").GetValue(Type.GetType("System.Reflection.Assembly").GetMethod("GetExecutingAssembly").Invoke(null, null));
        StreamReader streamReader = new StreamReader(assemblyLocation);
        Stream stream = (Stream) streamReader.GetType().GetProperty("BaseStream").GetValue(streamReader);
        BinaryReader reader = new BinaryReader(stream);
        string realMd5 = null, newMd5 = null;
        byte[] readAll = (byte[]) Type.GetType("System.IO.File").GetMethod("ReadAllBytes").Invoke(null, new object[] { assemblyLocation });
        int length = (int) readAll.GetType().GetProperty("Length").GetValue(readAll) - 16;
        byte[] thing = (byte[]) reader.GetType().GetMethod("ReadBytes").Invoke(reader, new object[] { length });
        newMd5 = BitConverter.ToString(System.Security.Cryptography.MD5.Create().ComputeHash(thing));
        stream.GetType().GetMethod("Seek").Invoke(stream, new object[] { -16, SeekOrigin.End });
        byte[] theBytes = (byte[]) reader.GetType().GetMethod("ReadBytes").Invoke(reader, new object[] { 16 });
        realMd5 = BitConverter.ToString(theBytes);

        if ((bool) Type.GetType("System.String").GetMethod("op_Inequality").Invoke(null, new object[] { newMd5, realMd5 }))
        {
            Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
            return;
        }

        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
        {
            using (ManagementObjectCollection items = searcher.Get())
            {
                foreach (ManagementBaseObject item in items)
                {
                    string manufacturer = item["Manufacturer"].ToString().ToLower();

                    if (manufacturer == "microsoft corporation" && item["Model"].ToString().ToUpperInvariant().Contains("VIRTUAL") || manufacturer.Contains("vmware") || item["Model"].ToString() == "VirtualBox")
                    {
                        Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                        return;
                    }
                }
            }
        }

        List<string> sandboxStrings = new List<string>() { "vmware", "virtualbox", "vbox", "qemu", "xen" };
        string[] HKLM_Keys_To_Check_Exist = new[] { @"HARDWARE\DEVICEMAP\Scsi\Scsi Port 2\Scsi Bus 0\Target Id 0\Logical Unit Id 0\Identifier", @"SYSTEM\CurrentControlSet\Enum\SCSI\Disk&Ven_VMware_&Prod_VMware_Virtual_S", @"SYSTEM\CurrentControlSet\Control\CriticalDeviceDatabase\root#vmwvmcihostdev", @"SYSTEM\CurrentControlSet\Control\VirtualDeviceDrivers", @"SOFTWARE\VMWare, Inc.\VMWare Tools", @"SOFTWARE\Oracle\VirtualBox Guest Additions", @"HARDWARE\ACPI\DSDT\VBOX_" };
        string[] HKLM_Keys_With_Values_To_Parse = new[] { @"SYSTEM\ControlSet001\Services\Disk\Enum\0", @"HARDWARE\Description\System\SystemBiosInformation", @"HARDWARE\Description\System\VideoBiosVersion", @"HARDWARE\Description\System\SystemManufacturer", @"HARDWARE\Description\System\SystemProductName", @"HARDWARE\Description\System\Logical Unit Id 0" };

        foreach (string HKLM_Key in HKLM_Keys_To_Check_Exist)
        {
            RegistryKey OpenedKey = Registry.LocalMachine.OpenSubKey(HKLM_Key, false);

            if (OpenedKey is object)
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
        }

        foreach (string HKLM_Key in HKLM_Keys_With_Values_To_Parse)
        {
            DirectoryInfo info = new DirectoryInfo(HKLM_Key);
            string valueName = (string) info.GetType().GetProperty("Name").GetValue(info);
            string directoryName = (string)Type.GetType("System.IO.Path").GetMethod("GetDirectoryName").Invoke(null, new object[] { HKLM_Key });
            string value = Convert.ToString(Registry.LocalMachine.OpenSubKey(directoryName, false).GetValue(valueName));

            foreach (string sandboxString in sandboxStrings)
            {
                if (!string.IsNullOrEmpty(value) && value.ToLower().Contains(sandboxString.ToLower()))
                {
                    Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                    return;
                }
            }
        }

        string[] FilePaths = new[] { @"C:\windows\Sysnative\Drivers\Vmmouse.sys", @"C:\windows\Sysnative\Drivers\vm3dgl.dll", @"C:\windows\Sysnative\Drivers\vmdum.dll", @"C:\windows\Sysnative\Drivers\vm3dver.dll", @"C:\windows\Sysnative\Drivers\vmtray.dll", @"C:\windows\Sysnative\Drivers\vmusbmouse.sys", @"C:\windows\Sysnative\Drivers\vmx_svga.sys", @"C:\windows\Sysnative\Drivers\vmxnet.sys", @"C:\windows\Sysnative\Drivers\VMToolsHook.dll", @"C:\windows\Sysnative\Drivers\vmhgfs.dll", @"C:\windows\Sysnative\Drivers\vmmousever.dll", @"C:\windows\Sysnative\Drivers\vmGuestLib.dll", @"C:\windows\Sysnative\Drivers\VmGuestLibJava.dll", @"C:\windows\Sysnative\Drivers\vmscsi.sys", @"C:\windows\Sysnative\Drivers\VBoxMouse.sys", @"C:\windows\Sysnative\Drivers\VBoxGuest.sys", @"C:\windows\Sysnative\Drivers\VBoxSF.sys", @"C:\windows\Sysnative\Drivers\VBoxVideo.sys", @"C:\windows\Sysnative\vboxdisp.dll", @"C:\windows\Sysnative\vboxhook.dll", @"C:\windows\Sysnative\vboxmrxnp.dll", @"C:\windows\Sysnative\vboxogl.dll", @"C:\windows\Sysnative\vboxoglarrayspu.dll", @"C:\windows\Sysnative\vboxoglcrutil.dll", @"C:\windows\Sysnative\vboxoglerrorspu.dll", @"C:\windows\Sysnative\vboxoglfeedbackspu.dll", @"C:\windows\Sysnative\vboxoglpackspu.dll", @"C:\windows\Sysnative\vboxoglpassthroughspu.dll", @"C:\windows\Sysnative\vboxservice.exe", @"C:\windows\Sysnative\vboxtray.exe", @"C:\windows\Sysnative\VBoxControl.exe" };

        foreach (string FilePath in FilePaths)
        {
            if ((bool) Type.GetType("System.IO.File").GetMethod("Exists").Invoke(null, new object[] { FilePath }))
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
        }

        Thread clearRamThread = new Thread(new ThreadStart(ClearRAM));
        clearRamThread.Priority = ThreadPriority.Highest;
        clearRamThread.Start();

        Thread antiDebugThread = new Thread(new ThreadStart(AntiDebug));
        antiDebugThread.Priority = ThreadPriority.Highest;
        antiDebugThread.Start();
    }

    public static void AntiDebug()
    {
        while (true)
        {
            Thread.Sleep(100);

            if (IsFunctionPatched("kernel32.dll", "GetTickCount"))
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
            else if (IsFunctionPatched("kernel32.dll", "GetTickCount64"))
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
            else if (IsFunctionPatched("winmm.dll", "timeGetTime"))
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
            else if (IsFunctionPatched("kernel32.dll", "QueryPerformanceCounter"))
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
            else if (IsFunctionPatched("kernel32.dll", "CheckRemoteDebuggerPresent"))
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
            else if (IsFunctionPatched(typeof(Debugger), "get_IsAttached"))
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
            else if (IsFunctionPatched("kernel32.dll", "CloseHandle"))
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
            else if (IsFunctionPatched("ntdll.dll", "NtQueryInformationProcess"))
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
            else if (IsFunctionPatched("ntdll.dll", "NtClose"))
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
            else if (IsFunctionPatched("ntdll.dll", "NtRemoveProcessDebug"))
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
            else if (IsFunctionPatched("ntdll.dll", "NtSetInformationDebugObject"))
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
            else if (IsFunctionPatched("ntdll.dll", "NtQuerySystemInformation"))
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
            else if (IsFunctionPatched("kernel32.dll", "WriteProcessMemory"))
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
            else if (IsFunctionPatched("kernel32.dll", "ReadProcessMemory"))
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
            else if (IsFunctionPatched("kernel32.dll", "OpenThread"))
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
            else if (IsFunctionPatched("ntdll.dll", "NtSetInformationThread"))
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
            else if (Debugger.IsAttached)
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
            else if (Debugger.IsLogging())
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
            else if (string.Compare(Environment.GetEnvironmentVariable("COR_ENABLE_PROFILING"), "1", StringComparison.Ordinal) == 0)
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
            else if (Process.GetCurrentProcess().Handle == IntPtr.Zero)
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
            else if (OutputDebugString("") > IntPtr.Size)
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
            else if (CheckRemoteDebugger())
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
            else if (IsDebuggerPresent())
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
            else if (CheckDebugPort())
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }

            try
            {
                CloseHandle(IntPtr.Zero);
            }
            catch
            {
                Type.GetType("System.Environment").GetMethod("Exit").Invoke(null, new object[] { 0 });
                return;
            }
        }
    }

    private static bool CheckDebugPort()
    {
        IntPtr DebugPort = new IntPtr(0);
        int ReturnLength;

        unsafe
        {
            if (NtQueryInformationProcess(Process.GetCurrentProcess().Handle, 7, out DebugPort, Marshal.SizeOf(DebugPort), out ReturnLength) == new IntPtr(0x00000000))
            {
                return DebugPort == new IntPtr(-1);
            }
        }

        return false;
    }

    private static bool CheckRemoteDebugger()
    {
        var isDebuggerPresent = false;
        var bApiRet = CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref isDebuggerPresent);
        return bApiRet && isDebuggerPresent;
    }

    public static bool IsFunctionPatched(string library, string functionName)
    {
        IntPtr kernel32 = LoadLibrary(library);
        IntPtr GetProcessId = GetProcAddress(kernel32, functionName);
        byte[] data = new byte[1];
        Marshal.Copy(GetProcessId, data, 0, 1);
        return data[0] == 0xE9;
    }

    public static bool IsFunctionPatched(Type theType, string methodName)
    {
        byte[] data = new byte[1];
        var getMethod = theType.GetMethod(methodName);
        IntPtr targetAddre = getMethod.MethodHandle.GetFunctionPointer();
        Marshal.Copy(targetAddre, data, 0, 1);
        return data[0] == 0x33;
    }

    public static void ClearRAM()
    {
        while (true)
        {
            Thread.Sleep(100);
            EmptyWorkingSet(Process.GetCurrentProcess().Handle);
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(GC.MaxGeneration);
            GC.WaitForPendingFinalizers();
        }
    }
}