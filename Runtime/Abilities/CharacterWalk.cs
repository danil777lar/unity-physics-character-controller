using Larje.Core.Extensions;
using Larje.Character.AI;
using System;
using Larje.Character;
using Larje.Core.CompositeProperties;
using UnityEngine;

namespace Larje.Character.Abilities
{
    public class CharacterWalk : CharacterAbility
    {
        [SerializeField] public BoolComposite canWalk;
        [SerializeField] public DirectionType directionType = DirectionType.World;

        [Header("Movement")] 
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private Vector3 forceScale = new Vector3(1f, 0f, 1f);

        [Header("Acceleration")] 
        [SerializeField] private Acceleration defaultAcceleration;
        [SerializeField] private Acceleration ungroundedAcceleration;

        [Space] 
        [SerializeField] private bool dropSpeedOnGrounded = true;
        [SerializeField, Range(-1f, 1f)] private float dropSpeedOnGroundedDot = 0f;

        private Vector3 _goalVelocity;
        private Camera _mainCamera;

        public float WalkSpeed => walkSpeed;
        public float SpeedPercent => Mathf.Clamp01(physics.HorizontalSpeed / (walkSpeed * WalkMultiplier.Value));
        public DirectionType MovementDirectionType => directionType;

        public PrioritizedProperty<Vector2> Input { get; } = new PrioritizedProperty<Vector2>();
        public FloatProduct WalkMultiplier { get; } = new FloatProduct();

        public Vector2 ConvertDirection(Vector2 input)
        {
            Vector3 right = Vector3.right;
            Vector3 forward = Vector3.forward;

            switch (directionType)
            {
                case DirectionType.Local:
                    right = transform.right;
                    forward = transform.forward;
                    break;

                case DirectionType.Camera:
                    right = _mainCamera.transform.right.XZ().normalized;
                    forward = _mainCamera.transform.forward.XZ().normalized;
                    break;
            }

            Vector3 direction = right * input.x + forward * input.y;
            Vector2 clampedDirection = new Vector2(direction.x, direction.z);
            if (clampedDirection.magnitude > 1f)
            {
                clampedDirection.Normalize();
            }
            return clampedDirection;
        }

        public void ClampCurrentSpeed()
        {
            float maxSpeed = walkSpeed * WalkMultiplier.Value;
            if (physics.HorizontalSpeed > maxSpeed)
            {
                Vector3 goalVelocity = physics.Velocity.XZ().normalized * maxSpeed;
                Vector3 neededAccel = (goalVelocity - physics.Velocity.XZ());
                physics.AddForce(neededAccel, ForceMode.VelocityChange);
            }
        }

        protected override void OnInitialized()
        {
            _mainCamera = Camera.main;
            physics.EventGrounded += OnGrounded;
        }

        private void FixedUpdate()
        {
            if (!Permitted) return;
            
            if (Initialized)
            {
                if (canWalk.Value)
                {
                    Acceleration acceleration = physics.Grounded ? defaultAcceleration : ungroundedAcceleration;
                    physics.AddSlopeTranslatedForce(GetAcceleration(acceleration));
                }
            }
        }

        private void OnGrounded()
        {
            if (dropSpeedOnGrounded)
            {
                float dot = Vector3.Dot(GetMovement(), _goalVelocity.normalized);
                if (GetInputValue() == Vector2.zero || dot <= dropSpeedOnGroundedDot)
                {
                    _goalVelocity = Vector3.zero;
                    physics.ResetVelocity();
                }
            }
        }

        private Vector3 GetAcceleration(Acceleration acceleration)
        {
            Vector3 movement = GetMovement();

            Vector3 currentVelocity = _goalVelocity.normalized;
            float currentVelocityDot = Vector3.Dot(movement, currentVelocity);
            float currentAcceleration = acceleration.Evaluate(currentVelocityDot) * WalkMultiplier.Value;
            float currentMaxAcceleration = acceleration.EvaluateMax(currentVelocityDot) * WalkMultiplier.Value;

            Vector3 goalVelocity = movement * (walkSpeed * WalkMultiplier.Value);
            _goalVelocity = Vector3.MoveTowards(_goalVelocity, goalVelocity, currentAcceleration * Time.fixedDeltaTime);

            Vector3 neededAccel = (_goalVelocity - physics.Velocity) / Time.fixedDeltaTime;
            neededAccel = Vector3.ClampMagnitude(neededAccel, acceleration.MaxValue * WalkMultiplier.Value);
            neededAccel = Vector3.Scale(neededAccel * physics.Mass, forceScale);

            return neededAccel;
        }

        private Vector3 GetMovement()
        {
            Vector2 processedInput = ConvertDirection(GetInputValue());
            Vector3 movement = new Vector3(processedInput.x, 0f, processedInput.y);
            if (movement.magnitude > 1f)
            {
                movement.Normalize();
            }

            return movement;
        }

        private Vector2 GetInputValue()
        {
            if (Input.TryGetValue(out Vector2 input))
            {
                return input;
            }

            return Vector2.zero;
        }

        public enum DirectionType
        {
            World,
            Local,
            Camera
        }

        [Serializable]
        public class Acceleration
        {
            [field: SerializeField] public float Value { get; private set; } = 20f;
            [field: SerializeField] public float MaxValue { get; private set; } = 15f;

            [field: SerializeField]
            public AnimationCurve Curve { get; private set; } = AnimationCurve.Linear(-1f, 1f, 1f, 1f);

            public float Evaluate(float t)
            {
                return Value * Curve.Evaluate(t);
            }

            public float EvaluateMax(float t)
            {
                return MaxValue * Curve.Evaluate(t);
            }
        }
    }
}
