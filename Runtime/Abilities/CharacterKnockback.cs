using Larje.Character.AI;
using Larje.Character;
using UnityEngine;

namespace Larje.Character
{
    public class CharacterKnockback : CharacterAbility
    {
        [SerializeField] private bool normalizeHitDirection = false;
        [SerializeField] private float knockbackForce;
        [SerializeField] private float knockbackDuration;

        protected override void OnInitialized()
        {
            character.Health.EventDamage += OnDamageTaken;
        }

        private void OnDamageTaken(DamageData damageData)
        {
            if (damageData.damage > 0)
            {
                Vector3 force = damageData.hitDirection;
                if (normalizeHitDirection)
                {
                    force = force.normalized;
                }
                force *= knockbackForce;
                physics.AddForce(force, ForceMode.Impulse);
            }
        }
    }
}
