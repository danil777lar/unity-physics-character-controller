using Larje.Character.AI;
using UnityEngine;

namespace Larje.Character
{
    public interface IDamageTarget
    {
        GameObject gameObject { get; }
        void SendDamage(DamageData damageData);
    }
}
