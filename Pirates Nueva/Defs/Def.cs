using System;
using System.Collections.Generic;
using System.Xml;

namespace Pirates_Nueva
{
    /// <summary>
    /// An immutable object that contains properties that defines a type of object.
    /// </summary>
    public abstract class Def
    {
        private static class DummyHolder<T>
            where T : Def
        {
            public static readonly T dummy = (T)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(T));
        }

        // An object to lock onto when initializing this class.
        private static readonly object padlock = new object();

        private static readonly Dictionary<string, Def> _defs = new Dictionary<string, Def>();
        private static bool isInitialized = false;

        /// <summary>
        /// The unique identifier of this <see cref="Def"/>. 
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// Initialize the <see cref="Def"/> class. Can only be called once.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this method is called more than once per program instance.</exception>
        internal static void Initialize(Master master) {
            lock(padlock) {
                // Throw an exception if this method has been called before.
                if(isInitialized)
                    throw new InvalidOperationException($"{nameof(Def)}.{nameof(Initialize)}(): {nameof(Def)} has already been initialized!");

                // Get an XmlReader to load the Defs with.
                using(XmlReader reader = Resources.GetXmlReader("defs")) {
                    while(reader.Read()) {
                        switch(reader.Name) {
                            /*
                             * Reading a BlockDef.
                             */
                            case "BlockDef":
                                readDef<BlockDef>(reader);
                                break;
                            case "FurnitureDef":
                                readDef<FurnitureDef>(reader);
                                break;
                            case "ItemDef":
                                readDef<ItemDef>(reader);
                                break;
                            case "SliceDef":
                                readDef<SliceDef>(reader);
                                break;
                        }
                    }
                }

                // Mark this class as having been Initialized.
                isInitialized = true;
            }

            /*
             * Create a Def of type /T/ from an XmlReader position on the Def's parent node.
             */
            void readDef<T>(XmlReader reader) where T : Def {
                var def = DummyHolder<T>.dummy.Construct(reader);
                _defs[def.ID] = def;
            }
        }

        /// <summary>
        /// Reads the ID attribute of the <see cref="XmlReader"/>. Does not consume any elements.
        /// </summary>
        protected Def(XmlReader reader) => ID = reader.GetAttribute("ID");

        /// <summary>
        /// Get the <see cref="Def"/> with identifier /id/ and of type /T/.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if there is no <see cref="Def"/> identified by /id/.</exception>
        /// <exception cref="InvalidCastException">Thrown if the <see cref="Def"/> identified by /id/ is not castable to /T/.</exception>
        public static T Get<T>(string id) where T : Def {
            if(_defs.TryGetValue(id, out Def def)) {
                if(def is T tdef) {
                    return tdef;
                }
                else {
                    throw new InvalidCastException(
                        $"{nameof(Def)}.{nameof(Get)}(): The {nameof(Def)} identified by \"{id}\" is not of, " +
                        $"nor does it descend from, Type \"{typeof(T).Name}\"!"
                        );
                }
            }
            else {
                throw new KeyNotFoundException($"There is no {nameof(Def)} identified by the string \"{id}\"!");
            }
        }

        /// <summary>
        /// Constructs a new instance of the current type of <see cref="Def"/>,
        /// reading from the specified <see cref="XmlReader"/>. <para />
        /// Should behave like a static method.
        /// </summary>
        protected abstract Def Construct(XmlReader reader);
    }
}
