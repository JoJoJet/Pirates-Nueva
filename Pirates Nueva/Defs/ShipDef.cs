using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Pirates_Nueva
{
    /// <summary>
    /// An immutable object containing properties for a <see cref="Ocean.Ship"/>.
    /// </summary>
    public class ShipDef : Def<ShipDef>
    {
        public int Width { get; }
        public int Height { get; }

        public PointI RootIndex { get; }

        /// <summary>
        /// The name of the current type of <see cref="ShipDef"/>, for use in XML definitions.
        /// <para /> Should behave like a static property.
        /// </summary>
        protected override string TypeName => "ShipDef";
        /// <summary>
        /// Info about the <see cref="Pirates_Nueva.Resources"/> file that contains
        /// the definitions for <see cref="ShipDef"/>s.
        /// </summary>
        protected sealed override ResourceInfo Resources => new ResourceInfo("ships", "ShipDefs");

        /// <summary>
        /// Constructs a new instance of the current type of <see cref="ShipDef"/>,
        /// reading from the specified <see cref="XmlReader"/>.
        /// <para /> Should behave like a static method.
        /// </summary>
        protected override ShipDef Construct(XmlReader reader) => new ShipDef(ref reader);
        /// <summary>
        /// Reads the ID attribute of the <see cref="XmlReader"/> and consumes the Width and Height elements.
        /// </summary>
        /// <param name="closeReader">
        /// Whether or not the <see cref="XmlReader"/> should be closed after being read from.
        /// If this value is `false`, then the reader should be closed by this constructor's caller.
        /// </param>
        protected ShipDef(ref XmlReader reader, bool closeReader = true) : base(reader) {
            reader = reader.ReadSubtree();

            reader.ReadToDescendant("Width");
            Width = reader.ReadElementContentAsInt();

            reader.ReadToNextSibling("Height");
            Height = reader.ReadElementContentAsInt();

            reader.ReadToNextSibling("RootIndex");
            RootIndex = reader.ReadPointI();

            if(closeReader)
                reader.Dispose();
        }
    }
}
