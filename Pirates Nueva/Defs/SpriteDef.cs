using System.Xml;

namespace Pirates_Nueva
{
    /// <summary>
    /// An immutable object containing the defintion of a <see cref="UI.Sprite"/>.
    /// </summary>
    public sealed class SpriteDef : Def<SpriteDef>
    {
        /// <summary>
        /// The ID of this <see cref="SpriteDef"/>'s source texture.
        /// </summary>
        internal string SourceID { get; }

        protected override string TypeName => "SpriteDef";

        protected override ResourceInfo Resources => new ResourceInfo("sprites", "SpriteDefs");

        protected override SpriteDef Construct(XmlReader reader) => new SpriteDef(reader);
        private SpriteDef(XmlReader reader) : base(reader) {
            using var r = reader.ReadSubtree();

            r.ReadToDescendant("SourceID");
            SourceID = r.ReadElementTrim();
        }
    }
}
