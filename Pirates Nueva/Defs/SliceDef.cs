﻿using System;
using System.Xml;
using Texture2D = Microsoft.Xna.Framework.Graphics.Texture2D;

namespace Pirates_Nueva
{
    /// <summary>
    /// An immutable object containing the definition of a <see cref="UI.NineSlice"/> sprite.
    /// </summary>
    public sealed class SliceDef : Def<SliceDef>
    {
        /// <summary>
        /// The identifier for the source <see cref="Texture2D"/> of a <see cref="UI.NineSlice"/> defined with this <see cref="Def"/>.
        /// </summary>
        internal string SpriteID { get; }
        internal (int left, int right, int top, int bottom) Slices { get; private set; }

        protected override string TypeName => "SliceDef";
        protected sealed override ResourceInfo Resources => new ResourceInfo("nineSlices", "SliceDefs");

        protected override SliceDef Construct(XmlReader reader) => new SliceDef(reader);
        private SliceDef(XmlReader parentReader) : base(parentReader) {
            using var r = parentReader.ReadSubtree();

            r.ReadToDescendant("SpriteID");
            SpriteID = r.ReadElementTrim();

            r.ReadToNextSibling("Slices");

            r.ReadToDescendant("Left");
            int left = r.ReadElementContentAsInt();

            r.ReadToNextSibling("Right");
            int right = r.ReadElementContentAsInt();

            r.ReadToNextSibling("Top");
            int top = r.ReadElementContentAsInt();

            r.ReadToNextSibling("Bottom");
            int bottom = r.ReadElementContentAsInt();

            Slices = (left, right, top, bottom);
        }
    }
}
