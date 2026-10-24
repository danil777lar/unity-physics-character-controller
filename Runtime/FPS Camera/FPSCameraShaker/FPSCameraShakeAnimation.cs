using Larje.Character.AI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Larje.Character
{
    [CreateAssetMenu(fileName = "FPSCameraShakeAnimation", menuName = "Larje/Character/FPSCameraShakeAnimation")]
    public class FPSCameraShakeAnimation : ScriptableObject
    {
        [field: SerializeField] public float Duration { get; private set; } = 1f;
        [field: Space]
        [field: SerializeField] public Shake XPosition { get; private set; }
        [field: SerializeField] public Shake YPosition { get; private set; }
        [field: SerializeField] public Shake ZPosition { get; private set; }
        [field: Space]
        [field: SerializeField] public Shake XRotation { get; private set; }
        [field: SerializeField] public Shake YRotation { get; private set; }
        [field: SerializeField] public Shake ZRotation { get; private set; }
        [field: Space]
        [field: SerializeField] public Shake Fov { get; private set; }

        [Serializable]
        public class Shake
        {
            [field: SerializeField] public float Multiplier { get; private set; } = 1f;
            [field: SerializeField] public AnimationCurve Curve { get; private set; } = AnimationCurve.Constant(0f, 1f, 0f);

            public float Evaluate(float t)
            {
                if (Curve == null)
                {
                    return 0f;
                }

                float value = Curve.Evaluate(t);
                return value * Multiplier;
            }
        }
    }
}
