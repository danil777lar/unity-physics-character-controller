using Larje.Character.AI;
using UnityEngine;

namespace Larje.Character
{
    public interface IHealthDamagePreprocessor
    {
        public DamageData ProcessDamage(DamageData damageData);
    }
}
