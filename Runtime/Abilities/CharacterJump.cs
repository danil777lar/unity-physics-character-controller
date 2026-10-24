using Larje.Core.Meshes;
using Larje.Character.AI;
using System;
using Larje.Character;
using Larje.Core.CompositeProperties;
using UnityEngine;

namespace Larje.Character.Abilities
{
    public class CharacterJump : CharacterAbility
    {
        [SerializeField] private float jumpForce = 4f;
        [SerializeField] private float jumpHeight = 4f;
        [SerializeField] private float horizontalSpeed = 2f;
        [SerializeField] private AnimationCurve jumpForceCurve = AnimationCurve.Constant(0f, 1f, 1f);
        [SerializeField] private int numberOfJumps = 1;

        private bool _isFirstJump;
        private bool _isJumping;
        private int _jumpsLeft;
        private float _traveledHeight;
        private Vector3 _jumpPoint;

        public bool IsInJump => _isJumping;
        public bool CanJump => _jumpsLeft > 0;
        public bool JumpInputValue { get; private set; }
        public PrioritizedProperty<JumpInput> Input { get; } = new PrioritizedProperty<JumpInput>();

        public event Action EventJump;

        public void JumpStart()
        {
            if (!Permitted) return;
            
            if (CanJump)
            {
                EventJump?.Invoke();

                _isFirstJump = _jumpsLeft == numberOfJumps;

                _jumpsLeft--;
                _isJumping = true;
                _jumpPoint = physics.transform.position;
                _traveledHeight = 0f;

                physics.AddForce(Vector3.up * -physics.Velocity.y, ForceMode.VelocityChange);
            }
        }

        public void JumpStop()
        {
            _isJumping = false;
            _traveledHeight = 0f;

            if (physics.Grounded)
            {
                ResetNumberOfJumps();
            }
        }

        public void ResetNumberOfJumps()
        {
            _jumpsLeft = numberOfJumps;
        }

        protected override void OnInitialized()
        {
            physics.useGravity.AddValue(() => !_isJumping);
            physics.EventGrounded += OnGrounded;
            physics.EventUngrounded += OnUngrounded;

            ResetNumberOfJumps();
        }

        private void Update()
        {
            if (Initialized)
            {
                HandleInput();
            }
        }

        private void FixedUpdate()
        {
            if (Initialized && Permitted)
            {
                if (_isJumping)
                {
                    float percent = _traveledHeight / jumpHeight;
                    _traveledHeight += jumpForce * jumpForceCurve.Evaluate(percent) * Time.fixedDeltaTime;

                    Vector3 targetPoint = GetTargetPoint();
                    Vector3 force = Vector3.up * (targetPoint.y - physics.transform.position.y) / Time.fixedDeltaTime;
                    force -= Vector3.up * physics.Velocity.y;

                    physics.AddForce(force, ForceMode.VelocityChange);

                    if (_traveledHeight >= jumpHeight)
                    {
                        JumpStop();
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (_isJumping)
            {
                Gizmos.color = Color.red;
                Mesh capsule =
                    MeshPrimitives.GenerateCapsuleMesh(physics.ColliderRadiusCurrent, physics.ColliderHeightCurrent);
                Gizmos.DrawWireMesh(capsule, GetTargetPoint() + physics.ColliderCenter, Quaternion.identity);
            }
        }

        private void HandleInput()
        {
            if (Input.TryGetValue(out JumpInput value))
            {
                if (value.Pressed())
                {
                    JumpInputValue = true;
                    JumpStart();
                }

                if (value.Released())
                {
                    JumpInputValue = false;
                }
            }
        }

        private void OnGrounded()
        {
            JumpStop();
        }

        private void OnUngrounded()
        {
            if (_jumpsLeft == numberOfJumps)
            {
                _jumpsLeft--;
            }
        }

        private Vector3 GetTargetPoint()
        {
            return _jumpPoint + Vector3.up * _traveledHeight;
        }

        public class JumpInput
        {
            public readonly Func<bool> Pressed;
            public readonly Func<bool> Released;

            public JumpInput(Func<bool> pressed, Func<bool> released)
            {
                Pressed = pressed;
                Released = released;
            }
        }
    }
}
