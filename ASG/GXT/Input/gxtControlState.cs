namespace GXT.Input
{
    /// <summary>
    /// A state of a game control (typically a button)
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public enum gxtControlState
    {
        /// <summary>
        /// The control is released, and has been released for at least two frames.
        /// </summary>
        UP = 0,
        /// <summary>
        /// The control was released during the last frame.
        /// </summary>
        FIRST_RELEASED = 1,
        /// <summary>
        /// The control is pressed, and has bee pressed for at least two frames.
        /// </summary>
        DOWN = 2,
        /// <summary>
        /// The control was pressed during the last frame.
        /// </summary>
        FIRST_PRESSED = 3
    };
}
