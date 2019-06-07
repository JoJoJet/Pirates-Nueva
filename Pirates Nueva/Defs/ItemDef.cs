using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Pirates_Nueva
{
    public class ItemDef : Def<ItemDef>
    {
        public string TextureID { get; }

        protected override string ResourcePath => "items";

        protected override ItemDef Construct(XmlReader reader) => new ItemDef(reader);
        protected ItemDef(XmlReader reader) : base(reader) {
            using var r = reader.ReadSubtree();

            r.ReadToDescendant("TextureID");
            TextureID = r.ReadElementContentAsString();
        }
    }
}
