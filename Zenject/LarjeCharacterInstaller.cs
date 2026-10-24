using UnityEngine;
using Zenject;

namespace Larje.Character.Zenject
{
    [CreateAssetMenu(fileName = "LarjeCharacterInstaller", menuName = "Larje/Character/Zenject/Character Installer")]
    public sealed class LarjeCharacterInstaller : ScriptableObjectInstaller<LarjeCharacterInstaller>
    {
        [SerializeField] private CursorConfig cursor;

        public override void InstallBindings()
        {
            Container.BindLarjeCharacter(cursor);
        }
    }
}
