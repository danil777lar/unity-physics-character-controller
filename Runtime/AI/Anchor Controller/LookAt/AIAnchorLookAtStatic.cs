using Larje.Character;
using UnityEngine;

namespace Larje.Character.AI
{
    public class AIAnchorLookAtStatic : MonoBehaviour, IAIAnchorLookAt
    {
        [SerializeField] private Transform lookAtPoint;
        [Header("Noise")] 
        [SerializeField] private bool useNoise = false;
        [SerializeField] private float noiseDistance = 4f;
        [SerializeField] private float noiseSpeed = 1f;

        private Vector3 _initialPosition;

        public Vector3 LookAt => lookAtPoint.position;

        private void Start()
        {
            _initialPosition = lookAtPoint.position;
        }

        private void Update()
        {
            if (useNoise)
            {
                Vector3 noise = new Vector3(
                    (Mathf.PerlinNoise(Time.time * noiseSpeed, 0f) - 0.5f) * 2f,
                    (Mathf.PerlinNoise(0f, Time.time * noiseSpeed) - 0.5f) * 2f,
                    (Mathf.PerlinNoise(Time.time * noiseSpeed, Time.time * noiseSpeed) - 0.5f) * 2f
                ) * noiseDistance;
                lookAtPoint.position = _initialPosition + noise;
            }
        }
    }
}
