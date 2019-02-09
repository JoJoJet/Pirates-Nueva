using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Pirates_Nueva
{
    /// <summary>
    /// An immutable object containing properties for a <see cref="Block"/> of a <see cref="Ship"/>.
    /// </summary>
    public class BlockDef : Def
    {
        /// <summary>
        /// The name of the Texture to display onscreen for a <see cref="Block"/> with this <see cref="BlockDef"/>.
        /// </summary>
        public string TextureID { get; protected set; }

        /// <summary>
        /// Get the <see cref="BlockDef"/> with identifier /id/.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if there is no <see cref="BlockDef"/> identified by /id/.</exception>
        /// <exception cref="InvalidCastException">Thrown if the <see cref="Def"/> identified by /id/ is not a <see cref="BlockDef"/>.</exception>
        public static BlockDef Get(string id) => Get<BlockDef>(id);

        protected override void ReadXml(XmlReader parentReader) {
            using(XmlReader reader = parentReader.ReadSubtree()) {
                reader.ReadToDescendant("TextureID");
                TextureID = reader.ReadElementContentAsString();
            }
        }
    }
}
