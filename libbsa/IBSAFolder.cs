using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace org.foesmm.libbsa
{
    public interface IBSAFolder : IBSAAsset
    {
        BSAAssets<IBSAFile> Files { get; }
    }
}
