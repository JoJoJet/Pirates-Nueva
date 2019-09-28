using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using ship = Pirates_Nueva.Ocean.Ship;

namespace Pirates_Nueva
{
    /// <summary>
    /// An immutable object containing properties for a <see cref="Ocean.Ship"/>.
    /// </summary>
    public class ShipDef : Def<ShipDef>
    {
        /// <summary>
        /// Info about a ship <see cref="Ocean.Block"/> for a <see cref="ShipDef"/>'s default shape.
        /// </summary>
        public class BlockInfo
        {
            public string ID { get; }
            public int X { get; }
            public int Y { get; }

            internal BlockInfo(XmlReader reader) {
                ID = reader.GetAttributeStrict("ID");
                X = reader.GetAttributeInt("X");
                Y = reader.GetAttributeInt("Y");
            }
        }

        /// <summary>
        /// The length of a <see cref="ship"/> using this <see cref="ShipDef"/>, from stern to bow.
        /// </summary>
        public int Length { get; }
        /// <summary>
        /// The width of a <see cref="ship"/> using this <see cref="ShipDef"/>, from port to starboard.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// The index of the root block on a <see cref="ship"/> using this <see cref="ShipDef"/>.
        /// </summary>
        public PointI RootIndex { get; }

        /// <summary>
        /// The speed of a <see cref="ship"/> using this <see cref="ShipDef"/>, in units per second.
        /// </summary>
        public float Speed { get; }
        /// <summary>
        /// The turning radius of a <see cref="ship"/> using this <see cref="ShipDef"/>.
        /// </summary>
        public float TurnRadius { get; }
        /// <summary>
        /// The maximum turning speed of a <see cref="ship"/> using this <see cref="ShipDef"/>, in radians per second.
        /// </summary>
        public Angle TurnSpeed { get; }

        public IReadOnlyList<BlockInfo> DefaultShape { get; }

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

            reader.ReadToDescendant("Length");
            Length = reader.ReadElementContentAsInt();

            reader.ReadToNextSibling("Width");
            Width = reader.ReadElementContentAsInt();

            reader.ReadToNextSibling("RootIndex");
            RootIndex = reader.ReadPointI();

            reader.ReadToNextSibling("Speed");
            Speed = reader.ReadElementContentAsFloat();

            reader.ReadToNextSibling("TurningRadius");
            {
                TurnRadius = reader.ReadElementContentAsFloat();
                TurnSpeed = Angle.FromRadians(Speed / TurnRadius);
            }

            reader.ReadToNextSibling("DefaultShape");
            using(var r = reader.ReadSubtree()) {
                var shape = new List<BlockInfo>();

                if(r.ReadToDescendant("Block")) {
                    do {
                        shape.Add(new BlockInfo(r));
                    }
                    while(r.ReadToNextSibling("Block"));
                }

                DefaultShape = shape.AsReadOnly();
            }

            if(closeReader)
                reader.Dispose();
        }
    }
}
