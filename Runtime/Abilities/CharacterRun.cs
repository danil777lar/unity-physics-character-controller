using Larje.Character.AI;
using System;
using Larje.Character;
using Larje.Core.CompositeProperties;
using UnityEngine;

namespace Larje.Character.Abilities
{
    public class CharacterRun : CharacterAbility
    {
        [SerializeField] public BoolComposite canRun;

        [SerializeField] private float speedMultiplier = 2f;
        [SerializeField] private float lerpSpeedEnter = 10f;
        [SerializeField] private float lerpSpeedExit = 10f;

        private bool _inRunState = false;
        private float _speedMultiplierCurrent = 1f;

        private CharacterWalk _walk;

        public bool Running => _inRunState && canRun.Value;
        public PrioritizedProperty<bool> Input { get; } = new PrioritizedProperty<bool>();

        public event Action EventRunStart;
        public event Action EventRunStop;

        public void ResetMultiplier()
        {
            _speedMultiplierCurrent = 1f;
        }

        protected override void OnInitialized()
        {
            _walk = GetComponentInParent<CharacterWalk>();
            _walk.WalkMultiplier.AddValue(GetMultiplier);
        }

        private void Update()
        {
            if (!Permitted) return;
            
            if (Initialized)
            {
                InterpolateMultiplier();
                HandleInput();
            }
        }

        private void InterpolateMultiplier()
        {
            float targetMultiplier = Running ? speedMultiplier : 1f;
            float lerpSpeed = Running ? lerpSpeedEnter : lerpSpeedExit;

            _speedMultiplierCurrent = Mathf.Lerp(_speedMultiplierCurrent, targetMultiplier,
                Time.deltaTime * lerpSpeed);
        }

        private void HandleInput()
        {
            if (Input.TryGetValue(out bool value))
            {
                if (value)
                {
                    RunStart();
                }

                if (_inRunState && !value)
                {
                    RunStop();
                }
            }
        }

        public void RunStart()
        {
            if (!_inRunState)
            {
                _inRunState = true;
                EventRunStart?.Invoke();
            }
        }

        public void RunStop()
        {
            if (_inRunState)
            {
                _inRunState = false;
                EventRunStop?.Invoke();
            }
        }

        private float GetMultiplier()
        {
            return _speedMultiplierCurrent;
        }
    }
}