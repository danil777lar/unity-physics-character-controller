using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Larje.Core;
using Larje.Core.Input;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Larje.Character
{
    public sealed class CursorService : ICursorService, IInitializableService, IDisposable
    {
        private float _deltaTime;
        private Camera _camera;
        private Vector2 _rotation;
        private Vector2 _delta;
        private InputAction _look;

        private ICursorCamera _lastCamera;
        private readonly CursorConfig _config;
        private readonly IInputService _input;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private readonly List<Func<bool>> _visibility = new List<Func<bool>>();
        private readonly List<ICursorCamera> _cameras = new List<ICursorCamera>();
        private readonly Dictionary<Action<float>, CursorCallbackTiming> _listeners = new Dictionary<Action<float>, CursorCallbackTiming>();
        private readonly List<Action<float>> _snapshot = new List<Action<float>>();

        public bool StaticCursor => TryGetCurrentCamera(out ICursorCamera camera) && camera.StaticCursor;
        public bool CursorVisible => _visibility.TrueForAll(x => x());
        public Vector3 Origin => MainCamera != null ? MainCamera.transform.position : Vector3.zero;
        public Vector3 Direction { get; private set; } = Vector3.forward;
        public Vector2 ScreenPosition { get; private set; }
        public Ray Ray => new Ray(Origin, Direction);

        private Camera MainCamera
        {
            get
            {
                if (_camera == null)
                {
                    _camera = Camera.main;
                }

                return _camera;
            }
        }

        public CursorService(CursorConfig config, IInputService input)
        {
            _config = config;
            _input = input;
        }

        public void Initialize()
        {
            _look = _input.Find(_config.LookAction);
            CinemachineCore.CameraUpdatedEvent.AddListener(OnCinemachineUpdated);
            ServiceLoop.Run(ReadInput, _lifetime.Token, PlayerLoopTiming.Update);
            switch (_config.UpdateType)
            {
                case CursorUpdateType.Fixed:
                    RunFixedAsync(_lifetime.Token).Forget();
                    break;
                case CursorUpdateType.Late:
                    ServiceLoop.Run(UpdateCursor, _lifetime.Token, PlayerLoopTiming.PreLateUpdate);
                    break;
                default:
                    ServiceLoop.Run(UpdateCursor, _lifetime.Token, PlayerLoopTiming.Update);
                    break;
            }
        }

        public void Dispose()
        {
            CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCinemachineUpdated);
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        public void ForceSetDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.0001f || !TryGetCurrentCamera(out ICursorCamera camera))
            {
                return;
            }

            direction.Normalize();
            Vector3 euler = Quaternion.FromToRotation(camera.DefaultDirection, direction).eulerAngles;
            _rotation = new Vector2(euler.x, euler.y);
            Direction = direction;
        }

        public void AddVisibilityCondition(Func<bool> isVisible)
        {
            if (!_visibility.Contains(isVisible))
            {
                _visibility.Add(isVisible);
            }
        }

        public void RemoveVisibilityCondition(Func<bool> isVisible)
        {
            _visibility.Remove(isVisible);
        }

        public void AddCamera(ICursorCamera camera)
        {
            if (!_cameras.Contains(camera))
            {
                _cameras.Add(camera);
            }
        }

        public void RemoveCamera(ICursorCamera camera)
        {
            _cameras.Remove(camera);
        }

        public void AddUpdatedListener(Action<float> listener, CursorCallbackTiming timing)
        {
            _listeners[listener] = timing;
        }

        public void RemoveUpdatedListener(Action<float> listener)
        {
            _listeners.Remove(listener);
        }

        public bool IsCurrent(ICursorCamera camera)
        {
            return TryGetCurrentCamera(out ICursorCamera current) && current == camera;
        }

        public bool TryGetCurrentCamera(out ICursorCamera camera)
        {
            camera = null;
            foreach (ICursorCamera candidate in _cameras)
            {
                if (camera == null || candidate.Priority >= camera.Priority)
                {
                    camera = candidate;
                }
            }

            return camera != null;
        }

        private async UniTaskVoid RunFixedAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                bool cancelled = await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationToken).SuppressCancellationThrow();
                if (cancelled)
                {
                    return;
                }

                UpdateCursor(Time.fixedDeltaTime);
            }
        }

        private void ReadInput(float deltaTime)
        {
            if (_look != null && TryGetCurrentCamera(out ICursorCamera camera) && camera.Permitted)
            {
                _delta += _look.ReadValue<Vector2>();
            }
        }

        private void UpdateCursor(float deltaTime)
        {
            _deltaTime = deltaTime;
            Notify(CursorCallbackTiming.BeforeUpdate);
            if (!TryGetCurrentCamera(out ICursorCamera camera))
            {
                return;
            }

            if (_lastCamera != null && _lastCamera != camera)
            {
                _delta = Vector2.zero;
                _rotation = Vector2.zero;
                camera.Select();
                Direction = camera.DefaultDirection;
            }
            else
            {
                Vector2 delta = _config.UseSmooth ? _delta * (deltaTime * _config.RotationSharpness) : _delta;
                _delta -= delta;
                _rotation.y = RotateAxis(_rotation.y, delta.x, camera.RotationLimitMin.y, camera.RotationLimitMax.y);
                _rotation.x = RotateAxis(_rotation.x, -delta.y, camera.RotationLimitMin.x, camera.RotationLimitMax.x);

                Vector3 direction = Quaternion.AngleAxis(_rotation.y, Vector3.up) * camera.DefaultDirection;
                Direction = Quaternion.AngleAxis(-_rotation.x, Vector3.Cross(direction, Vector3.up)) * direction;
            }

            _lastCamera = camera;
            Notify(CursorCallbackTiming.AfterUpdate);
        }

        private void OnCinemachineUpdated(CinemachineBrain brain)
        {
            if (MainCamera != null)
            {
                ScreenPosition = MainCamera.WorldToScreenPoint(MainCamera.transform.position + Direction);
            }

            Notify(CursorCallbackTiming.AfterCinemachine);
        }

        private void Notify(CursorCallbackTiming timing)
        {
            _snapshot.Clear();
            foreach (KeyValuePair<Action<float>, CursorCallbackTiming> pair in _listeners)
            {
                if (pair.Value == timing)
                {
                    _snapshot.Add(pair.Key);
                }
            }

            foreach (Action<float> listener in _snapshot)
            {
                listener(_deltaTime);
            }
        }

        private float RotateAxis(float current, float delta, float min, float max)
        {
            float bound = current < 0f ? min : max;
            float distance = Mathf.Abs(bound);
            float percent = distance > 0f ? Mathf.Abs(current / distance) : 0f;
            float zone = _config.SensitivityDecreaseZone;
            float decrease = 1f - Mathf.Clamp01(percent - zone) / Mathf.Max(0.0001f, 1f - zone);
            bool approaching = Mathf.Abs(current - bound) > Mathf.Abs(current + delta - bound);
            float sensitivity = approaching ? _config.Sensitivity * decrease : _config.Sensitivity;

            float result = current + delta * sensitivity;
            if (result > 180f)
            {
                result -= 360f;
            }
            else if (result < -180f)
            {
                result += 360f;
            }

            return Mathf.Clamp(result, min, max);
        }
    }
}
