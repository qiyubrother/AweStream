using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace AweStream.Win32
{
    class General
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct SYSTEM_INFO
        {
            //[MarshalAs(UnmanagedType.U4)]
            //public int dwOemId;   // dwOemId is obsolete according to MS: http://msdn.microsoft.com/en-us/library/aa450921.aspx
            [MarshalAs(UnmanagedType.U2)]
            public short wProcessorArchitecture;
            [MarshalAs(UnmanagedType.U2)]
            public short wReserved;
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dwPageSize;
            public uint lpMinimumApplicationAddress;
            public uint lpMaximumApplicationAddress;
            [MarshalAs(UnmanagedType.U4)]
            public int dwActiveProcessorMask;
            [MarshalAs(UnmanagedType.U4)]
            public int dwNumberOfProcessors;
            [MarshalAs(UnmanagedType.U4)]
            public int dwProcessorType;
            [MarshalAs(UnmanagedType.U4)]
            public int dwAllocationGranularity;
            [MarshalAs(UnmanagedType.U2)]
            public short dwProcessorLevel;
            [MarshalAs(UnmanagedType.U2)]
            public short dwProcessorRevision;
        }


        internal const int ERROR_SUCCESS = 0;
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool OpenProcessToken(IntPtr ProcessHandle,
            UInt32 DesiredAccess, out IntPtr TokenHandle);

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern Int32 GetLastError();

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern void GetSystemInfo(out SYSTEM_INFO pSi);
    }
}
