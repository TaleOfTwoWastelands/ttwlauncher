using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.taleoftwowastelands.patchmaker
{
    [Flags]
    public enum ActionType
    {
        New = 1 << 0,
        Copy = 1 << 1,
        Rename = 1 << 2,
        Delete = 1 << 3,
        Patch = 1 << 4
    }
}
