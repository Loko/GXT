using Microsoft.Xna.Framework;

namespace GXT.Rendering
{
    /// <summary>
    /// "Iterator" for FrameSequence
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public interface gxtIFrameSequence
    {
        Rectangle CurrentFrameRect { get; }
        Vector2 CurrentFrameOrigin { get; }

        void Forwards();
        void Backwards();
        void ToStart();
        void ToEnd();
        void ToPosition(int index);
        int GetPosition();
        bool AtStart();
        bool AtEnd();
    }
}
