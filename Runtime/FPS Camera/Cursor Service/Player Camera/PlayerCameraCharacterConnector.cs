using Larje.Character.AI;
using Larje.Character;
using UnityEngine;

namespace Larje.Character
{
    [RequireComponent(typeof(PlayerCamera))]
    public class PlayerCameraCharacterConnector : MonoBehaviour
    {
        private PlayerCamera _playerCamera;
        private Character _character;

        private void Start()
        {
            _playerCamera = GetComponent<PlayerCamera>();
            _character = GetComponentInParent<Character>();

            _playerCamera.permitted.AddValue(() => _character.IsAlive);
            _character.IsActiveComposite.AddValue(() => _playerCamera.IsCurrent);
        }
    }
}
