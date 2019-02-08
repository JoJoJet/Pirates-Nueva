﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Xna.Framework.Graphics;

namespace Pirates_Nueva
{
    /// <summary>
    /// An immutable object containing properties for a <see cref="Block"/> of a <see cref="Ship"/>.
    /// </summary>
    public class BlockDef : Def
    {
        private string _textureId;

        /// <summary>
        /// The <see cref="Texture2D"/> to display onscreen for a <see cref="Block"/> with this <see cref="Def"/>.
        /// </summary>
        public Texture2D Texture => Master.Instance.Resources.LoadTexture(this._textureId);

        /// <summary>
        /// Get the <see cref="BlockDef"/> with identifier /id/.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if there is no <see cref="BlockDef"/> identified by /id/.</exception>
        /// <exception cref="InvalidCastException">Thrown if the <see cref="Def"/> identified by /id/ is not a <see cref="BlockDef"/>.</exception>
        public static BlockDef Get(string id) => Get<BlockDef>(id);

        protected override void ReadXml(XmlReader parentReader) {
            using(XmlReader reader = parentReader.ReadSubtree()) {
                reader.ReadToDescendant("TextureID");
                this._textureId = reader.ReadElementContentAsString();
            }
        }
    }
}
