using Larje.Core.GameFlow;
using UnityEngine;

namespace Larje.Character
{
    public sealed class SetStateOnDie : MonoBehaviour
    {
        [SerializeField] private GameStateId gameState;

        private Health _health;
        private IGameStateService _gameState;

#if LARJE_ZENJECT
        [Zenject.Inject]
#endif
        public void Construct(IGameStateService gameStateService)
        {
            _gameState = gameStateService;
        }

        private void Start()
        {
            _health = GetComponentInParent<Health>();
            if (_health != null)
            {
                _health.EventDeath += OnDeath;
            }
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.EventDeath -= OnDeath;
            }
        }

        private void OnDeath(DamageData data)
        {
            if (_gameState != null)
            {
                _gameState.Set(gameState);
            }
        }
    }
}
