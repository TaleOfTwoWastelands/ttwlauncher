using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Xml.Serialization;

namespace org.foesmm.libbsa
{
    public abstract class BSArchive : IDisposable
    {
        public enum Version
        {
            Morrowind,
            Oblivion,
            Fallout3,
            FalloutNewVegas,
            Skyrim,
            SkyrimSE,
            Fallout4
        }

        public string File { get; private set; }
        public VersionSignature Signature { get; private set; }
        protected BinaryReader Reader { get; private set; }
        protected BSAAssets<IBSAFolder> _folders = new BSAAssets<IBSAFolder>();

        public byte[] Checksum { get; private set; }

        public static IBSArchive Open(string filename)
        {
            var reader = new BinaryReader(new FileStream(filename, FileMode.Open));
            var header = VersionSignature.Read(reader);
            reader.Close();

            if (header.HeaderSignature == VersionSignature.Signature.Oblivion && header.HeaderVersion == VersionSignature.Version.Fallout3)
            {
                return new Fo3Archive(filename);
            } else {
                throw new UnsupportedArchiveException(header);
            }
        }

        public static IBSArchive Create(Version version, string filename, string looseFilesDirectory)
        {
            switch (version)
            {
                case Version.FalloutNewVegas:
                    var archive = new Fo3Archive(filename);
                    archive.Signature = new VersionSignature(VersionSignature.Signature.Oblivion, VersionSignature.Version.Fallout3);
                    archive.AddFiles(looseFilesDirectory);
                    archive.RebuildHeader();

                    return archive;
                default:
                    throw new UnsupportedArchiveException(string.Format("Archive creation for {0} is not supported", Enum.GetName(version.GetType(), version)));
            }
        }

        public void Dispose()
        {
            Reader.Close();
        }

        public BSArchive()
        {

        }

        public BSArchive(string filename) {
            File = filename;
            var stream = new BufferedStream(new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite), 8 * 1024 * 1024);

            var checksumStopwatch = Stopwatch.StartNew();
            Checksum = MD5.Create().ComputeHash(stream);
            checksumStopwatch.Stop();
            Console.WriteLine(
                "MD5 checksum {0}\nTook {1}",
                BitConverter.ToString(Checksum).Replace("-", string.Empty).ToLower(),
                checksumStopwatch.Elapsed
                );
            stream.Seek(0, SeekOrigin.Begin);

            Reader = new BinaryReader(stream);
            Signature = VersionSignature.Read(Reader);
        }

        public struct VersionSignature
        {
            public Signature HeaderSignature { get; private set; }
            public Version HeaderVersion { get; private set; }

            public enum Signature : UInt32
            {
                Unknown,
                Morrowind = 0x00000100,
                Oblivion = 0x00415342,
                Fallout4 = 0x58445442
            }

            public enum Version : UInt32
            {
                Unknown,
                Oblivion = 0x67,
                Fallout3 = 0x68,
                SkyrimSE = 0x69,
                Fallout4 = 0x01
            }

            public VersionSignature(Signature signature, Version version)
            {
                HeaderSignature = signature;
                HeaderVersion = version;
            }

            public static VersionSignature Read(BinaryReader reader)
            {
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                var result = new VersionSignature();
                try
                {
                    result.HeaderSignature = (Signature)reader.ReadUInt32();
                    result.HeaderVersion = (Version)reader.ReadUInt32();
                } catch (EndOfStreamException e)
                {
                    result.HeaderSignature = Signature.Unknown;
                    result.HeaderVersion = Version.Unknown;
                }
                return result;
            }

            internal void Write(BinaryWriter writer)
            {
                writer.Write((UInt32)HeaderSignature);
                writer.Write((UInt32)HeaderVersion);
            }
        }

        public void WriteDescriptor()
        {
           throw new NotImplementedException();
        }
    }
}
