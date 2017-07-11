using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace org.foesmm.libbsa
{
    public interface IBSArchive : IDisposable
    {
        long FolderCount { get; }
        long FileCount { get; }

        bool ContainsPathNames { get; }
        bool ContainsFileNames { get; }
        bool IsCompressed { get; set; }
        bool IsNamePrefixedToData { get; set; }

        BSAAssets<IBSAFolder> Folders { get; }

        void WriteDescriptor();
        void Save();
    }
}
