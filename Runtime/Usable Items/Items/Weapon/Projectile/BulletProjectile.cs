using Larje.Core.GameFlow;
using Larje.Character.AI;
using System;
using System.Collections.Generic;
using Larje.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Larje.Character
{
    public class BulletProjectile : Projectile
    {
        [SerializeField] private float damage = 10;
        [Space]
        [SerializeField] private float speed = 100f;
        [SerializeField] private float speedDrag = 0f;
        [SerializeField] private float gravity = 0f;
        [SerializeField] private float lifeTime = 2f;
        [SerializeField] private float destroyDelay = 0.1f;
        [SerializeField] private GameObject modelHolder;
        [SerializeField] private LayerMask hitLayer;
        [Space]
        [SerializeField] private List<GameStateId> activeInGameStates = new List<GameStateId>();
        [Space] 
        [SerializeField] private List<GameObject> spawnOnHit;
        [Space] 
        [SerializeField] private UnityEvent onHit;

        private IGameStateService _gameStateService;

        private float _currentSpeed;
        private float _currentGravity;
        private bool _destroying;
        private float _currentLifetime;

#if LARJE_ZENJECT
        [Zenject.Inject]
#endif
        public void Construct(IGameStateService gameStateService)
        {
            _gameStateService = gameStateService;
        }

        public void Init(Vector3 position, Vector3 direction, Vector3 modelStartPosition)
        {
            _currentLifetime = lifeTime;
            _currentSpeed = speed;
            _currentGravity = 0f;

            transform.position = position;
            transform.forward = direction;

            if (modelHolder != null)
            {
                modelHolder.transform.position = modelStartPosition;
            }
        }

        private void FixedUpdate()
        {
            if (_destroying || !IsActiveInCurrentState())
            {
                return;
            }

            Move();
        }

        private void Update()
        {
            if (_destroying)
            {
                modelHolder.transform.localPosition = Vector3.zero;
            }
            else
            {
                modelHolder.transform.localPosition = 
                    Vector3.Lerp(modelHolder.transform.localPosition, Vector3.zero, Time.deltaTime * 10f);   
            }
        }

        private bool IsActiveInCurrentState()
        {
            return activeInGameStates.Count == 0 || _gameStateService == null || activeInGameStates.Contains(_gameStateService.Current);
        }

        private void Move()
        {
            _currentSpeed = _currentSpeed * (1f - speedDrag * Time.fixedDeltaTime);
            _currentGravity -= gravity * Time.fixedDeltaTime;

            Vector3 velocity = transform.forward * _currentSpeed + Vector3.up * _currentGravity;

            bool hitten = false;
            Vector3 position = transform.position + velocity * Time.fixedDeltaTime;
            Vector3 direction = velocity.normalized;

            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, velocity.magnitude * Time.fixedDeltaTime, hitLayer))
            {
                position = hit.point;
                direction = hit.normal; 
                hitten = true;

                IDamageTarget target = hit.collider.GetComponentInParent<IDamageTarget>();
                DamageData damageData = new DamageData
                {
                    damage = damage,
                    hitPoint = hit.point,
                    hitNormal = hit.normal,
                    hitDirection = transform.forward
                };
                target?.SendDamage(damageData);

                OnHit(hit.normal);
            }

            transform.position = position;
            transform.forward = direction;
            _currentLifetime -= Time.fixedDeltaTime;

            if (!hitten && _currentLifetime <= 0)
            {
                OnEndLifetime();
            }
        }

        private void OnHit(Vector3 normal)
        {
            foreach (GameObject prefab in spawnOnHit)
            {
                GameObject instance = Instantiate(prefab, transform.parent);
                instance.SetActive(true);
                instance.transform.position = transform.position;
                instance.transform.forward = transform.forward;

                if (instance.TryGetComponent(out Rigidbody rb))
                {
                    rb.AddForce(normal * ((_currentSpeed + _currentGravity) * 0.25f), ForceMode.VelocityChange);
                }
            }

            onHit.Invoke();
            DestroySelf();
        }

        private void OnEndLifetime()
        {
            DestroySelf();
        }

        private void DestroySelf()
        {
            _destroying = true;
            Destroy(gameObject, destroyDelay);
        }
    }
}
