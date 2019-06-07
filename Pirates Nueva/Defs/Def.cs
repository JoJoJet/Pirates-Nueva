using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;

namespace Pirates_Nueva
{
    public abstract class Def<T>
        where T : Def<T>
    {
        /// <summary>
        /// A structure containing info about the <see cref="Pirates_Nueva.Resources"/> file
        /// that contains the definitions for Defs of this type.
        /// </summary>
        protected readonly struct ResourceInfo
        {
            /// <summary> The local path to the resources file. </summary>
            public string FilePath { get; }
            /// <summary> The name of the root XML node in the resources file. </summary>
            public string RootNode { get; }

            public ResourceInfo(string filePath, string rootNode) {
                FilePath = filePath;
                RootNode = rootNode;
            }
        }

        private static readonly T dummy = GetDummy(typeof(T));

        private static readonly Dictionary<string, T> dict = new Dictionary<string, T>();

        /// <summary>
        /// Info about the <see cref="Pirates_Nueva.Resources"/> file that contains
        /// the definitions Defs of this type.
        /// </summary>
        protected abstract ResourceInfo Resources { get; }

        /// <summary>
        /// The identifier for this Def, unique among its type.
        /// </summary>
        public string ID { get; }

        static T GetDummy(Type t) => (T)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(t);

        static Def() {
            //
            // Find all loaded subclasses of this type of Def.
            var subclasses = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                             from type in assembly.GetTypes()
                             where type.IsSameOrSubclass(typeof(T))
                             where !type.IsAbstract
                             select type;
            var dummies = subclasses.ToDictionary( t =>  t.FullName.Substring(t.Namespace.Length + 1),
                                                   t => new Lazy<T>(() => GetDummy(t)) );
            //
            // Open the resources file for Defs of this type.
            var resources = GetDummy(typeof(T)).Resources;
            using var reader = Pirates_Nueva.Resources.GetXmlReader("defs\\" + resources.FilePath);
            if(reader.ReadToDescendant(resources.RootNode)) {
                //
                // Read until the file ends.
                while(reader.Read()) {
                    //
                    // If the current node is not an element, skip it.
                    if(reader.NodeType != XmlNodeType.Element) {
                        continue;
                    }
                    //
                    // Create a new Def of the specified type
                    // by calling .Construct() on a dummy instance.
                    if(dummies.TryGetValue(reader.Name, out var dummy)) {
                        var def = dummy.Value.Construct(reader);
                        dict[def.ID] = def;
                    }
                    //
                    // Throw an exception if there is no class with that name.
                    else {
                        throw new XmlException($"There is no {typeof(T).FullName} class named \"{reader.Name}\"!");
                    }
                }
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
