using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Pirates_Nueva
{
    public static class XmlExt
    {
        /// <summary>
        /// Gets the value of the attribute with specified name,
        /// or throws an exception if it's not defined.
        /// </summary>
        /// <exception cref="XmlException"/>
        public static string GetAttributeStrict(this XmlReader reader, string name) {
            const string Sig = nameof(XmlReader) + "." + nameof(GetAttributeStrict) + "()";
            if(reader.NodeType != XmlNodeType.Element)
                throw new XmlException($"{Sig}: The reader must be positioned on an element! " +
                                       $"Node type {reader.NodeType} is invalid.");
            if(reader.GetAttribute(name) is string att)
                return att;
            else
                throw new XmlException($"{Sig}: Element \"{reader.Name}\" does not have an attribute named \"{name}\"!");
        }

        /// <summary>
        /// Gets the value of the attribute with specified name,
        /// or returns null if it's not defined.
        /// </summary>
        public static string? GetAttributeOrNull(this XmlReader reader, string name)
            => reader.GetAttribute(name);

        /// <summary>
        /// Gets the value of the attribute with specified name,
        /// and converts it to an <see cref="int"/>.
        /// Throws an exception if the attribute is not defined.
        /// </summary>
        /// <exception cref="XmlException"/>
        /// <exception cref="FormatException"/>
        public static int GetAttributeInt(this XmlReader reader, string name) {
            const string Sig = nameof(XmlReader) + "." + nameof(GetAttributeInt) + "()";
            if(reader.NodeType != XmlNodeType.Element)
                throw new XmlException($"{Sig}: The reader must be positioned on an element! " +
                                       $"Node type \"{reader.NodeType}\" is invalid.");
            var att = reader.GetAttributeStrict(name);
            if(int.TryParse(att, out int val))
                return val;
            else
                throw new FormatException($"{Sig}: value \"{att}\" of attribute \"{name}\" on " +
                                          $"element \"{reader.Name}\" is not a valid integer value!");
        }
        /// <summary>
        /// Gets the value of the attribute with specified name,
        /// and converts it to an <see cref="int"/>.
        /// Returns <paramref name="default"/> if the attribute is not defined.
        /// </summary>
        /// <param name="default">The value to return if the attribute is not defined.</param>
        /// <exception cref="XmlException"/>
        /// <exception cref="FormatException"/>
        public static int GetAttributeInt(this XmlReader reader, string name, int @default) {
            const string Sig = nameof(XmlReader) + "." + nameof(GetAttributeInt) + "()";
            if(reader.NodeType != XmlNodeType.Element)
                throw new XmlException($"{Sig}: The reader must be positioned on an element! " +
                                       $"Node type \"{reader.NodeType}\" is invalid.");
            if(reader.GetAttributeOrNull(name) is string att) {
                if(int.TryParse(att, out int val))
                    return val;
                else
                    throw new FormatException($"{Sig}: value \"{att}\" of attribute \"{name}\" on " +
                                              $"element \"{reader.Name}\" is not a valid integer value!");
            }
            else {
                return @default;
            }
        }
    }
}
