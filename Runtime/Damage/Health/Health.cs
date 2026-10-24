using Larje.Character.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Larje.Character
{
    public class Health : MonoBehaviour, IDamageTarget
    {
        [SerializeField] private float initialHealth;
        [SerializeField] private List<GameObject> spawnOnDamage;
        [SerializeField] private List<GameObject> spawnOnDeath;

        private float _currentHealth;
        private List<IHealthDamagePreprocessor> _damagePreprocessors = new List<IHealthDamagePreprocessor>(); 

        public bool IsAlive => _currentHealth > 0;
        public float HealthPercent => (float)_currentHealth / (float)initialHealth;
        public float CurrentHealth => _currentHealth;
        public float InitialHealth => initialHealth;

        public event Action<DamageData> EventDamage;
        public event Action<DamageData> EventDeath;
        public event Action EventRevived;

        public void SetInitialHealth(float health, bool setCurrent = true)
        {
            initialHealth = health;
            if (setCurrent)
            {
                _currentHealth = health;
            }
        }

        public void Revive(float healthPercent = 1f)
        {
            _currentHealth = initialHealth * healthPercent;
            EventRevived?.Invoke();
        }

        public void Heal(float amount)
        {
            if (IsAlive)
            {
                _currentHealth = Mathf.Min(_currentHealth + amount, initialHealth);
            }
        }

        public void SendDamage(DamageData damageData)
        {
            if (IsAlive)
            {
                foreach (IHealthDamagePreprocessor preprocessor in _damagePreprocessors)
                {
                    damageData = preprocessor.ProcessDamage(damageData);
                }
                damageData.damagedTargets ??= new List<IDamageTarget>();
                damageData.damagedTargets.Add(this);

                _currentHealth -= damageData.damage;
                SpawnEffects(spawnOnDamage, damageData);
                EventDamage?.Invoke(damageData);

                if (_currentHealth <= 0)
                {
                    Death(damageData);
                }
            }
        }

        private void Start()
        {
            _currentHealth = initialHealth;
            _damagePreprocessors = GetComponentsInChildren<IHealthDamagePreprocessor>().ToList();
        }

        private void Death(DamageData damageData)
        {
            SpawnEffects(spawnOnDeath, damageData);
            EventDeath?.Invoke(damageData);
        }

        private void SpawnEffects(List<GameObject> prefabs, DamageData damageData)
        {
            foreach (GameObject prefab in prefabs)
            {
                GameObject go = Instantiate(prefab, transform.parent);
                if (go.TryGetComponent(out IDamageTarget target))
                {
                    target.SendDamage(damageData);
                }
            }
        }
    }
}
