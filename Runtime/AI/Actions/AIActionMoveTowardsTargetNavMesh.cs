using Larje.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using Larje.Character;
using Larje.Character.Abilities;
using Larje.Character.AI;
using UnityEngine;
using UnityEngine.AI;

namespace Larje.Character.AI
{
    public class AIActionMoveTowardsTargetNavMesh : AIAction
    {
        [Space]
        [SerializeField] private DirectionType directionType = DirectionType.World;
        [SerializeField] private AnimationCurve localMovementSpeedCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        [Space]
        [SerializeField] private float pathShiftDistance = 0f;
        [Space] 
        [SerializeField] private float minimumDistance = 1f;
        [SerializeField] private float minimumDistanceToCorner = 1f;
        [Space] 
        [SerializeField] private bool drawDebug;

        private Transform _moveTarget;
        private NavMeshPath _path;
        private CharacterWalk _characterWalk;
        private CharacterOrientation _characterOrientation;

        protected override void OnInitialized()
        {
            _path = new NavMeshPath();
            _characterWalk = Brain.Owner.GetComponentInChildren<CharacterWalk>();
            _characterOrientation = Brain.Owner.GetComponentInChildren<CharacterOrientation>();

            if (_moveTarget == null)
            {
                _moveTarget = new GameObject().transform;
                _moveTarget.gameObject.name = "AIActionMoveTowardsTargetNavMesh: Move Target";
                _moveTarget.SetParent(transform);
            }
        }

        protected override void OnEnterState()
        {
            _characterWalk.Input.AddValue(GetMovement, () => 10);
            if (_characterOrientation)
            {
                _characterOrientation.BodyTarget.InputDirection.AddValue(GetDirectionToTarget, GetOrientationInputPriority); 
            }
        }

        protected override void OnExitState()
        {
            _characterWalk.Input.RemoveValue(GetMovement);
            if (_characterOrientation)
            {
                _characterOrientation.BodyTarget.InputDirection.RemoveValue(GetDirectionToTarget);
            }
        }

        public override void PerformAction()
        {
            SetTargetPosition();
        }

        protected virtual void SetTargetPosition()
        {
            if (Brain.Target == null)
            {
                return;
            }

            Vector3 sourcePosition = Brain.Owner.transform.position; 
            Vector3 targetPosition = Brain.Target.position;

            if (NavMesh.CalculatePath(sourcePosition, targetPosition, NavMesh.AllAreas, _path))
            {
                foreach (Vector3 corner in _path.GetShiftedCorners(pathShiftDistance))
                {
                    targetPosition = corner;
                    if (Vector3.Distance(targetPosition, Brain.Owner.transform.position) >= minimumDistanceToCorner)
                    {
                        break;
                    }
                }
            }

            _moveTarget.position = targetPosition;
        }

        protected Vector2 GetMovement()
        {
            Vector2 movement = directionType == DirectionType.World ? GetMovementWorld() : GetMovementLocal();
            if (Vector3.Distance(Brain.Owner.transform.position, _moveTarget.position) < minimumDistance)
            {
                movement = Vector2.zero;
            }

            return movement;
        }

        private Vector2 GetMovementLocal()
        {
            Vector2 movement = Vector2.zero;

            if (_characterOrientation)
            {
                Vector3 targetDirection = (_moveTarget.position - Brain.Owner.transform.position).normalized;
                Vector3 currentDirection = _characterOrientation.BodyTarget.Transform.forward.normalized;
                movement.x = currentDirection.x;
                movement.y = currentDirection.z;

                float angle = Vector3.Angle(targetDirection, currentDirection);
                float anglePercent = 1f - Mathf.Clamp01(angle / 180f);
                movement *= localMovementSpeedCurve.Evaluate(anglePercent);
            }

            return movement;
        }

        private Vector2 GetMovementWorld()
        {
            Vector2 movement = Vector2.zero; 
            Vector3 direction = _moveTarget.position - Brain.Owner.transform.position;
            movement.x = direction.x;
            movement.y = direction.z;

            return movement;
        }

        private Vector3 GetDirectionToTarget()
        {
            return _moveTarget.position - Brain.Owner.transform.position;
        }

        private int GetOrientationInputPriority()
        {
            return directionType == DirectionType.World ? -10 : 1;
        }

        private void OnDrawGizmos()
        {
            if (_path != null && ActionInProgress)
            {
                DrawPathGizmo(_path.corners.ToList(), Color.red);
                DrawPathGizmo(_path.GetShiftedCorners(pathShiftDistance), Color.blue);
            }
        }

        private void DrawPathGizmo(List<Vector3> points, Color color)
        {
            if (points.Count > 0)
            {
                Gizmos.color = color;
                Gizmos.DrawLine(transform.position, points[0]);
                for (int i = 0; i < points.Count - 1; i++)
                {
                    Gizmos.DrawSphere(points[i], 0.25f);
                    Gizmos.DrawLine(points[i], points[i + 1]);
                }
                Gizmos.DrawSphere(points[^1], 0.25f);
            }
        }

        public enum DirectionType
        {
            World,
            Local
        }
    }
}
