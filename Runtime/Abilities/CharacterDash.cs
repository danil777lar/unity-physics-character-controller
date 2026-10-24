using Larje.Core.Meshes;
using Larje.Core.Extensions;
using Larje.Character.AI;
using System;
using System.Collections.Generic;
using Larje.Core.CompositeProperties;
using UnityEngine;

using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Larje.Character.Abilities
{
    public class CharacterDash : CharacterAbility
    {
        [SerializeField] private float inputDelay = 0.25f;
        [SerializeField] private float dashDistance = 5f;
        [SerializeField] private float maxDashDuration = 1f;
        [SerializeField] private float dashCooldown = 1f;

        [Header("Speed")] [SerializeField] private bool clampSpeedOnStop = false;
        [SerializeField] private float dashSpeed = 10f;
        [SerializeField] private AnimationCurve dashSpeedCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        private float _dashDuration;
        private float _dashCooldown;
        private float _passedDistance;
        private float _targetDistance;
        private float _maxSpeed;
        private Vector3 _dashStartPosition;
        private Vector3 _direction;
        private CharacterWalk _walk;

        public bool Dashing { get; private set; } = false;
        public PrioritizedProperty<DashInput> Input { get; } = new PrioritizedProperty<DashInput>();

        protected override void OnInitialized()
        {
            _walk = GetComponentInParent<CharacterWalk>();
            _walk.canWalk.AddValue(() => !Dashing);
        }

        private void FixedUpdate()
        {
            if (!Permitted) return;
            
            if (Initialized)
            {
                UpdateCooldown();
                HandleInput();
                UpdateDash();
            }
        }

        private void OnDrawGizmos()
        {
            if (Dashing)
            {
                Gizmos.color = Color.red;
                Mesh mesh = MeshPrimitives.GenerateCapsuleMesh(physics.ColliderRadiusCurrent,
                    physics.ColliderHeightCurrent);

                Vector3 point = _dashStartPosition + _direction * _targetDistance;
                point += physics.ColliderCenter;
                Gizmos.DrawWireMesh(mesh, point);
            }
        }

        private void UpdateCooldown()
        {
            if (!Dashing && _dashCooldown > 0f)
            {
                _dashCooldown -= Time.fixedDeltaTime;
            }
        }

        private void HandleInput()
        {
            if (Input.TryGetValue(out DashInput value))
            {
                if (_dashCooldown <= 0f)
                {
                    Vector2 input = value.GetValue(inputDelay);
                    if (input != Vector2.zero)
                    {
                        StartDash(input);
                    }
                }
            }
        }

        private void StartDash(Vector2 direction)
        {
            if (!Dashing)
            {
                Debug.Log("Start Dash");
                Dashing = true;

                _dashStartPosition = physics.transform.position;

                Vector2 dashDirection = _walk.ConvertDirection(direction).normalized;
                _direction = new Vector3(dashDirection.x, 0f, dashDirection.y);

                _targetDistance = dashDistance;
                if (physics.Capsulecast(_direction * dashDistance, out RaycastHit hit, 1f))
                {
                    _targetDistance = hit.distance - physics.ColliderRadiusCurrent * 2f;
                }
            }
        }

        private void UpdateDash()
        {
            if (Dashing)
            {
                _dashDuration += Time.fixedDeltaTime;
                _passedDistance += physics.HorizontalSpeed * Time.fixedDeltaTime;
                if (_passedDistance >= _targetDistance || _dashDuration >= maxDashDuration)
                {
                    StopDash();
                }
                else
                {
                    float t = _passedDistance / _targetDistance;
                    float speed = dashSpeedCurve.Evaluate(t) * dashSpeed;

                    Vector3 targetVelocity = _direction * speed;
                    Vector3 neededAccel = targetVelocity - physics.Velocity.XZ();

                    physics.AddForce(neededAccel, ForceMode.VelocityChange);
                }
            }
        }

        private void StopDash()
        {
            Dashing = false;
            _dashDuration = 0f;
            _passedDistance = 0f;
            _maxSpeed = 0f;
            _dashCooldown = dashCooldown;

            if (clampSpeedOnStop)
            {
                _walk.ClampCurrentSpeed();
            }
        }

        public class DashInput
        {
            private float _lastInputTime;
            private List<Record> _lastInputs = new List<Record>();

            public Vector2 GetValue(float maxDelay)
            {
                if (_lastInputs.Count > 1)
                {
                    if (_lastInputs[^1].value == _lastInputs[^2].value && _lastInputs[^1].delay < maxDelay)
                    {
                        Vector2 result = _lastInputs[^1].value;
                        _lastInputs.Clear();
                        return result;
                    }
                }

                return Vector2.zero;
            }

            public void AddValue(Vector2 value, bool force = false)
            {
                _lastInputs.Add(new Record()
                {
                    delay = Time.time - _lastInputTime,
                    value = value
                });
                _lastInputTime = Time.time;

                while (_lastInputs.Count > 10)
                {
                    _lastInputs.RemoveAt(0);
                }
            }

            private struct Record
            {
                public float delay;
                public Vector2 value;
            }
        }
    }
}