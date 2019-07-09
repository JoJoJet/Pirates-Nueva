namespace Pirates_Nueva.UI
{
    public class MutableText<T> : GUI.Element<T>
    {
        private string text;

        public string Text {
            get => this.text;
            set {
                if(this.text != value) {
                    this.text = value;
                    PropertyChanged();
                }
            }
        }

        public Font Font { get; }

        public override int WidthPixels => (int)Font.MeasureString(Text).X;
        public override int HeightPixels => (int)Font.MeasureString(Text).Y;

        public MutableText(string text, Font font) : base() {
            this.text = text;
            Font = font;
        }

        protected override void Draw(Master master, int left, int top)
            => master.Renderer.DrawString(Font, Text, left, top, in Color.Black);
    }

    public class Button<T> : GUI.Element<T>, GUI.IButton
    {
        const int Padding = 3;

        private GUI.OnClick onClick;

        /// <summary> Text to display on this Button. </summary>
        public string Text { get; }
        public Font Font { get; }

        public override int WidthPixels => (int)Font.MeasureString(Text).X + Padding*2;
        public override int HeightPixels => (int)Font.MeasureString(Text).Y + Padding*2;

        GUI.OnClick GUI.IButton.OnClick => this.onClick;

        public Button(string text, Font font, GUI.OnClick onClick) {
            Text = text;
            Font = font;
            this.onClick = onClick;
        }
        protected override void Draw(Master master, int left, int top) {
            var panel = new NineSlice(SliceDef.Get("panel"), WidthPixels, HeightPixels, master);  // Make a panel.
            master.Renderer.DrawCorner(panel, left, top, WidthPixels, HeightPixels);              // Draw a panel behind the text.

            master.Renderer.DrawString(Font, Text, left + Padding, top + Padding, in Color.Black); // Draw the text.
        }
    }
}
