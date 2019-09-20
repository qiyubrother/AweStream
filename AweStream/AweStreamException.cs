using System;
using System.Collections.Generic;
using System.Text;

namespace AweStream
{
    class AweStreamException : Exception
    {
        public enum Reason
        {
            Success = 0,
            CannotAllocatePhysicalPages = 1,
            AweMemoryNotEnough = 2,
            CannotReserveMemory = 3,
            MapUserPhysicalPagesFail = 4,
            UnMapUserPhysicalPagesFail = 5,
            VitualFreeFail = 6,
            InvalidPosition = 7,
            CannotChangeLength = 8,
            Unknown = 255,
        }

        public AweStreamException(string message, Reason reason)
            : base(message) 
        {
        }
    }
}
