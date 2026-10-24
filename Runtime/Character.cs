using Larje.Core.GameFlow;
using Larje.Character.AI;
using UnityEngine;
using System.Collections.Generic;
using System;
using Larje.Core;
using Larje.Core.CompositeProperties;

namespace Larje.Character
{
    public sealed class Character : MonoBehaviour
    {
        [SerializeField] private CharacterType _characterType;
        [Space]
        [SerializeField] private BoolComposite _isActive = new BoolComposite();
        [SerializeField] private List<GameStateId> _activeInGameStates = new List<GameStateId>();

        private IGameStateService _gameStateService;

        private Health _health;
        private Dictionary<System.Type, CharacterAbility> _abilities = new Dictionary<System.Type, CharacterAbility>();
        
        public bool IsAlive => _health == null || _health.IsAlive;
        public bool IsActive => _isActive.Value && IsActiveInCurrentGameState();
        public CharacterType CharacterType => _characterType;
        public Health Health 
        {
            get
            {
                if (_health == null)
                {
                    _health = GetComponentInChildren<Health>();
                }
                return _health;
            }
        }

        public BoolComposite IsActiveComposite => _isActive;

        public event Action EventDeath;

        public T FindAbility<T>() where T : CharacterAbility
        {
            T ability = null;

            if (_abilities.ContainsKey(typeof(T)))
            {
                ability = _abilities[typeof(T)] != null ? _abilities[typeof(T)] as T : null;
            }
            else
            {
                ability = GetComponentInChildren<T>();
                _abilities.Add(typeof(T), ability);
            }

            return ability;
        }

#if LARJE_ZENJECT
        [Zenject.Inject]
#endif
        public void Construct(IGameStateService gameStateService)
        {
            _gameStateService = gameStateService;
        }

        private void Awake()
        {
            if (Health != null)
            {
                Health.EventDeath += OnDeath;
            }
        }

        private void OnDeath(DamageData data)
        {
            EventDeath?.Invoke();
        }

        private bool IsActiveInCurrentGameState()
        {
            if (_activeInGameStates.Count == 0 || _gameStateService == null)
            {
                return true;
            }

            return _activeInGameStates.Contains(_gameStateService.Current);
        }
    }

    public enum CharacterType
    {
        Player,
        NPC
    }
}
