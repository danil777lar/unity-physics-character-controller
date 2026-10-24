using Larje.Character.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

namespace Larje.Character
{
    public class FPSCameraShaker : MonoBehaviour
    {
        [SerializeField] private Transform cameraPositionTransform;
        [SerializeField] private Transform cameraRotationTransform;

        private float _defaultFov;
        private Vector3 _defaultPosition;
        private Vector3 _defaultRotation;
        private CinemachineCamera _cam;

        private List<Shake> _shakes = new List<Shake>();

        public void AddShake(Shake shake)
        {
            _shakes.Add(shake);
        }

        private void Start()
        {
            _defaultPosition = cameraPositionTransform.localPosition;
            _defaultRotation = cameraRotationTransform.localEulerAngles;

            _cam = GetComponentInChildren<CinemachineCamera>();
            _defaultFov = _cam.Lens.FieldOfView;
        }

        private void FixedUpdate()
        {
            ComputeShakes();
        }

        private void ComputeShakes()
        {
            Vector3 position = _defaultPosition;
            Vector3 rotation = _defaultRotation;
            float fov = _defaultFov;

            List<Shake> shakesToRemove = new List<Shake>();
            foreach (Shake shake in _shakes)
            {
                shake.PassedTime += Time.deltaTime * (shake.Speed?.Invoke() ?? 1f);
                if (shake.PassedTime >= shake.Animation.Duration && shake.Looped)
                {
                    shake.PassedTime = 0f;
                }

                if (shake.PassedTime >= shake.Animation.Duration)
                {
                    shakesToRemove.Add(shake);
                }
                else
                {
                    ApplyShake(shake, ref position, ref rotation, ref fov);
                }
            }

            cameraPositionTransform.localPosition = position;
            cameraRotationTransform.localEulerAngles = rotation;
            _cam.Lens.FieldOfView = fov;

            foreach (Shake shake in shakesToRemove)
            {
                _shakes.Remove(shake);
            }
        }

        private void ApplyShake(Shake shake, ref Vector3 position, ref Vector3 rotation, ref float fov)
        {
            float percent = shake.PassedTime / shake.Animation.Duration;
            float intensity = shake.Intensity?.Invoke() ?? 1f;

            position += new Vector3(shake.Animation.XPosition.Evaluate(percent), 
                            shake.Animation.YPosition.Evaluate(percent),
                            shake.Animation.ZPosition.Evaluate(percent)) *
                        intensity;

            rotation += new Vector3(shake.Animation.XRotation.Evaluate(percent),
                            shake.Animation.YRotation.Evaluate(percent),
                            shake.Animation.ZRotation.Evaluate(percent)) *
                        intensity;

            fov *= Mathf.Lerp(1f, shake.Animation.Fov.Evaluate(percent), intensity);
        }

        public class Shake
        {
            public float PassedTime;

            public readonly FPSCameraShakeAnimation Animation;
            public readonly bool Looped;
            public readonly Func<float> Speed;
            public readonly Func<float> Intensity;

            public Shake(FPSCameraShakeAnimation animation, bool looped = false, Func<float> speed = null, Func<float> intensity = null)
            {
                Animation = animation;
                Looped = looped;

                Speed = speed;
                Intensity = intensity;
            }
        }
    }
}
