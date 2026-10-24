using System;

namespace Larje.Character
{
    public sealed class PlayerProviderService : IPlayerProviderService
    {
        private Character _player;

        public event Action<Character> PlayerChanged;

        public bool TryGetPlayer(out Character player)
        {
            player = _player;
            return _player != null;
        }

        public void SetPlayer(Character player)
        {
            if (_player != player)
            {
                _player = player;
                PlayerChanged?.Invoke(player);
            }
        }

        public void ClearPlayer(Character player)
        {
            if (_player == player)
            {
                SetPlayer(null);
            }
        }
    }
}
