using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace com.taleoftwowastelands.patchmaker
{
    [XmlRoot("Settings")]
    public class Settings
    {
        [XmlElement("Version")]
        public string Version { get; set; }
        [XmlElement("Fallout3Data")]
        public string Fo3DataPath { get; set; }
        [XmlElement("CurrentLooseAssets")]
        public string LooseAssets { get; set; }
        [XmlElement("CurrentData")]
        public string CurrentDataPath { get; set; }
        [XmlElement("PatchPath")]
        public string PatchPath { get; set; }
        [XmlElement("Output")]
        public string OutputPath { get; set; }
        [XmlElement("Temp")]
        public string TempPath { get; set; }
        [XmlArray("Files")]
        public List<FilePair> Files { get; set; }

        public Settings()
        {
            Files = new List<FilePair>();
        }

        public static Settings Default()
        {
            return new Settings()
            {
                Version = "Version of TTW",
                Fo3DataPath = "Path to Fallout 3 Data folder",
                LooseAssets = "Path to modified (current) assets",
                CurrentDataPath = "Path to modified (current) packed files",

                PatchPath = "Output path for patches and recipe (will be appended by {version})",
                OutputPath = "Output path for finished (installed) files (will be appended by {version})",

                TempPath = "Path for temporary files",

                Files = new List<FilePair>(new FilePair[] { new FilePair() { OriginalName = "Original.bsa", ResultName = "Result.bsa", Type = ContentType.BSA } })
            };
        }

        internal string GetTempFolder(string subfolder)
        {
            return Directory.CreateDirectory(Path.Combine(TempPath, subfolder)).FullName;
        }
    }
}
