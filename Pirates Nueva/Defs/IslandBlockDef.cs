using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Pirates_Nueva.Ocean;

namespace Pirates_Nueva
{
    /// <summary>
    /// An immutable object containing properties for an <see cref="IslandBlock"/>.
    /// </summary>
    public sealed class IslandBlockDef : Def<IslandBlockDef>
    {
        private UI.Texture? texture;

        /// <summary>
        /// The name of the Texture to display onscreen for an <see cref="IslandBlock"/> with this Def.
        /// </summary>
        public string TextureID { get; }
        public UI.Texture Texture => this.texture ?? (this.texture = Pirates_Nueva.Resources.LoadTexture(TextureID));

        protected override string TypeName => "IslandBlockDef";
        protected override ResourceInfo Resources => new ResourceInfo("islandBlocks", "IslandBlockDefs");

        protected override IslandBlockDef Construct(XmlReader reader) => new IslandBlockDef(reader);
        IslandBlockDef(XmlReader reader) : base(reader) {
            using var r = reader.ReadSubtree();

            r.ReadToDescendant("TextureID");
            TextureID = r.ReadElementTrim();
        }
    }
}
