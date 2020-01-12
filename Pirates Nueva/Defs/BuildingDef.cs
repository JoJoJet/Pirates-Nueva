using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Pirates_Nueva
{
    /// <summary>
    /// A Def for a <see cref="Ocean.Building"/> in a <see cref="Ocean.Village"/>.
    /// </summary>
    public class BuildingDef : Def<BuildingDef>
    {
        protected override string TypeName => "BuildingDef";
        protected sealed override ResourceInfo Resources => new ResourceInfo("buildings", "BuildingDef");

        protected override BuildingDef Construct(XmlReader reader) => new BuildingDef(ref reader);
        protected BuildingDef(ref XmlReader reader, bool closeReader = true) : base(reader)
        {
            reader = reader.ReadSubtree();

            if(closeReader)
                reader.Dispose();
        }
    }
}
