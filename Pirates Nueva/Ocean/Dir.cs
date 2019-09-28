using Ang = Pirates_Nueva.Angle;

namespace Pirates_Nueva.Ocean
{
    /// <summary>
    /// A nautical direction.
    /// </summary>
    public enum Dir { Aport, Forward, Astarboard, Astern };

    public static class DirExt
    {
        /// <summary> Gets the <see cref="Ang"/> corresponding to this nautical direction. </summary>
        public static Ang Angle(this Dir dir) => dir switch
        {
            Dir.Aport      => Ang.Up,
            Dir.Forward    => Ang.Right,
            Dir.Astarboard => Ang.Down,
            _              => Ang.Left
        };
    }
}
