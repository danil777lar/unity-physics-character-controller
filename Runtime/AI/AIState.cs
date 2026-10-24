using Larje.Character;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Larje.Character.AI
{
    [Serializable]
    public class AIState 
    {
        public string StateName;
        
        public List<AIAction> Actions;
        public List<AITransition> Transitions;

        private AIBrain _brain;
        
        public void SetBrain(AIBrain brain)
        {
            _brain = brain;
        }
        
        public void EnterState()
        {
            foreach (AIAction action in Actions)
            {
                action.EnterState();
            }
            
            foreach (AITransition transition in Transitions)
            {
                if (transition.Decision != null)
                {
                    transition.Decision.EnterState();
                }
            }
        }
        
        public void ExitState()
        {
            foreach (AIAction action in Actions)
            {
                action.ExitState();
            }
            
            foreach (AITransition transition in Transitions)
            {
                if (transition.Decision != null)
                {
                    transition.Decision.ExitState();
                }
            }
        }
        
        public void PerformActions()
        {
            if (Actions.Count == 0)
            {
                return;
                
            }
            
            for (int i=0; i<Actions.Count; i++) 
            {
                if (Actions[i] != null)
                {
                    Actions[i].PerformAction();
                }
                else
                {
                    Debug.LogError("An action in " + _brain.gameObject.name + " on state " + StateName + " is null.");
                }
            }
        }
        
        public void EvaluateTransitions()
        {
            if (Transitions.Count == 0)
            {
                return; 
            }
            
            for (int i = 0; i < Transitions.Count; i++) 
            {
                if (Transitions[i].Decision != null)
                {
                    if (Transitions[i].Decision.Decide())
                    {
                        if (!string.IsNullOrEmpty(Transitions[i].TrueState))
                        {
                            _brain.TransitionToState(Transitions[i].TrueState);
                            break;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(Transitions[i].FalseState))
                        {
                            _brain.TransitionToState(Transitions[i].FalseState);
                            break;
                        }
                    }
                }                
            }
        }        
    }
}