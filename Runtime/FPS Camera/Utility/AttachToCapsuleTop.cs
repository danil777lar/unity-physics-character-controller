using Larje.Character.AI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Larje.Character
{
    public class AttachToCapsuleTop : MonoBehaviour
    {
        [Header("Offset")]
        [SerializeField] private Vector3 capsuleTopOffset;
        [SerializeField] private Vector3 interpolateSpeed;

        private CapsuleCollider _collider;

        private void Start()
        {
            _collider = GetComponentInParent<CapsuleCollider>();
        }

        private void Update()
        {
            if (_collider == null) return;

            Vector3 capsuleTop = _collider.center + Vector3.up * (_collider.height * 0.5f);
            Vector3 targetPosition = capsuleTop + _collider.transform.TransformVector(capsuleTopOffset);
            Vector3 resultPosition = transform.localPosition;

            resultPosition.x = Mathf.Lerp(resultPosition.x, targetPosition.x, interpolateSpeed.x * Time.deltaTime);
            resultPosition.y = Mathf.Lerp(resultPosition.y, targetPosition.y, interpolateSpeed.y * Time.deltaTime);
            resultPosition.z = Mathf.Lerp(resultPosition.z, targetPosition.z, interpolateSpeed.z * Time.deltaTime);

            transform.localPosition = resultPosition;
        }
    }
}
