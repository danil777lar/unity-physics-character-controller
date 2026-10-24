using Larje.Character.AI;
using UnityEngine;

namespace Larje.Character
{
    public class WeaponRoot : MonoBehaviour
    {
        [SerializeField] private Transform attachTo;

        private Transform _attachTarget;

        private void Start()
        {
            if (attachTo != null)
            {
                _attachTarget = new GameObject("Weapon Attach").transform;
                _attachTarget.SetParent(attachTo);
                _attachTarget.position = transform.position;
                _attachTarget.rotation = transform.rotation;
            }
        }

        private void Update()
        {
            if (_attachTarget != null)
            {
                transform.position = _attachTarget.position;
                transform.rotation = _attachTarget.rotation;
            }
        }
    }
}
