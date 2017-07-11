using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.foesmm.libbsa
{
    public class Fo3Folder : BSAAsset, IBSAFolder
    {
        protected BSAAssets<IBSAFile> _files;

        public UInt32 FileCount => Files.Capacity;

        public BSAAssets<IBSAFile> Files => _files;

        public UInt32 Offset;


        public Fo3Folder()
        {
            _files = new BSAAssets<IBSAFile>();
        }

        public Fo3Folder(string path) : this()
        {
            Name = path;
        }
    }
}
