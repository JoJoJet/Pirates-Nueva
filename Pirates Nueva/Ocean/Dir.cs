using Ang = Pirates_Nueva.Angle;

namespace Pirates_Nueva.Ocean
{
    /// <summary>
    /// A relative direction.
    /// </summary>
    public enum Dir { Up, Right, Down, Left };

    public static class DirExt
    {
        /// <summary> Gets the <see cref="Ang"/> corresponding to this relative direction. </summary>
        public static Ang Angle(this Dir dir) => dir switch
        {
            Dir.Up    => Ang.Up,
            Dir.Right => Ang.Right,
            Dir.Down  => Ang.Down,
            _         => Ang.Left
        };
    }
}
