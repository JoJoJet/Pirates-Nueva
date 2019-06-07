using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace Pirates_Nueva
{
    /// <summary>
    /// An exception thrown by resource loading in <see cref="Pirates_Nueva"/>.
    /// </summary>
    public class ResourceException : Exception
    {
        public ResourceException(string message) : base(message) {  }
    }

    internal class Resources
    {
        private const string IndependentResourcesRoot = @"C:\Users\joe10\source\repos\Pirates-Nueva\Pirates Nueva\Resources\";

        private readonly Dictionary<string, UI.Texture> _textures = new Dictionary<string, UI.Texture>();

        public Master Master { get; }

        private Microsoft.Xna.Framework.Content.ContentManager Content => Master.Content;

        public Resources(Master master) {
            Master = master;
        }

        /// <summary> Get the <see cref="XmlReader"/> for the specified resources file. </summary>
        public XmlReader GetXmlReader(string file) => XmlReader.Create(Load(file + ".xml"), new XmlReaderSettings() { CloseInput = true });

        /// <summary>
        /// Get the <see cref="UI.Texture"/> with name /name/.
        /// </summary>
        /// <exception cref="ResourceException">Thrown if there is no texture with name /name/.</exception>
        public UI.Texture LoadTexture(string name) {
            try {
                // Get the texture named /name/ out of this instance's dictionary.
                // If there is no texture with that name, load the texture with that name from file.
                if(this._textures.TryGetValue(name, out var tex) == false) {
                    tex = new UI.Texture(Content.Load<Texture2D>(name));
                    this._textures[name] = tex;
                }

                return tex;
            }
            catch(Microsoft.Xna.Framework.Content.ContentLoadException) {
                throw new ResourceException($"{nameof(Resources)}.{nameof(LoadTexture)}(): There is no texture named \"{name}\"!");
            }
        }

        public FileReader Load(string file) => new FileReader(IndependentResourcesRoot + file);
    }

    internal class FileReader : StreamReader {
        public FileReader(string path) : base(path) {  }
    }
}
