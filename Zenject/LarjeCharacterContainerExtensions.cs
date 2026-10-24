using Zenject;

namespace Larje.Character.Zenject
{
    public static class LarjeCharacterContainerExtensions
    {
        public static void BindLarjeCharacter(this DiContainer container, CursorConfig cursor)
        {
            container.BindInterfacesAndSelfTo<PlayerProviderService>().AsSingle();
            if (cursor != null)
            {
                container.BindInstance(cursor);
                container.BindInterfacesAndSelfTo<CursorService>().AsSingle();
            }
        }
    }
}
