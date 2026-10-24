using Larje.Character;
using UnityEngine;

namespace Larje.Character.AI
{
    public class AIAnchorPositionStatic : MonoBehaviour, IAIAnchorPosition
    {
        [SerializeField] private Transform point;

        public Vector3 Position => point.position;
    }
}
