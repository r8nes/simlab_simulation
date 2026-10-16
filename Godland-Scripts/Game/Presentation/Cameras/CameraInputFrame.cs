using UnityEngine;

namespace Godland.Game.Presentation.Cameras
{
    public readonly struct CameraInputFrame
    {
        public readonly Vector2 KeyPan;
        public readonly Vector2 DragPan;
        public readonly float KeyRotation;
        public readonly Vector2 DragRotation;
        public readonly float Zoom;

        public CameraInputFrame(Vector2 keyPan, Vector2 dragPan, float keyRotation, Vector2 dragRotation, float zoom)
        {
            KeyPan = keyPan;
            DragPan = dragPan;
            KeyRotation = keyRotation;
            DragRotation = dragRotation;
            Zoom = zoom;
        }
    }
}
