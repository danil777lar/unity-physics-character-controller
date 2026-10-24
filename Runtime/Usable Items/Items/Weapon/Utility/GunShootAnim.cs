using Larje.Character.AI;
using UnityEngine;

namespace Larje.Character
{
    public class GunShootAnim : MonoBehaviour
    {
        [SerializeField] private float maxValue;
        [SerializeField] private float shotForce;
        [SerializeField] private float springForce;
        [SerializeField] private float springDamper;
        [Space] 
        [SerializeField] private Vector3 applyAngle;
        [SerializeField] private Vector3 applyPosition;

        private float _currentValue;
        private float _currentSpeed;
        private GunUsableItem _gun;

        private void Start()
        {
            _gun = GetComponentInParent<GunUsableItem>();
            _gun.EventShoot += OnShoot; 
        }

        private void FixedUpdate()
        {
            SpringDecreaseSpeed(); 

            _currentValue += _currentSpeed * Time.fixedDeltaTime;
            _currentValue = Mathf.Clamp(_currentValue, -maxValue, maxValue);

            transform.localRotation = Quaternion.Euler(applyAngle * _currentValue);
            transform.localPosition = applyPosition * _currentValue;
        }

        private void OnShoot()
        {
            _currentSpeed += shotForce;
        }

        private void SpringDecreaseSpeed()
        {
            _currentSpeed += (-_currentValue * springForce) * Time.fixedDeltaTime;
            _currentSpeed *= 1f - (springDamper * Time.fixedDeltaTime);
        }
    }
}
