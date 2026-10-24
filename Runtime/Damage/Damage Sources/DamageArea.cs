using Larje.Character.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Larje.Character
{
    public class DamageArea : MonoBehaviour
    {
        [SerializeField] private float damageAmount = 10;
        [SerializeField] private float damageInterval = 1f;

        private Dictionary<IDamageTarget, float> _damageTargets = new Dictionary<IDamageTarget, float>();
        private Dictionary<GameObject, IDamageTarget> _objectsHistory = new Dictionary<GameObject, IDamageTarget>();

        private void Update()
        {
            foreach (IDamageTarget target in _damageTargets.Keys.ToArray())
            {
                if (_damageTargets[target] + damageInterval <= Time.time)
                {
                    _damageTargets[target] = Time.time;
                    SendDamage(target);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (TryGetDamageTarget(other.gameObject, out IDamageTarget target))
            {
                if (!_damageTargets.ContainsKey(target))
                {
                    _damageTargets.Add(target, Time.time);
                    SendDamage(target);    
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (TryGetDamageTarget(other.gameObject, out IDamageTarget target))
            {
                if (_damageTargets.ContainsKey(target))
                {
                    _damageTargets.Remove(target);
                }
            }
        }

        private bool TryGetDamageTarget(GameObject go, out IDamageTarget target)
        {
            target = null;
            if (_objectsHistory.ContainsKey(go))
            {
                if (_objectsHistory[go] != null)
                {
                    target = _objectsHistory[go];
                }
            }
            else
            {
                target = go.GetComponent<IDamageTarget>();
                _objectsHistory.Add(go, target);
            }

            return target != null;
        }

        private void SendDamage(IDamageTarget target)
        {
            DamageData damage = new DamageData()
            {
                damage = damageAmount,
            };
            target.SendDamage(damage);
        }
    }
}
