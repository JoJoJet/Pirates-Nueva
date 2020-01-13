using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Pirates_Nueva
{
    public class VillageDef : Def<VillageDef>
    {
        private readonly Requirement[] requirements;

        #region Def Implementation
        protected override string TypeName => "VillageDef";
        protected sealed override ResourceInfo Resources => new ResourceInfo("villages", "VillageDefs");
        protected override VillageDef Construct(XmlReader reader) => new VillageDef(ref reader);
        #endregion

        public IReadOnlyList<Requirement> Requirements => this.requirements;

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

            //
            // Read the requirements from file.
            var reqs = new List<Requirement>();
            if(reader.ReadToDescendant(nameof(Requirement))) {
                do {
                    reqs.Add(new Requirement(reader));
                }
                while(reader.ReadToNextSibling(nameof(Requirement)));
            }
            //
            // Sort the requirements.
            var sorted = from r in reqs
                         //
                         // Order the requirements by the minimum number of required things,
                         // with lower numbers coming first, but 0 coming last.
                         orderby r.Min > 0 ? r.Min : int.MaxValue ascending
                         select r;
            this.requirements = sorted.ToArray();

            if(closeReader)
                reader.Dispose();
        }

        public sealed class Requirement
        {
            public BuildingDef Building { get; }
            public int Min { get; }
            public int Max { get; }

            internal Requirement(XmlReader reader)
            {
                Building = BuildingDef.Get(reader.GetAttributeStrict("ID"));
                
                var count = Regex.Match(reader.GetAttributeStrict("Count"), @"(?<min>\d+)\s*?-\w*?(?<max>\d+)");
                Min = parse("min");
                Max = parse("max");

                int parse(string name) => int.Parse(count.Groups[name].Value);
            }
        }
    }
}
