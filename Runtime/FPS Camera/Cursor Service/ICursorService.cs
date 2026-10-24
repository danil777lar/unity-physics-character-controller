using System;
using UnityEngine;

namespace Larje.Character
{
    public interface ICursorService
    {
        bool StaticCursor { get; }
        bool CursorVisible { get; }
        Vector3 Origin { get; }
        Vector3 Direction { get; }
        Vector2 ScreenPosition { get; }
        Ray Ray { get; }

        void ForceSetDirection(Vector3 direction);
        void AddVisibilityCondition(Func<bool> isVisible);
        void RemoveVisibilityCondition(Func<bool> isVisible);
        void AddCamera(ICursorCamera camera);
        void RemoveCamera(ICursorCamera camera);
        void AddUpdatedListener(Action<float> listener, CursorCallbackTiming timing);
        void RemoveUpdatedListener(Action<float> listener);
        bool IsCurrent(ICursorCamera camera);
        bool TryGetCurrentCamera(out ICursorCamera camera);
    }
}
