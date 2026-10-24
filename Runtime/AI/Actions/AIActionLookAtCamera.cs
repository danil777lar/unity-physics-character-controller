using Larje.Character.Abilities;
using Larje.Core.Extensions;
using Larje.Character;
using Larje.Character.AI;
using UnityEngine;

namespace Larje.Character.AI
{
    public class AIActionLookAtCamera : AIAction
    {
        private Transform _camera;
        private CharacterOrientation _orientation;

        protected override void OnInitialized()
        {
            _camera = Camera.main.transform;
            _orientation = Brain.Owner.GetComponentInChildren<CharacterOrientation>();
        }

        protected override void OnEnterState()
        {
            _orientation.BodyTarget.InputDirection.AddValue(GetDirectionToCamera, () => 10);
            _orientation.BodyTarget.InputSpeedPercent.AddValue(GetRotationSpeed, () => 10);
            _orientation.LookTarget.InputDirection.AddValue(GetDirectionToCamera, () => 10);
            _orientation.LookTarget.InputSpeedPercent.AddValue(GetRotationSpeed, () => 10);
        }

        protected override void OnExitState()
        {
            _orientation.BodyTarget.InputDirection.RemoveValue(GetDirectionToCamera);
            _orientation.BodyTarget.InputSpeedPercent.RemoveValue(GetRotationSpeed);
            _orientation.LookTarget.InputDirection.RemoveValue(GetDirectionToCamera);
            _orientation.LookTarget.InputSpeedPercent.RemoveValue(GetRotationSpeed);
        }

        private Vector3 GetDirectionToCamera()
        {
            return (_camera.position - Brain.Owner.transform.position).XZ().normalized;
        }

        private float GetRotationSpeed()
        {
            return 1f;
        }

        public override void PerformAction() {}
    }
}
