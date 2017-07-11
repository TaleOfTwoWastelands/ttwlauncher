﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using Ionic.Zlib;
using System.Xml.Serialization;

namespace org.foesmm.libbsa
{
    public class Fo3File : BSAAsset, IBSAFile
    {
        protected UInt32 _size;
        public UInt32 Size
        {
            get
            {
                return _size;
            }
            set
            {
                if (((Fo3Archive.BSArchiveFlag)value & Fo3Archive.BSArchiveFlag.FileInverseCompressed) == Fo3Archive.BSArchiveFlag.FileInverseCompressed)
                {
                    value = (UInt32)((Fo3Archive.BSArchiveFlag)value ^ Fo3Archive.BSArchiveFlag.FileInverseCompressed);
                    _invertCompression = true;
                }
                _size = value;
            }
        }
        public UInt32 Offset;

        public FileInfo File { get; private set; }

        public bool Prefixed;
        public bool ArchiveCompressed;
        private bool _invertCompression;

        public bool Compressed => ArchiveCompressed ^ _invertCompression;

        public BinaryReader Reader;
        public byte[] Checksum;

        public byte[] GetData()
        {
            if (File != null)
            {
                return System.IO.File.ReadAllBytes(File.FullName);
            }

            byte[] buffer = new byte[Size];
            long outSize = buffer.Length;

            Reader.BaseStream.Seek(Offset, SeekOrigin.Begin);
            if (Prefixed)
            {
                var len = Reader.ReadByte();
                outSize -= len + 1;
                Reader.BaseStream.Seek(len, SeekOrigin.Current);
            }
            if (Compressed)
            {
                outSize = Reader.ReadUInt32();
                buffer = new byte[buffer.Length - 4];
            }
            Reader.Read(buffer, 0, buffer.Length);

            if (Compressed)
            {
                using (var compressedStream = new MemoryStream(buffer))
                {
                    using (var stream = new ZlibStream(compressedStream, CompressionMode.Decompress))
                    {
                        var outBuffer = new byte[outSize];
                        stream.Read(outBuffer, 0, outBuffer.Length);
                        return outBuffer;
                    }
                }
            }

            return buffer;
        }

        public Fo3File()
        {

        }

        public Fo3File(FileInfo file) : this()
        {
            File = file;
            Name = file.Name.ToLower();
            Size = (UInt32)file.Length;
            using (var stream = new BufferedStream(file.OpenRead(), 8 * 1024 * 1024)) {
                Checksum = MD5.Create().ComputeHash(stream);
            }
        }
    }
}
