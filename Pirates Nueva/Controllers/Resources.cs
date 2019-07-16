using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using ContentManager = Microsoft.Xna.Framework.Content.ContentManager;

namespace Pirates_Nueva
{
    /// <summary>
    /// An exception thrown by resource loading in <see cref="Pirates_Nueva"/>.
    /// </summary>
    public class ResourceException : Exception
    {
        public ResourceException(string message) : base(message) {  }
    }

    public static class Resources
    {
        private const string IndependentResourcesRoot = @"C:\Users\joe10\source\repos\Pirates-Nueva\Pirates Nueva\Resources\";

        private static readonly Dictionary<string, UI.Sprite> _sprites = new Dictionary<string, UI.Sprite>();

        private static bool isInitialized = false;

        private static ContentManager Content { get; set; }

        /// <summary>
        /// Performs one-time initialization of the <see cref="Resources"/> class.
        /// </summary>
        internal static void Initialize(ContentManager content) {
            const string Sig = nameof(Resources) + "." + nameof(Initialize) + "()";
            //
            // Throw an exception if this class has already been initialized.
            if(isInitialized) {
                throw new InvalidOperationException($"{Sig}(): This class has already been initialized!");
            }
            //
            // Store a reference to the content manager.
            Content = content;
            //
            // Set the class as having been initialized.
            isInitialized = true;
        }

        /// <summary> Get the <see cref="XmlReader"/> for the specified resources file. </summary>
        public static XmlReader GetXmlReader(string file) {
            ThrowIfUninitialized();
            return XmlReader.Create(Load(file + ".xml"), new XmlReaderSettings() { CloseInput = true });
        }

        /// <summary>
        /// Get the <see cref="UI.Sprite"/> with name /name/.
        /// </summary>
        /// <exception cref="ResourceException">Thrown if there is no sprite with name /name/.</exception>
        public static UI.Sprite LoadSprite(string name) {
            ThrowIfUninitialized();
            try {
                // Get the sprite named /name/ out of this instance's dictionary.
                // If there is no sprite with that name, load the sprite with that name from file.
                if(_sprites.TryGetValue(name, out var tex) == false) {
                    tex = new UI.Sprite(Content.Load<Texture2D>(name));
                    _sprites[name] = tex;
                }

                return tex;
            }
            catch(Microsoft.Xna.Framework.Content.ContentLoadException) {
                throw new ResourceException($"{nameof(Resources)}.{nameof(LoadSprite)}(): There is no sprite named \"{name}\"!");
            }
        }

        internal static FileReader Load(string file) {
            ThrowIfUninitialized();
            return new FileReader(IndependentResourcesRoot + file);
        }

        private static void ThrowIfUninitialized() {
            if(!isInitialized)
                throw new InvalidOperationException($"The {nameof(Resources)} class has not yet been initialized!");
        }
    }

    internal class FileReader : StreamReader {
        public FileReader(string path) : base(path) {  }
    }
}
