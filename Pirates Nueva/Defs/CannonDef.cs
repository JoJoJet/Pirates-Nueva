using System.Xml;
using Pirates_Nueva.Ocean;

namespace Pirates_Nueva
{
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
            FuelTypeID = reader.ReadElementTrim();
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
