using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace Pirates_Nueva
{
    public class VillageDef : Def<VillageDef>
    {
        private readonly Requirement[] requirements;

        protected override string TypeName => "VillageDef";
        protected sealed override ResourceInfo Resources => new ResourceInfo("villages", "VillageDefs");

        protected override VillageDef Construct(XmlReader reader) => new VillageDef(ref reader);
        /// <summary>
        /// Reads the ID attribute of the <see cref="XmlReader"/>.
        /// </summary>
        /// <param name="closeReader">
        /// Whether or not the <see cref="XmlReader"/> should be closed after being read from.
        /// If this value is `false`, then the reader should be closed by this constructor's caller.
        /// </param>
        protected VillageDef(ref XmlReader reader, bool closeReader = true) : base(reader)
        {
            reader = reader.ReadSubtree();

            var reqs = new List<Requirement>();
            if(reader.ReadToDescendant(nameof(Requirement))) {
                do {
                    reqs.Add(new Requirement(reader));
                }
                while(reader.ReadToNextSibling(nameof(Requirement)));
            }
            this.requirements = reqs.ToArray();

            if(closeReader)
                reader.Dispose();
        }

        public sealed class Requirement
        {
            public BuildingDef Building { get; }
            public (int min, int max) Count { get; }

            internal Requirement(XmlReader reader)
            {
                using var r = reader.ReadSubtree();
                
                Building = BuildingDef.Get(reader.GetAttributeStrict("ID"));
                
                var count = Regex.Match(reader.GetAttributeStrict("Count"), @"(?<min>\d+)\w*?-\w*?(?<max>\d+)");
                Count = (parse("min"), parse("max"));

                int parse(string name) => int.Parse(count.Groups[name].Value);
            }
        }
    }
}
