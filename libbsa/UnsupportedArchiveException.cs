using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.foesmm.libbsa
{
    class UnsupportedArchiveException : Exception
    {
        public UnsupportedArchiveException(BSArchive.VersionSignature header) : base(string.Format("Unsupported archive with signature: {0} ({1})", Enum.GetName(typeof(BSArchive.VersionSignature.Signature), header.HeaderSignature), Enum.GetName(typeof(BSArchive.VersionSignature.Version), header.HeaderVersion))) { }
        public UnsupportedArchiveException(string message) : base(message) { }
    }
}
