using Larje.Core.Meshes;
using Larje.Core.Extensions;
using Larje.Character.AI;
using System;
using Larje.Character;
using Larje.Core.CompositeProperties;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Larje.Character
{
    public class CharacterPhysics : MonoBehaviour
    {
        [Header("Options")]
        [SerializeField] private bool disableCollisionsWhenInactive = true;
        [SerializeField] private bool kinematicWhenInactive = true;

        [Header("Contacts")]
        [SerializeField] private LayerMask collisionMask;
        [SerializeField] private float raycastOffset = 0.1f;

        [Header("Floating")]
        [SerializeField] private float floatingHeight = 0.25f;
        [SerializeField] private float floatingSpringStrength = 100f;
        [SerializeField] private float floatingSpringDamper = 10f;

        [Header("Size")]
        [SerializeField] private float heightChangeSpeed = 10f;
        [SerializeField] private float radiusChangeSpeed = 10f;

        [Header("Gravity")]
        [SerializeField] public BoolComposite useGravity;
        [SerializeField] private float gravity = 40f;

        [Header("Slope")]
        [SerializeField] private bool useSlopeLimit = true;
        [SerializeField] private float slopeLimit = 45f;

        [Header("Logic")]
        [SerializeField] private bool resetOnDeath = true;
        [SerializeField] private bool kinematicOnDeath = true;

        [Header("Gizmos")]
        [SerializeField] private bool drawGizmos = false;
        [SerializeField] private Color gizmoColor = Color.white.SetAlpha(0.5f);

        private bool _grounded;
        private float _colliderHeightDefault;
        private float _colliderRadiusDefault;

        private RaycastHit _groundHit;

        private Character _character;
        private Rigidbody _rigidbody;
        private CapsuleCollider _collider;

        private Vector3 RBVelocity
        {
            #if UNITY_6000_0_OR_NEWER
            get => _rigidbody.linearVelocity;
            set => _rigidbody.linearVelocity = value;
            #else
            get => _rigidbody.velocity;
            set => _rigidbody.velocity = value;
            #endif
        } 

        public bool Grounded => _grounded;
        public float Mass => _rigidbody.mass;
        public float ColliderRadiusCurrent => _collider.radius;
        public float ColliderHeightCurrent => _collider.height;
        public float ColliderRadiusDefault => _colliderRadiusDefault;
        public float ColliderHeightDefault => _colliderHeightDefault;
        public Vector3 ColliderCenter => _collider.center;
        public Vector3 Velocity => RBVelocity; 
        public float VerticalSpeed => RBVelocity.Y().magnitude; 
        public float HorizontalSpeed => RBVelocity.XZ().magnitude; 
        public FloatProduct ColliderHeightMultiplier { get; } = new FloatProduct();
        public FloatProduct ColliderRadiusMultiplier { get; } = new FloatProduct();
        public GameObject Ground => _groundHit.collider != null ? _groundHit.collider.gameObject : null;

        public event Action EventGrounded;
        public event Action EventUngrounded;

        public void ResetVelocity()
        {
            RBVelocity = Vector3.zero;
        }

        public void AddForce(Vector3 force, ForceMode forceMode = ForceMode.Acceleration)
        {
            _rigidbody.AddForce(force, forceMode); 
        }

        public void AddSlopeTranslatedForce(Vector3 force, ForceMode forceMode = ForceMode.Acceleration)
        {
            Vector3 slopeNormal = _groundHit.normal;
            Vector3 slopeForce = Vector3.ProjectOnPlane(force, slopeNormal);
            _rigidbody.AddForce(slopeForce, forceMode); 
        }

        public void TranslateVelocity(Vector3 direction)
        {
            RBVelocity = direction.normalized * RBVelocity.magnitude;
        }

        public bool Capsulecast(Vector3 vector, out RaycastHit hit, float radiusMultiplier = 1f, bool useOffset = true, bool removeInsideHits = true)
        {
            hit = default;

            Vector3 bottom = _rigidbody.position;
            bottom += transform.up * _collider.radius;
            if (useOffset)
            {
                bottom += vector.normalized * raycastOffset;
            }

            Vector3 top = _rigidbody.position + transform.up * _collider.height;
            top -= transform.up * _collider.radius;
            if (useOffset)
            {
                top += vector.normalized * raycastOffset;
            }

            float radius = _collider.radius * radiusMultiplier;
            RaycastHit[] results = Physics.CapsuleCastAll(bottom, top, radius, vector.normalized, 
                vector.magnitude, collisionMask);

            return TryFilterHits(results, removeInsideHits, out hit);
        }

        public bool Raycast(Vector3 from, Vector3 vector, out RaycastHit hit)
        {
            RaycastHit[] results = Physics.RaycastAll(from, vector.normalized, 
                vector.magnitude, collisionMask);

            return TryFilterHits(results, true, out hit);
        }

        private void Awake()
        {
            _character = GetComponent<Character>();
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();

            _colliderHeightDefault = _collider.height;
            _colliderRadiusDefault = _collider.radius;

            _character.EventDeath += OnDeath;
        }

        private void FixedUpdate()
        {
            if (disableCollisionsWhenInactive)
            {
                _collider.enabled = _character.IsActive;
            }

            if (kinematicWhenInactive)
            {
                _rigidbody.isKinematic = !_character.IsActive;
            }

            if (_character.IsActive)
            {
                UpdateColliderSize();
                UpdateGravity();
                UpdateFloating();
            }
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos)
            {
                return;
            }

            Gizmos.color = gizmoColor;
            if (_collider != null)
            {
                Mesh capsule = MeshPrimitives.GenerateCapsuleMesh(_collider.radius, _collider.height);
                Gizmos.DrawMesh(capsule, transform.position + _collider.center, transform.rotation);
            }
        }

        private void UpdateColliderSize()
        {
            float targetHeight = (_colliderHeightDefault * ColliderHeightMultiplier.Value) - floatingHeight;
            float targetRadius = _colliderRadiusDefault * ColliderRadiusMultiplier.Value; 
            Vector3 targetCenter = new Vector3(0f, (_collider.height * 0.5f), 0f);
            _collider.radius = Mathf.MoveTowards(_collider.radius, targetRadius, Time.fixedDeltaTime * radiusChangeSpeed);
            _collider.height = Mathf.MoveTowards(_collider.height, targetHeight, Time.fixedDeltaTime * heightChangeSpeed);
            _collider.center = Vector3.MoveTowards(_collider.center, targetCenter, Time.fixedDeltaTime * heightChangeSpeed);
        }

        private void UpdateGravity()
        {
            bool wasGrounded = _grounded;

            Vector3 origin = transform.position + _collider.center;
            Vector3 rayDir = -transform.up * ((ColliderHeightCurrent * 0.5f) + (floatingHeight * 2f));

            _grounded = Raycast(origin, rayDir, out _groundHit);
            if (!_grounded)
            {
                _grounded = Capsulecast(-transform.up * (floatingHeight * 2f), out _groundHit, 0.9f);
                if (_groundHit.point.y > transform.position.y + _collider.radius)
                {
                    _grounded = false;
                }
            }

            if (_grounded && useSlopeLimit)
            {
                Vector3 normal = _groundHit.normal;
                float angle = Vector3.Angle(normal, transform.up);
                if (angle > slopeLimit)
                {
                    _grounded = false;
                }
            }

            if (useGravity.Value && !_grounded)
            {
                if (useSlopeLimit)
                {
                    AddSlopeTranslatedForce(Vector3.down * gravity);
                }
                else
                {
                    AddForce(Vector3.down * gravity);
                }
            }

            if (wasGrounded != _grounded)
            {
                if (_grounded) EventGrounded?.Invoke();
                else EventUngrounded?.Invoke();
            }
        }

        private void UpdateFloating()
        {
            Vector3 velocity = RBVelocity;
            Vector3 rayDirection = -transform.up;

            if (_grounded)
            {
                Vector3 otherVelocity = Vector3.zero;
                if (_groundHit.rigidbody != null)
                {
                #if UNITY_6000_0_OR_NEWER 
                    otherVelocity = _groundHit.rigidbody.linearVelocity;                
                #else
                    otherVelocity = _groundHit.rigidbody.velocity;
                #endif
                }

                float rayDirVel = Vector3.Dot(rayDirection, velocity);
                float otherDirVel = Vector3.Dot(rayDirection, otherVelocity);
                float relativeVelocity = rayDirVel - otherDirVel;

                float distance = Mathf.Abs(_groundHit.point.y - transform.position.y) - floatingHeight;
                float springForce = (distance * floatingSpringStrength) - (relativeVelocity * floatingSpringDamper);

                _rigidbody.AddForce(rayDirection * springForce);
            }   
        }

        private bool TryFilterHits(RaycastHit[] hits, bool removeInsideHits, out RaycastHit result)
        {
            result = default;
            bool hitten = false;

            foreach (RaycastHit res in hits)
            {
                if (res.collider != null)
                {
                    bool otherObject = res.collider.gameObject != gameObject;
                    if (res.collider.attachedRigidbody != null)
                    {
                        otherObject &= res.collider.attachedRigidbody.gameObject != gameObject;
                    }

                    if (otherObject && (res.point != Vector3.zero || !removeInsideHits))
                    {
                        result = res;
                        hitten = true;

                        if (drawGizmos)
                        {
                            Debug.DrawRay(result.point, result.normal, gizmoColor);
                        }

                        break;
                    }
                }
            }

            return hitten;
        }

        private void OnDeath()
        {
            if (resetOnDeath)   
            {
                ResetVelocity();
            }

            if (kinematicOnDeath)
            {
                _rigidbody.isKinematic = true;
            }
        }
    }
}
