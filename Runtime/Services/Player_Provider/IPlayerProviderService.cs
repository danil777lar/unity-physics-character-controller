using System;

namespace Larje.Character
{
    public interface IPlayerProviderService
    {
        event Action<Character> PlayerChanged;

        bool TryGetPlayer(out Character player);
        void SetPlayer(Character player);
        void ClearPlayer(Character player);
    }
}
