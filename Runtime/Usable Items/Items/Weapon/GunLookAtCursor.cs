using Larje.Character.AI;
using System;
using Larje.Core;
using UnityEngine;

namespace Larje.Character
{
    public class GunLookAtCursor : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;

        private ICursorService _cursorService;

#if LARJE_ZENJECT
        [Zenject.Inject]
#endif
        public void Construct(ICursorService cursorService)
        {
            _cursorService = cursorService;
        }

        private void Start()
        {
            _cursorService.AddUpdatedListener(OnCursorUpdated, CursorCallbackTiming.AfterUpdate);
        }

        private void OnDestroy()
        {
            if (_cursorService != null)
            {
                _cursorService.RemoveUpdatedListener(OnCursorUpdated);
            }
        }

        private void OnCursorUpdated(float deltaTime)
        {
            transform.forward = Vector3.Lerp(transform.forward, _cursorService.Direction, deltaTime * speed);
        }
    }
}
