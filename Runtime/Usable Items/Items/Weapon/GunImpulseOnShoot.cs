using Larje.Character.AI;
using Larje.Character;
using UnityEngine;

namespace Larje.Character
{
    public class GunImpulseOnShoot : MonoBehaviour
    {
        [SerializeField] private FPSCameraShakeAnimation shootAnim; 

        private GunUsableItem _gun;
        private FPSCameraShaker _shaker;

        private void Start()
        {
            _gun = GetComponent<GunUsableItem>();
            _shaker = GetComponentInParent<Character>().GetComponentInChildren<FPSCameraShaker>();

            if (_shaker != null)
            {
                _gun.EventShoot += OnShoot;
            }
        }

        private void OnShoot()
        {
            _shaker.AddShake(new FPSCameraShaker.Shake(shootAnim, false, () => 1f, () => 1f));
        }
    }
}
