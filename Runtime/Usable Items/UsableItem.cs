using Larje.Character.AI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Larje.Character
{
    public abstract class UsableItem : MonoBehaviour
    {
        public Func<bool> UseOriginPosition;
        public Func<bool> UseTargetPosition;
        public Func<Vector3> OriginPosition;  
        public Func<Vector3> TargetPosition;

        private Dictionary<int, bool> _actions = new Dictionary<int, bool>();

        public void StartAction(int actionId)
        {
            _actions[actionId] = true;
            OnActionStarted(actionId);
        }

        public void StopAction(int actionId)
        {
            _actions[actionId] = false;
            OnActionStopped(actionId);
        }

        public bool IsActionInProgress(int actionId)
        {
            return _actions.ContainsKey(actionId) && _actions[actionId];
        }

        public abstract void Equip();
        public abstract void Unequip(Action onComplete);
        public abstract bool CanStartAction(int actionId);

        protected virtual void OnActionStarted(int actionId) { }
        protected virtual void OnActionStopped(int actionId) { }
    }
}
