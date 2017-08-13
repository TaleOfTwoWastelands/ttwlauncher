using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

namespace org.foesmm.libBSA
{
    public interface IBSA : IDictionary<string, Fo3File>, IEnumerable<Fo3File>, IDisposable
    {
        string Filename { get; set; }
        byte[] Checksum { get; set; }
        BSAAssets<Fo3Folder> Folders { get; set; }
        BinaryReader Reader { get; set; }
        SortedDictionary<string, Fo3File> IndexFullPath { get; set; }
        SortedDictionary<string, Fo3File> IndexFileName { get; set; }

        long FolderCount { get; }
        long FileCount { get; }

        bool ContainsPathNames { get; }
        bool ContainsFileNames { get; }
        bool IsCompressed { get; set; }
        bool IsNamePrefixedToData { get; set; }

        void SetStream(Stream stream);
        void RecalculateChecksum();
        void BuildIndex();
        void WriteDescriptor();
        void Save();
    }
}
