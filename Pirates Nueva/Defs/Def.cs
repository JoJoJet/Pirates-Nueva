using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Pirates_Nueva
{
    /// <summary>
    /// An immutable object that contains properties for different types of objects.
    /// </summary>
    public abstract class Def
    {
        private static readonly Dictionary<string, Def> _defs = new Dictionary<string, Def>();

        /// <summary>
        /// The unique identifier of this <see cref="Def"/>. 
        /// </summary>
        public string ID { get; private set; }

        static Def() {
            // Get an XmlReader to load the Defs with.
            using(XmlReader reader = Master.Instance.Resources.GetXmlReader("defs")) {
                while(reader.Read()) {
                    switch(reader.Name) {
                        /*
                         * Reading a BlockDef.
                         */
                        case "BlockDef": {
                            readDef<BlockDef>(reader);
                            break;
                        }
                    }
                }
            }

            /*
             * Create a Def of type /T/ from an XmlReader position on the Def's parent node.
             */
            void readDef<T>(XmlReader reader) where T : Def, new() {
                T def = new T() { ID = reader.GetAttribute("ID") };
                def.ReadXml(reader);
                _defs[def.ID] = def;
            }
        }

        protected Def() { }

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

        protected abstract void ReadXml(XmlReader reader);
    }
}
