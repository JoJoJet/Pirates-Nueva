﻿using System.Xml;
using Pirates_Nueva.Ocean;

namespace Pirates_Nueva
{
    /// <summary>
    /// An immutable object containing properties for <see cref="Furniture"/> on a <see cref="Ship"/>.
    /// </summary>
    public abstract class FurnitureDef : Def<FurnitureDef>
    {
        /// <summary>
        /// The name of the Texture to display onscreen for a <see cref="Furniture"/> with this <see cref="Def"/>.
        /// </summary>
        public string SpriteID { get; }
        /// <summary>
        /// The number of blocks that the <see cref="UI.Sprite"/> of a <see cref="Furniture"/> with this <see cref="Def"/> takes up.
        /// </summary>
        public PointI SpriteSize { get; }
        /// <summary>
        /// Where the <see cref="UI.Sprite"/> is being drawn from, local to the sprite itself. Range: [0, 1].
        /// </summary>
        public PointF SpriteOrigin { get; }

        /// <summary>
        /// Info about the <see cref="Pirates_Nueva.Resources"/> file that contains
        /// the definitions for <see cref="FurnitureDef"/>s.
        /// </summary>
        protected sealed override ResourceInfo Resources => new ResourceInfo("furniture", "FurnitureDefs");

        /// <summary>
        /// Reads the ID attribute of the <see cref="XmlReader"/>, and consumes
        /// the SpriteID, SpriteSize, and SpriteOrigin nodes.
        /// </summary>
        /// <param name="closeReader">
        /// Whether or not the <see cref="XmlReader"/> should be closed after being read from.
        /// If this value is `false`, then the reader should be closed by this constructor's caller.
        /// </param>
        protected FurnitureDef(ref XmlReader reader, bool closeReader = true) : base(reader) {
            reader = reader.ReadSubtree();

            reader.ReadToDescendant("SpriteID");
            SpriteID = reader.ReadElementTrim();

            reader.ReadToNextSibling("SpriteSize");
            SpriteSize = reader.ReadPointI();

            reader.ReadToNextSibling("SpriteOrigin");
            SpriteOrigin = reader.ReadPointF();

            if(closeReader)
                reader.Dispose();
        }

        /// <summary>
        /// Creates a new <see cref="Furniture"/> object using this <see cref="FurnitureDef"/>.
        /// </summary>
        public abstract Furniture Construct(Block floor, Dir direction);
    }
    /// <summary>
    /// Implements the abstract <see cref="FurnitureDef"/> class for a nonspecific type of <see cref="Furniture"/>.
    /// </summary>
    internal sealed class FurnitureDefImplementation : FurnitureDef
    {
        protected override string TypeName => "FurnitureDef";

        protected override FurnitureDef Construct(XmlReader reader) => new FurnitureDefImplementation(reader);
        private FurnitureDefImplementation(XmlReader reader) : base(ref reader) { }

        public override Furniture Construct(Block floor, Dir direction) => new Furniture(this, floor, direction);
    }
}
