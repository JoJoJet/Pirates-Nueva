using System.Xml;

namespace Pirates_Nueva
{
    public class VillageDef : Def<VillageDef>
    {
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

            if(closeReader)
                reader.Dispose();
        }
    }
}
