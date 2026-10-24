using Larje.Character.AI;
using UnityEngine;

namespace Larje.Character
{
    public class CharacterRagdoller : CharacterAbility
    {
        [SerializeField] private bool ragdollOnDeath;
        [Space]
        [SerializeField] private GameObject ragdollRoot;

        private Animator _animator;

        protected override void OnInitialized()
        {
            _animator = ragdollRoot.GetComponentInChildren<Animator>();

            DisableRagdoll();

            if (ragdollOnDeath)
            {
                character.Health.EventDeath += (d) => 
                {
                    EnableRagdoll();
                    ApplyForceToRagdoll(d.hitDirection);
                };
                character.Health.EventRevived += () => DisableRagdoll();
            }
        }

        public void EnableRagdoll()
        {
            SetActiveComponents(true);
        }

        public void DisableRagdoll()
        {
            SetActiveComponents(false);
        }

        public void ApplyForceToRagdoll(Vector3 force, ForceMode mode = ForceMode.Impulse)
        {
            foreach (Rigidbody rb in ragdollRoot.GetComponentsInChildren<Rigidbody>())
            {
                rb.AddForce(force, mode);
            }
        }

        private void SetActiveComponents(bool active)
        {
            _animator.enabled = !active;
            foreach (Rigidbody rb in ragdollRoot.GetComponentsInChildren<Rigidbody>())
            {
                rb.isKinematic = !active;
            }

            foreach (Collider col in ragdollRoot.GetComponentsInChildren<Collider>())
            {
                col.enabled = active;
            }
        }
    }
}
