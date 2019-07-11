namespace Pirates_Nueva.UI
{
    public class Text<T> : GUI.Element<T>
    {
        public string Value { get; }
        public Font Font { get; }

        public override int WidthPixels => (int)Font.MeasureString(Value).X;
        public override int HeightPixels => (int)Font.MeasureString(Value).Y;

        public Text(string value, Font font) {
            Value = value;
            Font = font;
        }

        protected override void Draw(ILocalDrawer<T> drawer, Master master)
            => drawer.DrawString(Font, Value, Left, Top, in Color.Black);
    }

    public class MutableText<T> : GUI.Element<T>
    {
        private string value;

        public string Value {
            get => this.value;
            set {
                if(this.value != value) {
                    this.value = value;
                    PropertyChanged();
                }
            }
        }

        public Font Font { get; }

        public override int WidthPixels => (int)Font.MeasureString(Value).X;
        public override int HeightPixels => (int)Font.MeasureString(Value).Y;

        public MutableText(string value, Font font) {
            this.value = value;
            Font = font;
        }

        protected override void Draw(ILocalDrawer<T> drawer, Master master)
            => drawer.DrawString(Font, Value, Left, Top, in Color.Black);
    }

    public class Button<T> : GUI.Element<T>, GUI.IButton
    {
        const int Padding = 3;

        private readonly GUI.OnClick onClick;

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
        protected override void Draw(ILocalDrawer<T> drawer, Master master) {
            var panel = new NineSlice(SliceDef.Get("panel"), WidthPixels, HeightPixels, master); // Make a panel.
            drawer.DrawCorner(panel, Left, Top, WidthPixels, HeightPixels);                      // Draw a panel behind the text.
                                                                                                 //
            drawer.DrawString(Font, Text, Left + Padding, Top + Padding, in Color.Black);        // Draw the text.
        }
    }
}
