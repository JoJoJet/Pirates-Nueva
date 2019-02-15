using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Pirates_Nueva
{
    /// <summary>
    /// An immutable object containing the definition of a <see cref="UI.NineSlice"/> texture.
    /// </summary>
    public class SliceDef : Def
    {
        internal string Texture { get; private set; }
        internal (int left, int right, int top, int bottom) Slices { get; private set; }

        protected override void ReadXml(XmlReader parentReader) {
            using(var reader = parentReader.ReadSubtree()) {
                reader.ReadToDescendant("TextureID");
                Texture = reader.ReadElementContentAsString();

                reader.ReadToNextSibling("Slices");

                reader.ReadToDescendant("Left");
                int left = reader.ReadElementContentAsInt();

                reader.ReadToNextSibling("Right");
                int right = reader.ReadElementContentAsInt();

                reader.ReadToNextSibling("Top");
                int top = reader.ReadElementContentAsInt();

                reader.ReadToNextSibling("Bottom");
                int bottom = reader.ReadElementContentAsInt();

                this.Slices = (left, right, top, bottom);
            }
        }
    }
}
