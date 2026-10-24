using Larje.Character;
using Larje.Character.Abilities;
using Larje.Character.AI;
using UnityEngine;

namespace Larje.Character.AI
{
    public class AIActionJump : AIAction
    {
        [SerializeField] private int inputPriority = 100;

        private bool _pressed = false;
        private bool _released = false;
        private CharacterJump _characterJump;
        private CharacterJump.JumpInput _input; 

        public override void PerformAction()
        {
        }

        protected override void OnInitialized()
        {
            _characterJump = Brain.Owner.GetComponentInChildren<CharacterJump>();
        }

        protected override void OnEnterState()
        {
            _characterJump.Input.AddValue(GetInput, () => inputPriority);
            _pressed = true;
        }

        protected override void OnExitState()
        {
            _characterJump.Input.RemoveValue(GetInput);
        }

        private void Update()
        {
            UpdateInput();
        }

        private void UpdateInput()
        {
            if (_pressed && _characterJump.IsInJump)
            {
                _pressed = false;
            }
        }

        private CharacterJump.JumpInput GetInput()
        {
            if (_input == null)
            {
                _input = new CharacterJump.JumpInput(() => _pressed, () => _released);
            }
            return _input;
        }
    }
}
