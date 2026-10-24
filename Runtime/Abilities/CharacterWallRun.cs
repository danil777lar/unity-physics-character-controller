using Larje.Core.Extensions;
using Larje.Character.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Larje.Character.Abilities
{
    public class CharacterWallRun : CharacterAbility
    {
        [SerializeField] private float minSpeedToWallRun = 5f;
        [SerializeField] private float attachDistance = 0.2f;
        [SerializeField] private float maxTrajectoryAngle = 45f;
        [Space] [SerializeField] private float maxDistance = 20f;
        [SerializeField, Range(0f, 1f)] private float translateVelocityForwardOnJump;

        [Space] [SerializeField, Range(0f, 90f)]
        private float minWallRunAngle = 45f;

        [SerializeField, Range(0f, 90f)] private float maxWallRunAngle = 90f;

        [Header("Gizmo")] [SerializeField] private Color gizmoColor = Color.green.SetAlpha(0.5f);

        private float _wallRunSpeed;
        private float _traveledDistance;
        private Vector3 _wallRunDirection;
        private Vector3 _wallAttachNormal;
        private List<List<WallRunPoint>> _gizmoPath = new List<List<WallRunPoint>>();

        private CharacterJump _jump;
        private CharacterLedgeClimb _climb;

        public bool WallRunning { get; private set; }

        protected override void OnInitialized()
        {
            physics.useGravity.AddValue(() => !WallRunning);

            _climb = GetComponentInParent<CharacterLedgeClimb>();
            _climb.canClimb.AddValue(() => !WallRunning);

            _jump = GetComponentInParent<CharacterJump>();
            _jump.EventJump += OnJump;
        }

        private void FixedUpdate()
        {
            if (!Permitted) return;
            
            if (Initialized)
            {
                UpdateWallRunning();
                TryStartWallRun();
            }
        }

        private void OnDrawGizmos()
        {
            if (Initialized)
            {
                foreach (List<WallRunPoint> path in _gizmoPath)
                {
                    if (path.Count >= 2)
                    {
                        for (int i = 0; i < path.Count - 1; i++)
                        {
                            Gizmos.color = gizmoColor;
                            Gizmos.DrawLine(path[i].Position, path[i + 1].Position);
                        }

                        for (int i = 0; i < path.Count; i++)
                        {
                            Gizmos.color = gizmoColor;
                            Gizmos.DrawWireSphere(path[i].Position, 0.1f);
                            Gizmos.DrawLine(path[i].Position, path[i].Position + path[i].Normal);

                            Gizmos.color = Color.yellow;
                            Gizmos.DrawLine(path[i].Position, path[i].Position + path[i].Horizontal);

                            Gizmos.color = Color.magenta;
                            Gizmos.DrawLine(path[i].Position, path[i].Position + path[i].Speed);

                            Gizmos.color = Color.red;
                            Gizmos.DrawLine(path[i].Position, path[i].Position + path[i].OldSpeed);
                        }
                    }
                }
            }
        }

        private void TryStartWallRun()
        {
            if (!CanWallRun() || WallRunning)
            {
                return;
            }

            Vector3 vector = physics.Velocity * Time.fixedDeltaTime;
            if (physics.Capsulecast(vector, out RaycastHit hit, 1f, false, false))
            {
                Vector3 normal = -hit.normal;
                float angle = Vector3.Angle(normal, vector);
                if (angle > minWallRunAngle && angle < maxWallRunAngle)
                {
                    _traveledDistance = 0f;
                    _wallRunSpeed = physics.Velocity.magnitude;
                    _gizmoPath.Add(new List<WallRunPoint>());
                    if (_gizmoPath.Count > 2)
                    {
                        _gizmoPath.RemoveAt(0);
                    }

                    _jump?.ResetNumberOfJumps();
                    UpdateNormal(normal);
                    WallRunning = true;
                }
            }
        }

        private void UpdateWallRunning()
        {
            if (WallRunning)
            {
                _traveledDistance += physics.Velocity.magnitude * Time.fixedDeltaTime;
                if (!CanWallRun() || _traveledDistance >= maxDistance)
                {
                    StopWallRun();
                    return;
                }

                Vector3 goalVelocity = _wallRunDirection * _wallRunSpeed;
                Vector3 acceleration = (goalVelocity - physics.Velocity) * 5f;
                physics.AddForce(acceleration, ForceMode.Acceleration);

                Vector3 vector = _wallAttachNormal * (physics.ColliderRadiusCurrent + attachDistance);
                if (physics.Raycast(physics.transform.position, vector, out RaycastHit hit))
                {
                    Vector3 normal = -hit.normal;
                    if (_wallAttachNormal != normal)
                    {
                        UpdateNormal(normal);
                    }
                }
                else
                {
                    StopWallRun();
                    return;
                }
            }
        }

        private void OnJump()
        {
            if (WallRunning)
            {
                Vector3 forward = physics.transform.forward;
                float angle = Vector3.Angle(forward, _wallAttachNormal * -1f);
                if (angle > 90f)
                {
                    angle = 180f - angle;
                    forward = Quaternion.AngleAxis(angle, Vector3.up) * (_wallAttachNormal * -1f);
                }

                float t = translateVelocityForwardOnJump;
                Vector3 direction = Vector3.Lerp(physics.Velocity, forward, t);

                physics.TranslateVelocity(direction);
                StopWallRun();
            }
        }

        private void StopWallRun()
        {
            if (WallRunning)
            {
                _gizmoPath.Last().Add(new WallRunPoint() { Position = physics.transform.position });
                WallRunning = false;
            }
        }

        private bool CanWallRun()
        {
            return !physics.Grounded && physics.HorizontalSpeed > minSpeedToWallRun;
        }

        private void UpdateNormal(Vector3 normal)
        {
            _wallRunDirection = Vector3.ProjectOnPlane(physics.Velocity, normal).normalized;
            Vector3 oldSpeed = _wallRunDirection;

            Vector3 horizontal = _wallRunDirection;
            horizontal.y = 0f;
            horizontal.Normalize();

            float angle = Vector3.Angle(horizontal, _wallRunDirection);
            if (angle > maxTrajectoryAngle)
            {
                _wallRunDirection = Vector3.RotateTowards(horizontal, _wallRunDirection,
                    maxTrajectoryAngle * Mathf.Deg2Rad, 0f);
            }

            _wallAttachNormal = normal;

            physics.TranslateVelocity(_wallRunDirection);

            _gizmoPath.Last().Add(new WallRunPoint()
            {
                Position = physics.transform.position,
                Normal = normal,
                Horizontal = horizontal,
                Speed = _wallRunDirection,
                OldSpeed = oldSpeed,
            });
        }

        private struct WallRunPoint
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector3 Horizontal;
            public Vector3 Speed;
            public Vector3 OldSpeed;
        }
    }
}