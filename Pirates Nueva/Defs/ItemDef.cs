﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Pirates_Nueva
{
    public class ItemDef : Def
    {
        public string TextureID { get; }

        /// <summary>
        /// Gets the <see cref="ItemDef"/> identified by the specified <see cref="string"/>.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if there is no <see cref="ItemDef"/> identified by /id/.</exception>
        /// <exception cref="InvalidCastException">Thrown if the <see cref="Def"/> identified by /id/ is not a <see cref="ItemDef"/>.</exception>
        public static ItemDef Get(string id) => Get<ItemDef>(id);

        protected override Def Construct(XmlReader reader) => new ItemDef(reader);
        protected ItemDef(XmlReader reader) : base(reader) {
            using var r = reader.ReadSubtree();

            r.ReadToDescendant("TextureID");
            TextureID = r.ReadElementContentAsString();
        }
    }
}
