using System;
using System.Collections.Generic;
using System.Xml;
using Pirates_Nueva.Ocean;

namespace Pirates_Nueva
{
    /// <summary>
    /// An immutable object containing properties for a <see cref="Block"/> of a <see cref="Ship"/>.
    /// </summary>
    public class BlockDef : Def<BlockDef>
    {
        /// <summary>
        /// The name of the Texture to display onscreen for a <see cref="Block"/> with this <see cref="BlockDef"/>.
        /// </summary>
        public string TextureID { get; }

        protected override string TypeName => "BlockDef";
        protected sealed override ResourceInfo Resources => new ResourceInfo("blocks", "BlockDefs");

        protected override BlockDef Construct(XmlReader reader) => new BlockDef(reader);
        protected BlockDef(XmlReader reader) : base(reader) {
            using var r = reader.ReadSubtree();

            r.ReadToDescendant("TextureID");
            TextureID = r.ReadElementTrim();
        }
    }
}
