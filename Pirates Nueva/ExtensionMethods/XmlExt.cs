using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Runtime.CompilerServices;

namespace Pirates_Nueva
{
    public static class XmlExt
    {
        /// <summary>
        /// Reads the current element and returns the contents as a trimmed <see cref="string"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadElementTrim(this XmlReader reader)
            => reader.ReadElementContentAsString().Trim();
        /// <summary>
        /// Reads the current element and returns the contents as a <see cref="string"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadElementString(this XmlReader reader)
            => reader.ReadElementContentAsString();

        /// <summary>
        /// Gets the value of the attribute with specified name,
        /// or throws an exception if it's not defined.
        /// </summary>
        /// <exception cref="XmlException"/>
        public static string GetAttributeStrict(this XmlReader reader, string name) {
            const string Sig = nameof(XmlReader) + "." + nameof(GetAttributeStrict) + "()";
            ThrowIfInvalidNodeType(reader, XmlNodeType.Element, Sig);
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
            ThrowIfInvalidNodeType(reader, XmlNodeType.Element, Sig);
            //
            // Read the attribute and parse it as an integer.
            if(reader.GetAttributeOrNull(name) is string att) {
                if(int.TryParse(att, out int val))
                    return val;
                //
                // Throw an exception if the attribute can't be parsed as an integer.
                else
                    throw new FormatException($"{Sig}: value \"{att}\" of attribute \"{name}\" on " +
                                              $"element \"{reader.Name}\" is not a valid integer value!");
            }
            //
            // Throw an exception if the attribute is missing.
            else {
                throw new XmlException($"{Sig}: Element \"{reader.Name}\" does not have an attribute named \"{name}\"!");
            }
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
            ThrowIfInvalidNodeType(reader, XmlNodeType.Element, Sig);
            //
            // Read the attribute and parse it as an integer.
            if(reader.GetAttributeOrNull(name) is string att) {
                if(int.TryParse(att, out int val))
                    return val;
                //
                // Throw an exception if the attribute can't be parsed as an integer.
                else
                    throw new FormatException($"{Sig}: value \"{att}\" of attribute \"{name}\" on " +
                                              $"element \"{reader.Name}\" is not a valid integer value!");
            }
            //
            // Return the default value if the attribute is missing.
            else {
                return @default;
            }
        }

        /// <summary> Throws an exception if the reader is not positioned on the specified node type. </summary>
        private static void ThrowIfInvalidNodeType(XmlReader reader, XmlNodeType type, string callerSignature) {
            if(reader.NodeType != type)
                throw new XmlException($"{callerSignature}: The reader must be positioned on a node of " +
                                       $"type \"{type}\"! Node type \"{reader.NodeType}\" is invalid.");
        }
    }
}
