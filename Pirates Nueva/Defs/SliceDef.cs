﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Texture2D = Microsoft.Xna.Framework.Graphics.Texture2D;

namespace Pirates_Nueva
{
    /// <summary>
    /// An immutable object containing the definition of a <see cref="UI.NineSlice"/> texture.
    /// </summary>
    public class SliceDef : Def
    {
        /// <summary>
        /// The identifier for the source <see cref="Texture2D"/> of a <see cref="UI.NineSlice"/> defined with this <see cref="Def"/>.
        /// </summary>
        internal string TextureID { get; private set; }
        internal (int left, int right, int top, int bottom) Slices { get; private set; }

        protected override void ReadXml(XmlReader parentReader) {
            using(var reader = parentReader.ReadSubtree()) {
                reader.ReadToDescendant("TextureID");
                TextureID = reader.ReadElementContentAsString();

                reader.ReadToNextSibling("Slices");

                reader.ReadToDescendant("Left");
                int left = reader.ReadElementContentAsInt();

                reader.ReadToNextSibling("Right");
                int right = reader.ReadElementContentAsInt();

                reader.ReadToNextSibling("Top");
                int top = reader.ReadElementContentAsInt();

                reader.ReadToNextSibling("Bottom");
                int bottom = reader.ReadElementContentAsInt();

                this.Slices = (left, right, top, bottom);
            }
        }
    }
}