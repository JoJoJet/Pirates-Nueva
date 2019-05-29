using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Pirates_Nueva.Ocean;

namespace Pirates_Nueva
{
    /// <summary>
    /// An immutable object containing properties for a <see cref="Furniture"/> of a <see cref="Ship"/>.
    /// </summary>
    public class FurnitureDef : Def
    {
        private System.Reflection.ConstructorInfo ctor;

        public Type Type { get; protected set; }
        /// <summary>
        /// The name of the Texture to display onscreen for a <see cref="Furniture"/> with this <see cref="Def"/>.
        /// </summary>
        public string TextureID { get; protected set; }
        /// <summary>
        /// The number of blocks that the Texture of a <see cref="Furniture"/> with this <see cref="Def"/> takes up.
        /// </summary>
        public PointI TextureSize { get; protected set; }
        /// <summary>
        /// Where the Texture is being drawn from, local to the texture itself. Range: [0, 1].
        /// </summary>
        public PointF TextureOrigin { get; protected set; }

        /// <summary>
        /// Get the <see cref="FurnitureDef"/> with identifier /id/.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if there is no <see cref="FurnitureDef"/> identified by /id/.</exception>
        /// <exception cref="InvalidCastException">Thrown if the <see cref="Def"/> identified by /id/ is not a <see cref="FurnitureDef"/>.</exception>
        public static FurnitureDef Get(string id) => Get<FurnitureDef>(id);

        /// <summary>
        /// Creates a <see cref="Furniture"/> using this <see cref="Def"/>.
        /// </summary>
        public Furniture Construct(Block floor, Dir direction)
            => (Furniture)this.ctor.Invoke(new object[] { this, floor, direction });

        //
        // Parameters for a furniture constructor.
        private static readonly Type[] ctorParams = { typeof(FurnitureDef), typeof(Block), typeof(Dir) };
        protected override void ReadXml(XmlReader parentReader) {
            using(XmlReader reader = parentReader.ReadSubtree()) {
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
}
