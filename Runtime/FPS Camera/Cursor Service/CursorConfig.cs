using UnityEngine;

namespace Larje.Character
{
    [CreateAssetMenu(fileName = "CursorConfig", menuName = "Larje/Character/Cursor Config")]
    public sealed class CursorConfig : ScriptableObject
    {
        [Header("Input")]
        [SerializeField] private string lookAction = "Player/Look";
        [SerializeField] private float sensitivity = 1f;

        [Header("Smoothing")]
        [SerializeField] private bool useSmooth = true;
        [SerializeField] private float rotationSharpness = 10f;
        [SerializeField] private float sensitivityDecreaseZone = 0.75f;

        [Header("Update")]
        [SerializeField] private CursorUpdateType updateType = CursorUpdateType.Normal;

        public string LookAction => lookAction;
        public float Sensitivity => sensitivity;
        public bool UseSmooth => useSmooth;
        public float RotationSharpness => rotationSharpness;
        public float SensitivityDecreaseZone => sensitivityDecreaseZone;
        public CursorUpdateType UpdateType => updateType;
    }
}
