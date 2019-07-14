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
        private readonly string solidId, trId, brId, blId, tlId;
        private UI.Texture? solid, tr, br, bl, tl;

        protected override string TypeName => "IslandBlockDef";
        protected override ResourceInfo Resources => new ResourceInfo("islandBlocks", "IslandBlockDefs");

        protected override IslandBlockDef Construct(XmlReader reader) => new IslandBlockDef(reader);
        IslandBlockDef(XmlReader parentReader) : base(parentReader) {
            using var reader = parentReader.ReadSubtree();

            reader.ReadToDescendant("TextureIDs");
            using var r = reader.ReadSubtree();

            r.ReadToDescendant("Solid");
            solidId = r.ReadElementTrim();

            r.ReadToNextSibling("TopRight");
            trId = r.ReadElementTrim();

            r.ReadToNextSibling("BottomRight");
            brId = r.ReadElementTrim();

            r.ReadToNextSibling("BottomLeft");
            blId = r.ReadElementTrim();

            r.ReadToNextSibling("TopLeft");
            tlId = r.ReadElementTrim();
        }

        public UI.Texture GetTexture(IslandBlockShape shape) => shape switch
        {
            IslandBlockShape.Solid       => this.solid ?? (this.solid = LoadTexture(this.solidId)),
            IslandBlockShape.TopRight    => this.tr    ?? (this.tr    = LoadTexture(this.trId)),
            IslandBlockShape.BottomRight => this.br    ?? (this.br    = LoadTexture(this.brId)),
            IslandBlockShape.BottomLeft  => this.bl    ?? (this.bl    = LoadTexture(this.blId)),
            IslandBlockShape.TopLeft     => this.tl    ?? (this.tl    = LoadTexture(this.tlId)),
            _ => throw new ArgumentException($"\"{shape}\" is an invalid enum value.")
        };
    }
}
