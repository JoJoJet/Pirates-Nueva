using MonoFont = Microsoft.Xna.Framework.Graphics.SpriteFont;
#nullable enable

namespace Pirates_Nueva.UI
{
    /// <summary>
    /// Wrapper for <see cref="MonoFont"/>.
    /// </summary>
    public class Font
    {
        /// <summary> The line spacing (the distance from baseline to baseline) of the font. </summary>
        public int LineSpacing => Mono.LineSpacing;
        /// <summary> The spacing (tracking) between characters in the font. </summary>
        public float Spacing => Mono.Spacing;

        private MonoFont Mono { get; }

        private Font(MonoFont mono) => Mono = mono;

        /// <summary>
        /// Returns the size of a string when rendered in this font.
        /// </summary>
        /// <param name="text">The text to measure.</param>
        /// <returns>The size, in pixels, of 'text' when rendered in this font.</returns>
        public PointF MeasureString(string text) => Mono.MeasureString(text);
        
        public static implicit operator Font(MonoFont mono) => new Font(mono);
        public static implicit operator MonoFont(Font font) => font.Mono;
    }
}
