using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.foesmm.libbsa
{
    public interface IBSAAsset
    {
        UInt64 NameHash { get; set; }
        string Name { get; set; }
    }
}
