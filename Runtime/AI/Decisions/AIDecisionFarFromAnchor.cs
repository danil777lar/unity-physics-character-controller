using Larje.Character;
using Larje.Character.AI;
using UnityEngine;

namespace Larje.Character.AI
{
    public class AIDecisionFarFromAnchor : AIDecision
    {
        [SerializeField] private float distance;

        private IAIAnchorPosition _anchor;

        public override bool Decide()
        {
            if (_anchor == null)
            {
                return true;
            }

            return Vector3.Distance(Brain.Owner.transform.position, _anchor.Position) > distance;
        }

        protected override void OnInitialized()
        {
            _anchor = GetComponentInParent<IAIAnchorPosition>();
        }
    }
}
