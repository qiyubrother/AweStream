using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace AweStream.Win32
{
    class AweApi
    {
        internal const UInt32 PAGE_NOACCESS          =0x01  ;   
        internal const UInt32 PAGE_READONLY          =0x02  ;   
        internal const UInt32 PAGE_READWRITE         =0x04  ;   
        internal const UInt32 PAGE_WRITECOPY         =0x08  ;   
        internal const UInt32 PAGE_EXECUTE           =0x10  ;   
        internal const UInt32 PAGE_EXECUTE_READ      =0x20  ;   
        internal const UInt32 PAGE_EXECUTE_READWRITE =0x40  ;   
        internal const UInt32 PAGE_EXECUTE_WRITECOPY =0x80  ;   
        internal const UInt32 PAGE_GUARD            =0x100  ;   
        internal const UInt32 PAGE_NOCACHE          =0x200  ;
        internal const UInt32 PAGE_WRITECOMBINE = 0x400;   

        internal const UInt32  MEM_COMMIT         =  0x1000 ;    
        internal const UInt32 MEM_RESERVE         =  0x2000 ;     
        internal const UInt32 MEM_DECOMMIT        =  0x4000 ;     
        internal const UInt32  MEM_RELEASE        =  0x8000 ;     
        internal const UInt32  MEM_FREE           = 0x10000 ;     
        internal const UInt32  MEM_PRIVATE        = 0x20000 ;     
        internal const UInt32  MEM_MAPPED         = 0x40000 ;     
        internal const UInt32  MEM_RESET          = 0x80000 ;     
        internal const UInt32  MEM_TOP_DOWN       =0x100000 ;     
        internal const UInt32  MEM_WRITE_WATCH    =0x200000 ;     
        internal const UInt32  MEM_PHYSICAL       =0x400000 ;     
        internal const UInt32  MEM_LARGE_PAGES  =0x20000000 ;
        internal const UInt32 MEM_4MB_PAGES = 0x80000000;    
        
        
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AllocateUserPhysicalPages(IntPtr hProcess,
           ref UInt32 NumberOfPages, IntPtr PageArray);

        [DllImport("kernel32.dll")]
        internal static extern bool FreeUserPhysicalPages(IntPtr hProcess, ref UInt32
           NumberOfPages, IntPtr UserPfnArray);

        [DllImport("kernel32.dll", SetLastError=true)]
        internal unsafe static extern void* VirtualAlloc(void* lpAddress, UInt32 dwSize,
           UInt32 flAllocationType, UInt32 flProtect);

        [DllImport("kernel32.dll")]
        internal unsafe static extern bool MapUserPhysicalPages(void* lpAddress, UInt32
           NumberOfPages, IntPtr UserPfnArray);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal unsafe static extern bool VirtualFree(void* lpAddress, UInt32 dwSize,
           UInt32 dwFreeType);
    }
}
