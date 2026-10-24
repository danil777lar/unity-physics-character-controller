using Larje.Character.AI;
using UnityEngine;

namespace Larje.Character
{
    public class CharacterAbility : MonoBehaviour
    {
        [SerializeField] private bool abilityEnabled = true;
        
        public bool Initialized { get; private set; }
        public virtual bool Permitted => character.IsAlive && character.IsActive && abilityEnabled;
        
        protected Character character { get; private set; }
        protected CharacterPhysics physics { get; private set; }

        public Transform Root => character.transform;

        private void Awake()
        {
            character = GetComponentInParent<Character>();
            physics = GetComponentInParent<CharacterPhysics>();

            if (character == null)
            {
                Debug.LogError($"CharacterAbility {name} requires a Character component in its parents.", this);
                return;
            }

            if (physics == null)
            {
                Debug.LogError($"CharacterAbility {name} requires a CharacterPhysics component in its parents.", this);
                return;
            }

            Initialized = true;

            OnInitialized();
        }

        protected virtual void OnInitialized()
        {
        }
    }
}
