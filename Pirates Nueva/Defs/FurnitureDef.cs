using System;
using System.Collections.Generic;
using System.Xml;
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
        public string TextureID { get; }
        /// <summary>
        /// The number of blocks that the Texture of a <see cref="Furniture"/> with this <see cref="Def"/> takes up.
        /// </summary>
        public PointI TextureSize { get; }
        /// <summary>
        /// Where the Texture is being drawn from, local to the texture itself. Range: [0, 1].
        /// </summary>
        public PointF TextureOrigin { get; }

        /// <summary>
        /// Info about the <see cref="Pirates_Nueva.Resources"/> that contains
        /// the definitions for <see cref="FurnitureDef"/>s.
        /// </summary>
        protected sealed override ResourceInfo Resources => new ResourceInfo("furniture", "FurnitureDefs");

        /// <summary>
        /// Reads the ID attribute of the <see cref="XmlReader"/>, and consumes
        /// the TextureID, TextureSize, and TextureOrigin nodes.
        /// </summary>
        /// <param name="closeReader">
        /// Whether or not the <see cref="XmlReader"/> should be closed after being read from.
        /// If this value is `false`, then the reader should be closed by this constructor's caller.
        /// </param>
        protected FurnitureDef(ref XmlReader reader, bool closeReader = true) : base(reader) {
            reader = reader.ReadSubtree();

            reader.ReadToDescendant("TextureID");
            TextureID = reader.ReadElementContentAsString();

            reader.ReadToNextSibling("TextureSize");
            TextureSize = reader.ReadPointI();

            reader.ReadToNextSibling("TextureOrigin");
            TextureOrigin = reader.ReadPointF();

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

    /// <summary>
    /// An immutable object containing properties for a <see cref="Cannon"/> on a <see cref="Ship"/>.
    /// </summary>
    public class CannonDef : FurnitureDef
    {
        /// <summary>
        /// The ID of the <see cref="ItemDef"/> that fuels a
        /// <see cref="Cannon"/> using this <see cref="CannonDef"/>.
        /// </summary>
        public string FuelTypeID { get; }
        /// <summary>
        /// The name for the current type of <see cref="CannonDef"/>, for use in XML definitions.
        /// <para /> Should behave like a static property.
        /// </summary>
        protected override string TypeName => "CannonDef";

        /// <summary>
        /// Constructs a new instance of the current type of <see cref="CannonDef"/>,
        /// reading from the specified <see cref="XmlReader"/>.
        /// <para /> Should behave like a static method.
        /// </summary>
        protected override FurnitureDef Construct(XmlReader reader) => new CannonDef(ref reader);
        /// <summary>
        /// Reads the ID attribute, and consumes the TextureID, TextureSize, TextureOrigin, and FuelTypeID nodes.
        /// </summary>
        /// <param name="closeReader">
        /// Whether or not the <see cref="XmlReader"/> should be closed after being read from.
        /// If this value is `false`, then the reader should be closed by this constructor's caller.
        /// </param>
        protected CannonDef(ref XmlReader reader, bool closeReader = true) : base(ref reader, closeReader: false) {
            reader.ReadToNextSibling("FuelTypeID");
            FuelTypeID = reader.ReadElementContentAsString();
            //
            // Close the reader if we're instructed to.
            if(closeReader)
                reader.Dispose();
        }

        /// <summary>
        /// Creates a new <see cref="Cannon"/> object using this <see cref="CannonDef"/>.
        /// </summary>
        public override Furniture Construct(Block floor, Dir direction) => new Cannon(this, floor, direction);
    }
}
