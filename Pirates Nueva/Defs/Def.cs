using System;
using System.Collections.Generic;
using System.Xml;

namespace Pirates_Nueva
{
    public abstract class Def<T>
        where T : Def<T>
    {
        private static readonly T dummy = (T)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(T));

        private static readonly Dictionary<string, T> dict = new Dictionary<string, T>();

        /// <summary>
        /// The local path to the resource file containing the definitions for the Defs of this type.
        /// Should behave like a static property.
        /// </summary>
        protected abstract string ResourcePath { get; }

        /// <summary>
        /// The identifier for this Def, unique among its type.
        /// </summary>
        public string ID { get; }

        static Def() {
            using var reader = Resources.GetXmlReader("defs\\" + dummy.ResourcePath);
            if(reader.ReadToDescendant("Def")) {
                do {
                    var def = dummy.Construct(reader);
                    dict[def.ID] = def;
                }
                while(reader.ReadToNextSibling("Def"));
            }
        }
        /// <summary>
        /// Reads the ID attribute of the <see cref="XmlReader"/>. Does not consume any elements.
        /// </summary>
        protected Def(XmlReader reader) => ID = reader.GetAttribute("ID");

        /// <summary>
        /// Gets the Def identified by the specified <see cref="string"/>.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown when there is no Def ID'd by the specified string.</exception>
        public static T Get(string id) => dict.TryGetValue(id, out var def)
                                          ? def
                                          : throw new KeyNotFoundException($"There is no {typeof(T).Name} " +
                                                                           $"identifed by the string \"{id}\"!");

        /// <summary>
        /// Constructs a new instance of the current type of Def,
        /// reading from the specified <see cref="XmlReader"/>.
        /// <para /> Should behave like a static method.
        /// </summary>
        protected abstract T Construct(XmlReader reader);
    }
}
