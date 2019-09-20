using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AweStream.Win32
{
    class Buffer
    {
        [DllImport("msvcrt.dll", SetLastError = false)]
        unsafe internal static extern IntPtr memcpy(byte* dest, byte* src, int count);

        internal unsafe static void memcpy(byte* src, int srcIndex, byte[] dest, int destIndex, int len)
        {
            Debug.Assert((srcIndex >= 0) && (destIndex >= 0) && (len >= 0), "Index and length must be non-negative!");
            Debug.Assert(dest.Length - destIndex >= len, "not enough bytes in dest");
            // If dest has 0 elements, the fixed statement will throw an
            // IndexOutOfRangeException.  Special-case 0-byte copies. 
            if (len == 0)
                return;
            fixed (byte* pDest = dest)
            {
                memcpy(src + srcIndex, pDest + destIndex, len);
            }
        }

        internal unsafe static void memcpy(byte[] src, int srcIndex, byte* pDest, int destIndex, int len)
        {
            Debug.Assert((srcIndex >= 0) && (destIndex >= 0) && (len >= 0), "Index and length must be non-negative!");
            Debug.Assert(src.Length - srcIndex >= len, "not enough bytes in src");
            // If dest has 0 elements, the fixed statement will throw an
            // IndexOutOfRangeException.  Special-case 0-byte copies. 
            if (len == 0)
                return;
            fixed (byte* pSrc = src)
            {
                memcpy(pSrc + srcIndex, pDest + destIndex, len);
            }
        } 
    }
}
