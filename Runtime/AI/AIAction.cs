using Larje.Character;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Larje.Character.AI
{
    public abstract class AIAction : MonoBehaviour
    {
        [field: SerializeField] public string Label { get; private set; }
        [field: SerializeField] public InitializationModes InitializationMode { get; private set; } = InitializationModes.OnlyOnce;
        
        private bool _initialized;
        
        public bool ActionInProgress { get; private set; }
        public AIBrain Brain { get; private set; }

        protected bool ShouldInitialize
        {
            get
            {
                switch (InitializationMode)
                {
                    case InitializationModes.EveryTime:
                        return true;
                    case InitializationModes.OnlyOnce:
                        return _initialized == false;
                }
                return true;
            }
        }
        
        public abstract void PerformAction();
        
        protected virtual void Awake()
        {
            Brain = this.gameObject.GetComponentInParent<AIBrain>();
        }
        
        public void Initialization()
        {
            if (ShouldInitialize)
            {
                OnInitialized();
            }
            _initialized = true;
        }
        
        public void EnterState()
        {
            ActionInProgress = true;
            OnEnterState();
        }
        
        public void ExitState()
        {
            ActionInProgress = false;
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

        public enum InitializationModes
        {
            EveryTime, 
            OnlyOnce
        }
    }
}