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
        public string SourceID { get; }

        /// <summary> The amount of cropping done from the left edge of the texture. </summary>
        public int FromLeft { get; }
        /// <summary> The amount of cropping done from the bottom edge of the texture. </summary>
        public int FromBottom { get; }
        public int Width { get; }
        public int Height { get; }

        protected override string TypeName => "SpriteDef";

        protected override ResourceInfo Resources => new ResourceInfo("sprites", "SpriteDefs");

        protected override SpriteDef Construct(XmlReader reader) => new SpriteDef(reader);
        private SpriteDef(XmlReader parentReader) : base(parentReader) {
            using var reader = parentReader.ReadSubtree();

            reader.ReadToDescendant("SourceID");
            SourceID = reader.ReadElementTrim();

            reader.ReadToNextSibling("Cropping");
            using var r = reader.ReadSubtree();

            r.ReadToDescendant("LeftBottom");
            var (left, bottom) = r.ReadPointI();

            r.ReadToNextSibling("RightTop");
            var (right, top) = r.ReadPointI();

            (FromLeft, FromBottom) = (left, bottom);
            (Width, Height) = (right - left + 1, top - bottom + 1);
        }
    }
}
