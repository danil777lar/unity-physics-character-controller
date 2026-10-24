using Larje.Character;
using System.Collections.Generic;
using UnityEngine;

namespace Larje.Character.AI
{
    [System.Serializable]
    public class AITransition 
    {
        public AIDecision Decision;
        public string TrueState;
        public string FalseState;
    }
}