using System;
using System.Runtime.InteropServices;

namespace Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FixLine
    {
        public FixVector2 point;
        public FixVector2 direction;
    }
}

