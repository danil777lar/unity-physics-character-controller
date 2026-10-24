using Larje.Character;
using Larje.Character.AI;
using UnityEngine;

namespace Larje.Character.AI.Decisions
{
    public class AIDecisionTimeInState : AIDecision
    {
        [Space] 
        [SerializeField] private float timeMin;
        [SerializeField] private float timeMax;

        private float _targetTime;
        
        public override bool Decide()
        {
            return Brain.TimeInThisState >= _targetTime;
        }

        protected override void OnEnterState()
        {
            _targetTime = Random.Range(timeMin, timeMax);
        }
    }
}
