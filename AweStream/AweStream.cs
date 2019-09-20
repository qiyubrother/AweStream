using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using AweStream.Win32;

namespace AweStream
{
    public class AweStream : Stream, IDisposable
    {
        private static bool _SetLockPagesPrivilegeOk = false;
        private static object _SetLockPagesPrivilegeLockObj = new object();

        unsafe void* _VirtualAddress = null;
        private UInt32 _Capacity = 0;
        private UInt32 _PageSize;
        private UInt32 _NumberOfPages = 0;
        private IntPtr _PFNArray = IntPtr.Zero;
        private UInt32 _PFNArraySize;
        private bool _AweAllocated = false;
        private bool _CanWrite = false;
        private UInt32 _Position = 0;


        public UInt32 Capacity
        {
            get
            {
                return _Capacity;
            }
        }

        public UInt32 PageSize
        {
            get
            {
                return _PageSize;
            }
        }

        public UInt32 NumberOfPages
        {
            get
            {
                return _NumberOfPages;
            }
        }

        public bool IsMapped
        {
            get
            {
                unsafe
                {
                    return _VirtualAddress != null;
                }
            }
        }

        unsafe public byte* LpMemory
        {
            get
            {
                return (byte*)_VirtualAddress;
            }
        }


        #region Constructor
        ~AweStream()
        {
            Dispose();
        }

        public AweStream(UInt32 capacity)
        {
            unsafe
            {
                // Enable the privilege of lock memory.

                lock (_SetLockPagesPrivilegeLockObj)
                {
                    if (!_SetLockPagesPrivilegeOk)
                    {
                        LoggedSetLockPagesPrivilege.SetLockPagesPrivilege(System.Diagnostics.Process.GetCurrentProcess(), true);
                        _SetLockPagesPrivilegeOk = true;
                    }
                }

                General.SYSTEM_INFO sysInfo;
                General.GetSystemInfo(out sysInfo);  // fill the system information structure

                _PageSize = sysInfo.dwPageSize;
                if ((capacity % _PageSize) != 0)
                {
                    _NumberOfPages = capacity / _PageSize + 1;
                }
                else
                {
                    _NumberOfPages = capacity / _PageSize;
                }

                _PFNArraySize = (UInt32)(_NumberOfPages * sizeof(UInt64*));               // memory to request for PFN array

                _PFNArray = Marshal.AllocHGlobal((int)_PFNArraySize);

                UInt32 numberOfPagesInitial = _NumberOfPages;

                if (!AweApi.AllocateUserPhysicalPages(System.Diagnostics.Process.GetCurrentProcess().Handle,
                    ref _NumberOfPages, _PFNArray))
                {
                    Dispose();
                    throw new AweStreamException("Cannot allocate physical pages", AweStreamException.Reason.CannotAllocatePhysicalPages);
                }

                _AweAllocated = true;

                if (numberOfPagesInitial != _NumberOfPages)
                {
                    Dispose();
                    throw new AweStreamException(string.Format("Allocated only {0} pages.", _NumberOfPages),
                         AweStreamException.Reason.AweMemoryNotEnough);
                }

                _Capacity = _PageSize * _NumberOfPages;
            }

        }

        #endregion

        #region Map methods

        public void Map(bool readOnly)
        {
            unsafe
            {
                if (IsMapped)
                {
                    return;
                }

                if (readOnly)
                {
                    _VirtualAddress = AweApi.VirtualAlloc(null, Capacity, AweApi.MEM_RESERVE | AweApi.MEM_PHYSICAL,
                                        AweApi.PAGE_READONLY);
                }
                else
                {
                    _VirtualAddress = AweApi.VirtualAlloc(null, Capacity, AweApi.MEM_RESERVE | AweApi.MEM_PHYSICAL,
                                        AweApi.PAGE_READWRITE);
                }

                if (_VirtualAddress == null)
                {
                    throw new AweStreamException("Cannot reserve memory.", AweStreamException.Reason.CannotReserveMemory);
                }

                if (!AweApi.MapUserPhysicalPages(_VirtualAddress, _NumberOfPages, _PFNArray))
                {
                    AweApi.VirtualFree(_VirtualAddress, Capacity, AweApi.MEM_RELEASE);
                    _VirtualAddress = null;
                    throw new AweStreamException(string.Format("MapUserPhysicalPages failed ({0})", General.GetLastError()),
                        AweStreamException.Reason.MapUserPhysicalPagesFail);
                }

                _CanWrite = !readOnly;
            }
        }

        public void Map()
        {
            Map(false);
        }

        public void UnMap()
        {
            unsafe
            {
                if (_VirtualAddress != null)
                {
                    if (!AweApi.MapUserPhysicalPages(_VirtualAddress, _NumberOfPages, IntPtr.Zero))
                    {
                        throw new AweStreamException(string.Format("UnMapUserPhysicalPages failed ({0})", General.GetLastError()),
                            AweStreamException.Reason.UnMapUserPhysicalPagesFail);

                    }

                    if (!AweApi.VirtualFree(_VirtualAddress, 0, AweApi.MEM_RELEASE))
                    {
                        throw new AweStreamException(string.Format("VitualFree failed ({0})", General.GetLastError()),
                            AweStreamException.Reason.VitualFreeFail);

                    }

                    _VirtualAddress = null;
                }
            }
        }

        #endregion


        #region Stream methods

        public override bool CanRead
        {
            get 
            {
                return IsMapped;
            }
        }

        public override bool CanSeek
        {
            get 
            {
                return IsMapped;
            }
        }

        public override bool CanWrite
        {
            get 
            {
                return IsMapped && _CanWrite;
            }
        }

        public override void Flush()
        {
            
        }

        public override long Length
        {
            get 
            {
                return Capacity;
            }
        }

        public override long Position
        {
            get
            {
                return (long)_Position;
            }

            set
            {
                if (value > Capacity || value < 0)
                {
                    throw new AweStreamException("Invalid position", AweStreamException.Reason.InvalidPosition);
                }

                _Position = (UInt32)value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int relCount;

            if (count <= 0)
            {
                return 0;
            }

            if (Position + count > Length)
            {
                relCount =(int)(Length - Position);
            }
            else
            {
                relCount = count;
            }

            if (relCount <= 0)
            {
                return 0;
            }

            unsafe
            {
                if (count <= 8)
                {
                    int byteCount = count;
                    while (--byteCount >= 0)
                    {
                        buffer[offset + byteCount] = LpMemory[(int)Position + byteCount];
                    }
                }
                else
                {
                    Win32.Buffer.memcpy(LpMemory, (int)Position, buffer, offset, count);
                }
            }

            Position += relCount; 
            return relCount;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length + offset;
                    break;
            }

            return Position;
        }

        public override void SetLength(long value)
        {
            throw new AweStreamException("Cannot change length", AweStreamException.Reason.CannotChangeLength);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count <= 0)
            {
                return;
            }

            if (Position + count > Length)
            {
                throw new System.IndexOutOfRangeException();
            }

            unsafe
            {
                if (count <= 8)
                {
                    int byteCount = count;
                    while (--byteCount >= 0)
                    {
                        LpMemory[(int)Position + byteCount] = buffer[offset + byteCount];
                    }
                }
                else
                {
                    Win32.Buffer.memcpy(buffer, offset, LpMemory, (int)Position, count);
                }
            }

            Position += count;
        }
    

        #endregion

        #region IDisposable Members

        public new void Dispose()
        {
            try
            {
                UnMap();
            }
            catch
            {
            }

            try
            {
                if (_AweAllocated)
                {
                    AweApi.FreeUserPhysicalPages(System.Diagnostics.Process.GetCurrentProcess().Handle,
                        ref _NumberOfPages, _PFNArray);

                    _AweAllocated = false;
                }
            }
            catch
            {
            }

            try
            {
                if (_PFNArray != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_PFNArray);
                    _PFNArray = IntPtr.Zero;
                }
            }
            catch
            {
            }

            try
            {
            }
            catch
            {
                base.Dispose();
            }
        }

        #endregion
    }
}
