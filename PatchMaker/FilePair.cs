using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace com.taleoftwowastelands.patchmaker
{
    public class FilePair
    {
        [XmlAttribute("originalName")]
        public string OriginalName { get; set; }
        [XmlAttribute("resultName")]
        public string ResultName { get; set; }
        [XmlAttribute("type")]
        public ContentType Type { get; set; }
        [XmlArray("Actions")]
        public List<RecordAction> Actions { get; set; }
        [XmlAttribute("patch")]
        public string PatchName { get; set; }

        public FilePair()
        {
            Actions = new List<RecordAction>();
        }

        public bool ShouldSerializeActions()
        {
            return Actions != null && Actions.Count > 0;
        }
    }
}
