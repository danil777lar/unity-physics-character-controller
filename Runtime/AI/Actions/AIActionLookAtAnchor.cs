using Larje.Character.Abilities;
using Larje.Core.Extensions;
using Larje.Character;
using Larje.Character.AI;
using UnityEngine;

namespace Larje.Character.AI
{
    public class AIActionLookAtAnchor : AIAction
    {
        private IAIAnchorLookAt _anchorLookAt;

        private CharacterOrientation _orientation;

        protected override void OnInitialized()
        {
            _anchorLookAt = GetComponentInParent<IAIAnchorLookAt>();
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
            if (_anchorLookAt == null)
            {
                return Vector3.forward;
            }

            return (_anchorLookAt.LookAt - Brain.Owner.transform.position).XZ().normalized;
        }

        private float GetRotationSpeed()
        {
            return 1f;
        }

        public override void PerformAction() {}
    }
}
