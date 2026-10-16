using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Presentation.Cameras
{
    [CreateAssetMenu(menuName = "Godland/Camera Config", fileName = "CameraConfig")]
    public sealed class CameraConfig : ScriptableObject
    {
        [field: SerializeField, BoxGroup("Pan"), MinValue(0)]
        public float KeyPanSpeed { get; private set; } = 1.2f;

        [field: SerializeField, BoxGroup("Pan"), MinValue(0)]
        public float DragPanSensitivity { get; private set; } = 0.0015f;

        [field: SerializeField, BoxGroup("Zoom"), Range(0.01f, 0.5f)]
        public float ZoomStep { get; private set; } = 0.12f;

        [field: SerializeField, BoxGroup("Zoom"), MinMaxSlider(1f, 100f, true)]
        public Vector2 DistanceRange { get; private set; } = new(5f, 40f);

        [field: SerializeField, BoxGroup("Inspection"), MinValue(0.5f), InfoBox("Дистанция, на которую камера подлетает при осмотре тайла.")]
        public float InspectDistance { get; private set; } = 2.5f;

        [field: SerializeField, BoxGroup("Inspection"), MinValue(0.5f), InfoBox("Насколько близко можно приблизить колёсиком в режиме осмотра (обычный минимум — Distance Range).")]
        public float InspectMinDistance { get; private set; } = 1.2f;

        [field: SerializeField, BoxGroup("Rotation"), MinValue(0)]
        public float KeyRotationSpeed { get; private set; } = 90f;

        [field: SerializeField, BoxGroup("Rotation"), MinValue(0)]
        public float DragRotationSensitivity { get; private set; } = 0.2f;

        [field: SerializeField, BoxGroup("Rotation"), MinMaxSlider(5f, 89f, true)]
        public Vector2 PitchRange { get; private set; } = new(25f, 80f);

        [field: SerializeField, BoxGroup("Start")]
        public Vector3 StartFocus { get; private set; } = Vector3.zero;

        [field: SerializeField, BoxGroup("Start")]
        public float StartYaw { get; private set; } = 45f;

        [field: SerializeField, BoxGroup("Start")]
        public float StartPitch { get; private set; } = 55f;

        [field: SerializeField, BoxGroup("Start")]
        public float StartDistance { get; private set; } = 20f;

        [field: SerializeField, BoxGroup("Focus"), MinValue(0.05f), SuffixLabel("s")]
        public float DoubleClickInterval { get; private set; } = 0.3f;

        [field: SerializeField, BoxGroup("Smoothing"), MinValue(0)]
        public float Sharpness { get; private set; } = 12f;
    }
}
