using UnityEngine;

namespace Larje.Character
{
    public sealed class PlayerProviderConnector : MonoBehaviour
    {
        private Character _player;
        private IPlayerProviderService _provider;

#if LARJE_ZENJECT
        [Zenject.Inject]
#endif
        public void Construct(IPlayerProviderService provider)
        {
            _provider = provider;
            Register();
        }

        private void OnEnable()
        {
            Register();
        }

        private void OnDestroy()
        {
            if (_provider != null && _player != null)
            {
                _provider.ClearPlayer(_player);
            }
        }

        private void Register()
        {
            if (_provider == null)
            {
                return;
            }

            _player = GetComponentInParent<Character>();
            if (_player != null)
            {
                _provider.SetPlayer(_player);
            }
        }
    }
}
