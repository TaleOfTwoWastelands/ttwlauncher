using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace com.taleoftwowastelands.patchmaker
{
    [XmlRoot("Recipe")]
    public class Recipe
    {
        [XmlAttribute("from")]
        public string VersionFrom { get; set; }
        [XmlAttribute("to")]
        public string VersionTo { get; set; }
        [XmlArray("Files")]
        public List<FilePair> Files { get; set; }
        [XmlArray("Patches")]
        public List<Patch> Patches { get; set; }
        [XmlArray("Assets")]
        [XmlArrayItem("Asset")]
        public List<Patch> Assets { get; set; }

        public Recipe()
        {
            VersionFrom = "vanilla";
            Files = new List<FilePair>();
            Patches = new List<Patch>();
            Assets = new List<Patch>();
        }
    }
}
