using Larje.Character;
using System.Collections.Generic;
using UnityEngine;

namespace Larje.Character.AI
{
    public abstract class AIDecision : MonoBehaviour
    {
        [field: SerializeField] public string Label { get; private set; }

        protected bool DecisionInProgress { get; private set; }
        protected AIBrain Brain { get; private set; }

        public abstract bool Decide();
        
        protected virtual void Awake()
        {
            Brain = this.gameObject.GetComponentInParent<AIBrain>();
        }
        
        public void Initialization()
        {
            OnInitialized();
        }

        public void EnterState()
        {
            DecisionInProgress = true;
            OnEnterState();
        }

        public void ExitState()
        {
            DecisionInProgress = false;
            OnExitState();
        }

        protected virtual void OnInitialized()
        {
        }
        
        protected virtual void OnEnterState()
        {
        }
        
        protected virtual void OnExitState()
        {
        }
    }
}
