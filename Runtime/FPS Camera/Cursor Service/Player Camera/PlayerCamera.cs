using Larje.Character.AI;
using System;
using System.Collections.Generic;
using Larje.Core;
using Larje.Core.CompositeProperties;
using Unity.Cinemachine;
using UnityEngine;

namespace Larje.Character
{
    public class PlayerCamera : MonoBehaviour, ICursorCamera
    {
        [SerializeField] public BoolComposite permitted = new BoolComposite();
        [SerializeField] private bool resetDirectionOnSelect = false;

        [Header("Limits")]
        [SerializeField] private bool staticCursor = false;
        [SerializeField] private Vector2 limitMin = new Vector2(-90f, -90f);
        [SerializeField] private Vector2 limitMax = new Vector2(90f, 90f);

        [Header("Sensitivity")]
        [SerializeField] private bool useDecreaseSensZone = true;
        [SerializeField] private float decreaseSensZoneMin = 50f;
        [SerializeField] private float decreaseSensZoneMax = 100f;
        [SerializeField] private AnimationCurve speedCurve;

        [Header("Links")]
        [SerializeField] private CinemachineCamera virtualCamera;
        [SerializeField] private List<Axis> axes;


        private ICursorService _cursorService;

        public bool Permitted => permitted.Value;
        public bool StaticCursor => staticCursor;
        public int Priority => virtualCamera.Priority;
        public bool IsCurrent => _cursorService.IsCurrent(this);
        public Vector3 DefaultDirection { get; private set; }
        public Vector3 CurrentDirection => virtualCamera.transform.forward;
        public Vector2 RotationLimitMin => limitMin;
        public Vector2 RotationLimitMax => limitMax;

#if LARJE_ZENJECT
        [Zenject.Inject]
#endif
        public void Construct(ICursorService cursorService)
        {
            _cursorService = cursorService;
        }

        public void Select()
        {
            if (resetDirectionOnSelect)
            {
                DefaultDirection = virtualCamera.transform.forward;
            }
            ForceSetDirection(DefaultDirection);
        }

        private void Start()
        {
            DefaultDirection = virtualCamera.transform.forward;

            _cursorService.AddCamera(this);
            _cursorService.AddUpdatedListener(UpdateAxisRotations, CursorCallbackTiming.AfterUpdate);
        }

        private void OnDestroy()
        {
            if (_cursorService == null) return;

            _cursorService.RemoveCamera(this);
            _cursorService.RemoveUpdatedListener(UpdateAxisRotations);
        }

        private void UpdateAxisRotations(float deltaTime)
        {
            if (IsCurrent)
            {
                axes.ForEach(x => RotateAxis(x, deltaTime));
            }
        }

        private void ForceSetDirection(Vector3 direction)
        {
            axes.ForEach(x => ForceSetDirectionAxis(x, direction));
        }

        private void ForceSetDirectionAxis(Axis axis, Vector3 direction)
        {
            Quaternion from = Quaternion.LookRotation(axis.AxisTransform.forward);
            Vector3 eulerAngles = Quaternion.LookRotation(direction).eulerAngles;
            eulerAngles.x = Mathf.Lerp(from.eulerAngles.x, eulerAngles.x, axis.Mask.x);
            eulerAngles.y = Mathf.Lerp(from.eulerAngles.y, eulerAngles.y, axis.Mask.y);
            eulerAngles.z = Mathf.Lerp(from.eulerAngles.z, eulerAngles.z, axis.Mask.z);

            axis.AxisTransform.rotation = Quaternion.Euler(eulerAngles);
            axis.AxisTransform.localRotation = Quaternion.Euler(Vector3.Scale(axis.AxisTransform.localRotation.eulerAngles, axis.Scale));
        }

        private void RotateAxis(Axis axis, float deltaTime)
        {
            Quaternion from = Quaternion.LookRotation(axis.AxisTransform.forward);
            Quaternion to = Quaternion.LookRotation(_cursorService.Direction);

            float targetSpeed = speedCurve.Evaluate(1f);
            if (useDecreaseSensZone)
            {
                float angle = Mathf.Max(Quaternion.Angle(from, to) - decreaseSensZoneMin, 0f);
                float t = angle / (decreaseSensZoneMax - decreaseSensZoneMin);
                targetSpeed = angle < decreaseSensZoneMin ? 0f : speedCurve.Evaluate(t);
            }

            Vector3 eulerAngles = Quaternion.Lerp(from, to, deltaTime * targetSpeed).eulerAngles;
            eulerAngles.x = Mathf.Lerp(from.eulerAngles.x, eulerAngles.x, axis.Mask.x);
            eulerAngles.y = Mathf.Lerp(from.eulerAngles.y, eulerAngles.y, axis.Mask.y);
            eulerAngles.z = Mathf.Lerp(from.eulerAngles.z, eulerAngles.z, axis.Mask.z);

            axis.AxisTransform.rotation = Quaternion.Euler(eulerAngles);
            axis.AxisTransform.localRotation = Quaternion.Euler(Vector3.Scale(axis.AxisTransform.localRotation.eulerAngles, axis.Scale));
        }

        [Serializable]
        private class Axis
        {
            [field: SerializeField] public Transform AxisTransform { get; private set; }
            [field: SerializeField] public Vector3Int Mask { get; private set; }
            [field: SerializeField] public Vector3Int Scale { get; private set; } = Vector3Int.one;
        }
    }
}
