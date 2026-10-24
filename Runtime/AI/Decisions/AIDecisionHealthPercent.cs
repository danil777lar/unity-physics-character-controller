using Larje.Character;
using Larje.Character.AI;
using UnityEngine;

namespace Larje.Character.AI
{
    public class AIDecisionHealthPercent : AIDecision
    {
        [Space]
        [SerializeField] private ConditionType condition; 
        [SerializeField, Range(0f, 1f)] private float healthPercent;

        public override bool Decide()
        {
            if (Brain.Owner.Health != null)
            {
                float currentPercent = Brain.Owner.Health.HealthPercent;
                if (condition == ConditionType.Greater)
                {
                    return currentPercent >= healthPercent;
                }
                else
                {
                    return currentPercent <= healthPercent;
                }
            }

            return false;
        }

        private enum ConditionType
        {
            Less,
            Greater
        }
    }
}
