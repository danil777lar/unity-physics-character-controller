using Larje.Core.Extensions;
using Larje.Character;
using System;
using System.Collections.Generic;
using Larje.Character.AI;
using UnityEngine;
using UnityEngine.AI;

namespace Larje.Character.AI
{
    public class AIActionSetRandomTargetPositionInRadius : AIAction
    {
        [Header("Delay")]
        [SerializeField] private float delayMin = 1f;
        [SerializeField] private float delayMax = 10f;
        [Header("Radius")]
        [SerializeField] private bool attachToInitialPosition = false;
        [SerializeField] private float radius = 10f;

        private float _seed;
        private float _delay;
        private Vector3 _initialPosition;
        private Vector3 _targetPosition;
        private Transform _target;

        public override void PerformAction()
        {
        }

        protected override void OnInitialized()
        {
            _initialPosition = Brain.Owner.transform.position;
            _seed = UnityEngine.Random.Range(-10000f, 10000f);

            if (_target == null)
            {
                _target = new UnityEngine.GameObject("Walk Target").transform;
                _target.SetParent(transform);
            }
        }

        protected override void OnEnterState()
        {
            Brain.Target = _target;
        }

        protected override void OnExitState()
        {
        }

        private void Update()
        {
            if (ActionInProgress)
            {
                if (_delay <= 0f)
                {
                    CalculatePoint();
                    _delay = Mathf.Lerp(delayMin, delayMax, Mathf.PerlinNoise(Time.time + _seed, 0f));
                }
                else
                {
                    _delay -= Time.deltaTime;
                }

                if (NavMesh.SamplePosition(_targetPosition, out NavMeshHit hit, 1000f, NavMesh.AllAreas))
                {
                    _target.position = hit.position;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (_target == null)
            {
                return;
            }

            Gizmos.color = Color.green.SetAlpha(ActionInProgress ? 1f : 0.2f);
            Gizmos.DrawSphere(_target.position, 0.5f);
        }

        private void CalculatePoint()
        {
            _targetPosition = attachToInitialPosition ? _initialPosition : transform.position;

            float angle = UnityEngine.Random.Range(0f, 360f);
            float distance = UnityEngine.Random.Range(0f, radius);
            _targetPosition += Quaternion.Euler(0f, angle, 0f) * Vector3.forward * distance;
        }
    }
}
