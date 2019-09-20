using System;
using System.Collections.Generic;
using System.Text;

namespace AweStream.Win32
{
    public class LoggedSetLockPagesPrivilegeException : Exception
    {
        public enum Reason
        {
            Success = 0,
            CannotOpenProcessToken = 1,
            CannotGetPrivilege = 2,
            CannotAdjustTokenPrivileges = 3,
            Unknown = 255,
        }

        public LoggedSetLockPagesPrivilegeException(string message, Reason reason)
            : base(message)
        {
        }
    }
}
