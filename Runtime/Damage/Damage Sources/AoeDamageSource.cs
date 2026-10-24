using Larje.Character.AI;
using System.Collections.Generic;
using UnityEngine;

namespace Larje.Character
{
    public abstract class AoeDamageSource : MonoBehaviour 
    {
        private List<IDamageTarget> targetBlackList;

        protected IReadOnlyCollection<IDamageTarget> TargetBlackList => targetBlackList;

        public void SetBlackList(List<IDamageTarget> blackList)
        {
            targetBlackList = blackList;
        }
    }
}
