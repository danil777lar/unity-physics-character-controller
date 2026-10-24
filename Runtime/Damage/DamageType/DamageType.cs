using Larje.Character.AI;
using System;using UnityEngine;

namespace Larje.Character
{
    public partial class DamageType
    {
        private readonly string value;

        private DamageType(string v)
        {
            value = v;
        }

        public static bool operator ==(DamageType a, DamageType b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return a.value == b.value;
        }

        public static bool operator !=(DamageType a, DamageType b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj is DamageType other)
            {
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(value);
        }
    }
}
