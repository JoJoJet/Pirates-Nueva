using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Pirates_Nueva
{
    /// <summary>
    /// An immutable object containing properties for a <see cref="Furniture"/> of a <see cref="Ship"/>.
    /// </summary>
    public class FurnitureDef : Def
    {
        /// <summary>
        /// The name of the Texture to display onscreen for a <see cref="Furniture"/> with this <see cref="Def"/>.
        /// </summary>
        public string TextureID { get; protected set; }
        /// <summary>
        /// The number of blocks that the Texture of a <see cref="Furniture"/> with this <see cref="Def"/> takes up.
        /// </summary>
        public PointI TextureSize { get; protected set; }
        public PointF TextureOrigin { get; protected set; }

        /// <summary>
        /// Get the <see cref="FurnitureDef"/> with identifier /id/.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if there is no <see cref="FurnitureDef"/> identified by /id/.</exception>
        /// <exception cref="InvalidCastException">Thrown if the <see cref="Def"/> identified by /id/ is not a <see cref="FurnitureDef"/>.</exception>
        public static FurnitureDef Get(string id) => Get<FurnitureDef>(id);

        protected override void ReadXml(XmlReader parentReader) {
            using(XmlReader reader = parentReader.ReadSubtree()) {
                reader.ReadToDescendant("TextureID");
                TextureID = reader.ReadElementContentAsString();

                reader.ReadToNextSibling("TextureSize");
                TextureSize = reader.ReadPointI();

                reader.ReadToNextSibling("TextureOrigin");
                TextureOrigin = reader.ReadPointF();
            } 
        }
    }
}
