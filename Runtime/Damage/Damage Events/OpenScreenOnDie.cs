using Larje.Core.UI;
using UnityEngine;

namespace Larje.Character
{
    public sealed class OpenScreenOnDie : MonoBehaviour
    {
        [SerializeField] private UIScreenId screen;

        private Health _health;
        private IUIService _ui;

#if LARJE_ZENJECT
        [Zenject.Inject]
#endif
        public void Construct(IUIService ui)
        {
            _ui = ui;
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
            if (_ui != null)
            {
                _ui.OpenScreen(screen);
            }
        }
    }
}
