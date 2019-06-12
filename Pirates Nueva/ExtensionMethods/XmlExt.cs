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
        public static string GetAttributeStrict(this XmlReader reader, string name) {
            const string Sig = nameof(XmlReader) + "." + nameof(GetAttributeStrict) + "()";
            if(reader.NodeType != XmlNodeType.Element)
                throw new XmlException($"{Sig}: The reader must be positioned on an element!");
            if(reader.GetAttribute(name) is string att)
                return att;
            else
                throw new XmlException($"{Sig}: Element \"{reader.Name}\" does not have an attribute named \"{name}\"!");
        }
    }
}
