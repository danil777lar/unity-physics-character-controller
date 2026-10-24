using Larje.Character.AI;
using System;
using System.Runtime.InteropServices;
using Larje.Core.CompositeProperties;
using UnityEngine;

namespace Larje.Character.Abilities
{
    public class CharacterCrouch : CharacterAbility
    {
        [SerializeField] private float speedModifier = 0.5f;
        [SerializeField] private float lerpSpeedEnter = 10f;
        [SerializeField] private float lerpSpeedExit = 10f;

        [Header("Collider")] 
        [SerializeField] private bool resizeCollider = true;
        [SerializeField] private float colliderHeightMultiplier = 0.4f;

        [Header("Slide")] 
        [SerializeField] private bool slideOnCrouch = false;
        [SerializeField] private float slideSpeedMultiplier = 2f;
        [SerializeField] private bool multiplyDistanceOnMultiplier = false;
        [SerializeField] private float slideDistance = 5f;

        private float _speedMultiplierCurrent;
        private float _slideDistance;

        private CharacterWalk _walk;
        private CharacterRun _run;

        public bool Crouching { get; private set; } = false;
        public bool Sliding { get; private set; } = false;
        public PrioritizedProperty<bool> Input { get; } = new PrioritizedProperty<bool>();

        protected override void OnInitialized()
        {
            _walk = GetComponentInParent<CharacterWalk>();
            _run = GetComponentInParent<CharacterRun>();

            physics.ColliderHeightMultiplier.AddValue(GetColliderHeightMultiplier);
            _walk.WalkMultiplier.AddValue(GetWalkSpeedMultiplier);

            _walk.canWalk.AddValue(() => !Sliding);
            _run.canRun.AddValue(() => !Sliding);
        }

        private void Update()
        {
            if (!Permitted) return;
            
            if (Initialized)
            {
                HandleInput();
            }
        }

        private void FixedUpdate()
        {
            if (!Permitted) return;
            
            InterpolateMultiplier();
            TryExitSlide();

            if (Crouching && !Sliding && _run.Running)
            {
                CrouchStop();
            }
        }

        private void HandleInput()
        {
            if (Input.TryGetValue(out bool input))
            {
                if (input)
                {
                    if (Crouching)
                    {
                        CrouchStop();
                    }
                    else
                    {
                        CrouchStart();
                    }
                }
            }
        }

        private void InterpolateMultiplier()
        {
            if (!Sliding)
            {
                float targetMultiplier = Crouching ? speedModifier : 1f;
                float lerpSpeed = Crouching ? lerpSpeedEnter : lerpSpeedExit;

                _speedMultiplierCurrent = Mathf.Lerp(_speedMultiplierCurrent, targetMultiplier,
                    Time.deltaTime * lerpSpeed);
            }
        }

        private void CrouchStart()
        {
            Crouching = true;
            TryEnterSlide();
        }

        private void CrouchStop()
        {
            if (CanStand())
            {
                Crouching = false;
                Sliding = false;
                _slideDistance = 0f;
                _speedMultiplierCurrent = Mathf.Clamp01(_speedMultiplierCurrent);
            }
        }

        private void TryEnterSlide()
        {
            if (physics.Velocity.magnitude > _walk.WalkSpeed)
            {
                Sliding = slideOnCrouch;
                _speedMultiplierCurrent = physics.Velocity.magnitude / _walk.WalkSpeed;
                _speedMultiplierCurrent *= slideSpeedMultiplier;

                _slideDistance = slideDistance;
                if (multiplyDistanceOnMultiplier)
                {
                    _slideDistance *= _speedMultiplierCurrent;
                }

                _run?.ResetMultiplier();
            }
        }

        private void TryExitSlide()
        {
            if (Crouching && Sliding)
            {
                _slideDistance -= physics.Velocity.magnitude * Time.deltaTime;
                if (physics.Velocity.magnitude <= _walk.WalkSpeed * speedModifier || _slideDistance <= 0)
                {
                    Sliding = false;
                    _slideDistance = 0f;
                }
            }
        }

        private float GetWalkSpeedMultiplier()
        {
            return Crouching ? speedModifier : 1f;
        }

        private float GetColliderHeightMultiplier()
        {
            return (Crouching && resizeCollider) ? colliderHeightMultiplier : 1f;
        }

        private bool CanStand()
        {
            float distance = physics.ColliderHeightDefault - physics.ColliderHeightCurrent;
            bool floored = physics.Capsulecast(transform.up * distance, out RaycastHit hit, 0.9f);

            return !floored;
        }
    }
}