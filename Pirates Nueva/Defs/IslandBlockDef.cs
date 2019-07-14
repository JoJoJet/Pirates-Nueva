using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Pirates_Nueva.Ocean;
using static Pirates_Nueva.Resources;

namespace Pirates_Nueva
{
    /// <summary>
    /// An immutable object containing properties for an <see cref="IslandBlock"/>.
    /// </summary>
    public sealed class IslandBlockDef : Def<IslandBlockDef>
    {
        private readonly string[] ids = new string[5];
        private readonly UI.Texture?[] texes = new UI.Texture?[5];

        protected override string TypeName => "IslandBlockDef";
        protected override ResourceInfo Resources => new ResourceInfo("islandBlocks", "IslandBlockDefs");

        protected override IslandBlockDef Construct(XmlReader reader) => new IslandBlockDef(reader);
        IslandBlockDef(XmlReader parentReader) : base(parentReader) {
            using var reader = parentReader.ReadSubtree();

            reader.ReadToDescendant("TextureIDs");
            using var r = reader.ReadSubtree();

            r.ReadToDescendant("Solid");
            this.ids[0] = r.ReadElementTrim();

            r.ReadToNextSibling("TopRight");
            this.ids[1] = r.ReadElementTrim();

            r.ReadToNextSibling("BottomRight");
            this.ids[2] = r.ReadElementTrim();

            r.ReadToNextSibling("BottomLeft");
            this.ids[3] = r.ReadElementTrim();

            r.ReadToNextSibling("TopLeft");
            this.ids[4] = r.ReadElementTrim();
        }

        public UI.Texture GetTexture(IslandBlockShape shape)
            => this.texes[(int)shape] ?? (this.texes[(int)shape] = LoadTexture(this.ids[(int)shape]));
    }
}
