using Larje.Character.AI;
using UnityEngine;

namespace Larje.Character
{
    public partial class DamageType
    {
        public static DamageType Physical => new DamageType("physical");
        public static DamageType Fire => new DamageType("fire");
    }
}
