using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

namespace AweStream.Win32
{


    /// <summary>
    /// Configuration
    /// gpedit.msc ->Windows Settings->Security Settings->Local Policies->User Rights Assignment->Lock pages in memory
    /// Add the account 
    /// or use system account
    /// </summary>
    public class LoggedSetLockPagesPrivilege
    {
        private const Int32 ANYSIZE_ARRAY = 1;
        private const string SE_LOCK_MEMORY_NAME = "SeLockMemoryPrivilege";

        
        internal const UInt32 SE_PRIVILEGE_ENABLED_BY_DEFAULT = 0x00000001;
        internal const UInt32 SE_PRIVILEGE_ENABLED = 0x00000002;
        internal const UInt32 SE_PRIVILEGE_REMOVED = 0x00000004;
        internal const UInt32 SE_PRIVILEGE_USED_FOR_ACCESS = 0x80000000;

        [StructLayout(LayoutKind.Sequential)]
        internal struct LUID
        {
            public UInt32 LowPart;
            public Int32 HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;
            public UInt32 Attributes;
        }

        internal struct TOKEN_PRIVILEGES
        {
            public UInt32 PrivilegeCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = ANYSIZE_ARRAY)]
            public LUID_AND_ATTRIBUTES[] Privileges;
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool LookupPrivilegeValue(string lpSystemName, string lpName,
            out LUID lpLuid);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AdjustTokenPrivileges(IntPtr TokenHandle,
           [MarshalAs(UnmanagedType.Bool)]bool DisableAllPrivileges,
           ref TOKEN_PRIVILEGES NewState,
           UInt32 BufferLength,
           IntPtr PreviousState,
           IntPtr ReturnLength);

        /// <summary>
        /// Set lock pages privilege
        /// </summary>
        /// <param name="process">process</param>
        /// <param name="enable"></param>
        static public void SetLockPagesPrivilege(Process process, bool enable)
        {
            IntPtr token;
            TOKEN_PRIVILEGES info = new TOKEN_PRIVILEGES();

            // Open the token.

            bool result = General.OpenProcessToken(process.Handle,
                                        General.TOKEN_ADJUST_PRIVILEGES,
                                        out token);
            if (!result)
            {
                throw new LoggedSetLockPagesPrivilegeException("Cannot open process token.", 
                    LoggedSetLockPagesPrivilegeException.Reason.CannotOpenProcessToken);
            }

            info.PrivilegeCount = 1;
            info.Privileges = new LUID_AND_ATTRIBUTES[1];

            // Enable or disable?

            if (enable)
            {
                info.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
            }
            else
            {
                info.Privileges[0].Attributes = 0;
            }


            // Get the LUID.
            result = LookupPrivilegeValue(null, SE_LOCK_MEMORY_NAME,
                                              out info.Privileges[0].Luid);

            if (!result)
            {
                General.CloseHandle(token);
                throw new LoggedSetLockPagesPrivilegeException("Cannot get privilege.",
                    LoggedSetLockPagesPrivilegeException.Reason.CannotGetPrivilege);
            }

            // Adjust the privilege.

            result = AdjustTokenPrivileges(token, false,
                                             ref info,
                                             0, IntPtr.Zero, IntPtr.Zero);

            // Check the result.

            if (!result)
            {
                General.CloseHandle(token);
                throw new LoggedSetLockPagesPrivilegeException(string.Format("Cannot adjust token privileges {0}.", General.GetLastError()),
                    LoggedSetLockPagesPrivilegeException.Reason.CannotAdjustTokenPrivileges);
            }
            else
            {
                int lastError = General.GetLastError();

                if (lastError != General.ERROR_SUCCESS)
                {
                    General.CloseHandle(token);
                    throw new LoggedSetLockPagesPrivilegeException(string.Format("Cannot enable the SE_LOCK_MEMORY_NAME privilege {0}; please check the local policy.", lastError),
                        LoggedSetLockPagesPrivilegeException.Reason.Unknown);
                }
            }

            General.CloseHandle(token);

        }
    }
}
