using Larje.Character.AI;
using System.Collections.Generic;
using UnityEngine;

namespace Larje.Character
{
    public interface ICursorCamera
    {
        public bool Permitted { get; }
        public bool StaticCursor { get; }
        public int Priority { get; }

        public Vector3 DefaultDirection { get; }

        public Vector2 RotationLimitMin { get; }
        public Vector2 RotationLimitMax { get; }

        public void Select();
    }
}
