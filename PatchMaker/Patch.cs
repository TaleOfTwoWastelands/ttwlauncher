using System.Xml.Serialization;

namespace com.taleoftwowastelands.patchmaker
{
    public class Patch
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("offset")]
        public long Offset { get; set; }
        [XmlAttribute("size")]
        public long Size { get; set; }
        [XmlAttribute("compressed")]
        public bool Compressed { get; set; }

        public bool ShouldSerializeCompressed()
        {
            return Compressed;
        }
    }
}