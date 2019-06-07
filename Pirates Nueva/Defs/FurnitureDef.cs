using System;
using System.Collections.Generic;
using System.Xml;
using Pirates_Nueva.Ocean;
using CtorInfo = System.Reflection.ConstructorInfo;

namespace Pirates_Nueva
{
    /// <summary>
    /// An immutable object containing properties for a <see cref="Furniture"/> of a <see cref="Ship"/>.
    /// </summary>
    public class FurnitureDef : Def<FurnitureDef>
    {
        private readonly CtorInfo ctor;

        public Type Type { get; }
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

        protected override string ResourcePath => "furniture";

        /// <summary>
        /// Creates a <see cref="Furniture"/> using this <see cref="Def"/>.
        /// </summary>
        public Furniture Construct(Block floor, Dir direction)
            => (Furniture)this.ctor.Invoke(new object[] { this, floor, direction });

        protected override FurnitureDef Construct(XmlReader reader) => new FurnitureDef(reader);
        //
        // Method signature for a furniture constructor.
        private static readonly Type[] ctorParams = { typeof(FurnitureDef), typeof(Block), typeof(Dir) };
        protected FurnitureDef(XmlReader parentReader) : base(parentReader) {
            using var reader = parentReader.ReadSubtree();
            //
            // Read the type of the furniture def.
            reader.ReadToDescendant("ClassName");
            var className = reader.ReadElementContentAsString();
            var type = Type.GetType(className);
            if(!type.IsSameOrSubclass(typeof(Furniture)))
                throw new InvalidCastException($"Class {type.FullName} does not descend from {typeof(Furniture).FullName}!");
            this.ctor = type.GetConstructor(ctorParams)
                ?? throw new MissingMethodException($"Class {type.FullName} must have a constructor with " +
                                                     "the following parameters: (FurnitureDef, Block, Dir)!");
            Type = type;

            reader.ReadToNextSibling("TextureID");
            TextureID = reader.ReadElementContentAsString();

            reader.ReadToNextSibling("TextureSize");
            TextureSize = reader.ReadPointI();

            reader.ReadToNextSibling("TextureOrigin");
            TextureOrigin = reader.ReadPointF();
        }
    }
}
