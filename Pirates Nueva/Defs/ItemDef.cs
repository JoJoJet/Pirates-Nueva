using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Pirates_Nueva
{
    public class ItemDef : Def
    {
        private string? texId;

        public string TextureID => this.texId ?? ThrowNotInitialized<string>();

        /// <summary>
        /// Gets the <see cref="ItemDef"/> identified by the specified <see cref="string"/>.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if there is no <see cref="ItemDef"/> identified by /id/.</exception>
        /// <exception cref="InvalidCastException">Thrown if the <see cref="Def"/> identified by /id/ is not a <see cref="ItemDef"/>.</exception>
        public static ItemDef Get(string id) => Get<ItemDef>(id);

        protected override void ReadXml(XmlReader reader) {
            using(var r = reader.ReadSubtree()) {
                r.ReadToDescendant("TextureID");
                this.texId = r.ReadElementContentAsString();
            }
        }
    }
}
