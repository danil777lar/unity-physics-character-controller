using Larje.Character;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Larje.Character.AI
{
    public class AIBrain : MonoBehaviour
    {
        [SerializeField] public List<AIState> states;
        
        [SerializeField, Min(0f)] private float actionsFrequency = 0f;
        [SerializeField, Min(0f)] private float decisionFrequency = 0f;
        
        [SerializeField] private bool brainActive = true;
        [SerializeField] private bool resetBrainOnStart = true;
        [SerializeField] private bool resetBrainOnEnable = false;

        private float _lastActionsUpdate = 0f;
        private float _lastDecisionsUpdate = 0f;
        
        private AIState _initialState;
        private AIState _newState;
        private AIDecision[] _decisions;
        private AIAction[] _actions;

        public float TimeInThisState { get; private set; }
        public Vector3 LastKnownTargetPosition { get; private set; } = Vector3.zero;
        public Transform Target { get; set; }
        public AIState CurrentState { get; private set; }
        public Character Owner { get; private set; }

        public AIAction[] GetAttachedActions()
        {
            AIAction[] actions = this.gameObject.GetComponentsInChildren<AIAction>();
            return actions;
        }

        public AIDecision[] GetAttachedDecisions()
        {
            AIDecision[] decisions = this.gameObject.GetComponentsInChildren<AIDecision>();
            return decisions;
        }
        
        public void ResetBrain()
        {
            InitializeDecisions();
            InitializeActions();
            brainActive = true;
            this.enabled = true;

            Owner = GetComponentInParent<Character>();

            if (CurrentState != null)
            {
                CurrentState.ExitState();
                OnExitState();
            }
            
            if (states.Count > 0)
            {
                _newState = states[0];
                CurrentState = _newState;
                CurrentState?.EnterState();
            }  
        }
        
        public void TransitionToState(string newStateName)
        {
            _newState = FindState(newStateName);
            
            if (CurrentState == null)
            {
                CurrentState = _newState;
                if (CurrentState != null)
                {
                    CurrentState.EnterState();
                }
                return;
            }
            if (newStateName != CurrentState.StateName)
            {
                CurrentState.ExitState();
                OnExitState();

                CurrentState = _newState;
                if (CurrentState != null)
                {
                    CurrentState.EnterState();
                }                
            }
        }
        
        [ContextMenu("Delete unused actions and decisions")]
        public void DeleteUnusedActionsAndDecisions()
        {
            AIAction[] actions = this.gameObject.GetComponentsInChildren<AIAction>();
            AIDecision[] decisions = this.gameObject.GetComponentsInChildren<AIDecision>();
            foreach (AIAction action in actions)
            {
                bool found = false;
                foreach (AIState state in states)
                {
                    if (state.Actions.Contains(action))
                    {
                        found = true;
                    }
                }
                if (!found)
                {    
                    DestroyImmediate(action);
                }
            }
            foreach (AIDecision decision in decisions)
            {
                bool found = false;
                foreach (AIState state in states)
                {
                    foreach (AITransition transition in state.Transitions)
                    {
                        if (transition.Decision == decision)
                        {
                            found = true;
                        }
                    }
                }
                if (!found)
                {    
                    DestroyImmediate(decision);
                }
            }
        }

        private void OnEnable()
        {
            if (resetBrainOnEnable)
            {
                ResetBrain();
            }
        }
        
        private void Awake()
        {
            Owner = GetComponentInParent<Character>();
            
            foreach (AIState state in states)
            {
                state.SetBrain(this);
            }
            _decisions = GetAttachedDecisions();
            _actions = GetAttachedActions();
        }
        
        private void Start()
        {
            if (resetBrainOnStart)
            {
                ResetBrain();    
            }
        }
        
        private void Update()
        {
            if (!brainActive || (CurrentState == null) || (Time.timeScale == 0f))
            {
                return;
            }

            if (Owner != null && (!Owner.IsAlive || !Owner.IsActive))
            {
                return;
            }

            if (Time.time - _lastActionsUpdate > actionsFrequency)
            {
                CurrentState.PerformActions();
                _lastActionsUpdate = Time.time;
            }
            
            if (!brainActive)
            {
                return;
            }
            
            if (Time.time - _lastDecisionsUpdate > decisionFrequency)
            {
                CurrentState.EvaluateTransitions();
                _lastDecisionsUpdate = Time.time;
            }
            
            TimeInThisState += Time.deltaTime;

            StoreLastKnownPosition();
        }
        
        private void OnExitState()
        {
            TimeInThisState = 0f;
        }
        
        private void InitializeDecisions()
        {
            if (_decisions == null)
            {
                _decisions = GetAttachedDecisions();
            }
            foreach(AIDecision decision in _decisions)
            {
                decision.Initialization();
            }
        }
        
        private void InitializeActions()
        {
            if (_actions == null)
            {
                _actions = GetAttachedActions();
            }
            foreach(AIAction action in _actions)
            {
                action.Initialization();
            }
        }
        
        private AIState FindState(string stateName)
        {
            foreach (AIState state in states)
            {
                if (state.StateName == stateName)
                {
                    return state;
                }
            }
            if (stateName != "")
            {
                Debug.LogError("You're trying to transition to state '" + stateName + "' in " + this.gameObject.name + "'s AI Brain, but no state of this name exists. Make sure your states are named properly, and that your transitions states match existing states.");
            }            
            return null;
        }
        
        private void StoreLastKnownPosition()
        {
            if (Target != null)
            {
                LastKnownTargetPosition = Target.transform.position;
            }
        }
    }
}
