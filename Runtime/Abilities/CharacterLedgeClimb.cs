using Larje.Core.Meshes;
using Larje.Core.Extensions;
using Larje.Character.AI;
using System.Numerics;
using Larje.Character;
using Larje.Core.CompositeProperties;
using Larje.Core.Splines;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Larje.Character.Abilities
{
    public class CharacterLedgeClimb : CharacterAbility
    {
        [SerializeField] public BoolComposite canClimb;
        [Space] [SerializeField] private float height = 3f;
        [SerializeField] private float distance = 1f;
        [SerializeField] private float climbDetectStep = 0.1f;
        [SerializeField] private float distanceToStop = 0.1f;
        [SerializeField, Range(-1f, 1f)] private float minDotToAutoClimb = 0.5f;
        [Space] [SerializeField] private float minClimbSpeed = 5f;
        [SerializeField] private float climbSpeedMultiplier = 1f;

        private bool _climbing;
        private bool _canClimb;
        private float _startSpeed;
        private float _climbPathPercent;
        private Vector3 _climbStartPoint;
        private Vector3 _climbTargetPoint;
        private Spline _climbPath;
        private RaycastHit _hit;

        private CharacterJump _jump;

        protected override void OnInitialized()
        {
            _jump = GetComponentInParent<CharacterJump>();
            physics.useGravity.AddValue(() => !_climbing);
        }

        private void FixedUpdate()
        {
            if (!Permitted) return;
            
            if (Initialized)
            {
                if (canClimb.Value)
                {
                    TryDetectLedge();
                    TryStartClimb();
                    UpdateClimb();
                }
                else if (_climbing)
                {
                    StopClimb();
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (Initialized)
            {
                Color gizmoColor = Color.blue.SetAlpha(0.5f);
                Gizmos.color = gizmoColor;

                if (_hit.collider != null)
                {
                    Mesh c = MeshPrimitives.GenerateCapsuleMesh(physics.ColliderRadiusCurrent,
                        physics.ColliderHeightCurrent);
                    Vector3 origin = physics.transform.position + physics.ColliderCenter;
                    Vector3 offset = physics.transform.forward * (_hit.distance + physics.ColliderRadiusCurrent);
                    Gizmos.DrawWireMesh(c, origin + offset);

                    if (_canClimb)
                    {
                        Gizmos.DrawSphere(_climbTargetPoint, 0.25f);
                    }
                }

                if (_climbing && _climbPath != null)
                {
                    _climbPath.DrawLineGizmo(gizmoColor, 0.02f);
                    Gizmos.DrawSphere(_climbPath.Evaluate(_climbPathPercent), 0.1f);
                }
            }
        }

        private void TryDetectLedge()
        {
            if (!physics.Grounded && !_climbing)
            {
                Vector3 castVector = physics.transform.forward * distance;
                if (physics.Capsulecast(castVector, out RaycastHit hit, 1f, false, false))
                {
                    _hit = hit;

                    float distance = _hit.distance + physics.ColliderRadiusCurrent * 2f;
                    Vector3 point = physics.transform.position + physics.transform.forward * distance;

                    for (float heightShift = climbDetectStep; heightShift < height; heightShift += climbDetectStep)
                    {
                        Vector3 bottom = point;
                        bottom += physics.transform.up * physics.ColliderRadiusCurrent;
                        bottom += physics.transform.up * heightShift;

                        Vector3 top = bottom;
                        top += physics.transform.up *
                               (physics.ColliderHeightCurrent - physics.ColliderRadiusCurrent * 2f);

                        if (!Physics.CheckCapsule(bottom, top, physics.ColliderRadiusCurrent))
                        {
                            _canClimb = true;
                            _climbTargetPoint = point + physics.transform.up * (heightShift + 0.1f);
                            break;
                        }
                    }

                    return;
                }
            }

            _canClimb = false;
            _hit = default;
        }

        private void TryStartClimb()
        {
            if (_canClimb && !_climbing)
            {
                bool canAutoClimb = Vector3.Dot(physics.Velocity, physics.transform.forward) >= minDotToAutoClimb;
                if (_jump.JumpInputValue || canAutoClimb)
                {
                    _climbing = true;
                    physics.ResetVelocity();
                    _startSpeed = physics.Velocity.magnitude;
                    _climbStartPoint = physics.transform.position;
                    _climbPath = new CatmullRomSpline().SetPoints(new Vector3[]
                    {
                        _climbStartPoint,
                        _climbStartPoint.XZ() + _climbTargetPoint.Y() + Vector3.up * 0.1f,
                        _climbTargetPoint
                    });
                    _jump?.JumpStop();
                }
            }
        }

        private void UpdateClimb()
        {
            if (_climbing)
            {
                float climbSpeed = Mathf.Max(_startSpeed, minClimbSpeed) * climbSpeedMultiplier;
                _climbPathPercent += (climbSpeed * Time.fixedDeltaTime) / _climbPath.Length;

                Vector3 targetPoint = _climbPath.Evaluate(_climbPathPercent);
                physics.ResetVelocity();

                Vector3 force = (targetPoint - physics.transform.position) / Time.fixedDeltaTime;
                force -= physics.Velocity;
                physics.AddForce(force, ForceMode.VelocityChange);

                bool climbComplete = Vector3.Distance(physics.transform.position, _climbTargetPoint) < 0.1f;
                climbComplete |= (force.magnitude / climbSpeed) > 2f;
                if (climbComplete)
                {
                    StopClimb();
                }
            }
        }

        private void StopClimb()
        {
            _climbing = false;
            _canClimb = false;
            _climbPathPercent = 0f;
            _hit = default;
            physics.ResetVelocity();
        }
    }
}