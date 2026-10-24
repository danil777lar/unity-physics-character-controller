using Larje.Character;
using Larje.Character.AI;
using UnityEngine;

namespace Larje.Character.AI
{
    public class AIActionSetBrain : AIAction
    {
        [Space] 
        [SerializeField] private string brainKey;
        [SerializeField] private AIBrain brainPrefab;

        protected override void OnEnterState()
        {
            AIBrainLoader brainLoader = GetComponentInParent<AIBrainLoader>();
            if (brainLoader != null)
            {
                if (brainPrefab != null)
                {
                    brainLoader.SetBrain(brainPrefab);
                    return;
                }

#if LARJE_ADDRESSABLES
                if (!string.IsNullOrEmpty(brainKey))
                {
                    brainLoader.SetBrain(brainKey);
                    return;
                }
#endif
            }
        }

        public override void PerformAction()
        {
        }
    }
}
