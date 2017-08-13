using System.Xml.Serialization;

namespace com.taleoftwowastelands.patchmaker
{
    public class RecordAction
    {
        [XmlAttribute("origin")]
        public string Origin { get; set; }
        [XmlAttribute("path")]
        public string Path { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("type")]
        public ActionType Type { get; set; }
        [XmlAttribute("patch")]
        public string PatchName { get; set; }
        [XmlAttribute("newPath")]
        public string NewPath { get; set; }
        [XmlAttribute("newName")]
        public string NewName { get; set; }
        [XmlAttribute("error")]
        public string Error { get; set; }
    }
}