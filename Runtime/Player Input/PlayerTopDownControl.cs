using Larje.Character.Abilities;
using Larje.Core.Input;
using UnityEngine;

namespace Larje.Character
{
    public sealed class PlayerTopDownControl : MonoBehaviour
    {
        [SerializeField] private int priority = 1;

        private IInputService _input;

#if LARJE_ZENJECT
        [Zenject.Inject]
#endif
        public void Construct(IInputService input)
        {
            _input = input;
        }

        private void Start()
        {
            CharacterWalk walk = GetComponentInParent<CharacterWalk>();
            if (walk != null)
            {
                walk.Input.AddValue(GetInput, () => priority);
            }
        }

        private Vector2 GetInput()
        {
            return _input != null ? _input.Movement : Vector2.zero;
        }
    }
}
