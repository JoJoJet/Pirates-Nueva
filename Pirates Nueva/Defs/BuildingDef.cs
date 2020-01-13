using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace Pirates_Nueva
{
    /// <summary>
    /// A Def for a <see cref="Ocean.Building"/> in a <see cref="Ocean.Village"/>.
    /// </summary>
    public class BuildingDef : Def<BuildingDef>
    {
        #region Def Implementation
        protected override string TypeName => "BuildingDef";
        protected sealed override ResourceInfo Resources => new ResourceInfo("buildings", "BuildingDef");
        protected override BuildingDef Construct(XmlReader reader) => new BuildingDef(ref reader);
        #endregion

        public int Width { get; }
        public int Height { get; }

        protected BuildingDef(ref XmlReader reader, bool closeReader = true) : base(reader)
        {
            reader = reader.ReadSubtree();

            var size = Regex.Match(reader.GetAttributeStrict("Size"), @"(?<width>\d+),?\s*(?<height>\d+)");

            Width = parseSize("width");
            Height = parseSize("height");

            if(closeReader)
                reader.Dispose();

            int parseSize(string name) => int.Parse(size.Groups[name].Value);
        }
    }
}
