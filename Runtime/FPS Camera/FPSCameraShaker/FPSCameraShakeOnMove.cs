using Larje.Core.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;
using Larje.Character.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using Larje.Character.Abilities;
using UnityEngine;

namespace Larje.Character
{
    public class FPSCameraShakeOnMove : MonoBehaviour
    {
        private const string Walk = "Walk";
        private const string Run = "Run";
        private const string Crouch = "Crouch";
        private const string Crawl = "Crawl";
        private const string Slide = "Slide";

        [SerializeField] private FPSCameraShakeAnimation landingAnim;
        [SerializeField] private FPSCameraShakeAnimation walkAnim;
        [SerializeField] private FPSCameraShakeAnimation runAnim;
        [SerializeField] private FPSCameraShakeAnimation crawlingAnim;
        [SerializeField] private FPSCameraShakeAnimation crouchAnim;
        [SerializeField] private FPSCameraShakeAnimation slidingAnim;
        [Space] 
        [SerializeField] private float groundedTargetSpeed = 40f;

        private bool _canPlayGroundAnim = true;

        private Intensity _currenIntensity;

        private Dictionary<string, Intensity> _intensities = new Dictionary<string, Intensity>();

        private FPSCameraShaker _shaker;

        private CharacterPhysics _physics;
        private CharacterJump _jump;
        private CharacterWalk _walk;
        private CharacterRun _run;
        private CharacterCrouch _crouch;

        private void Start()
        {
            _physics = GetComponentInParent<CharacterPhysics>();
            _shaker = GetComponent<FPSCameraShaker>();

            GrabMovement();
            GrabJump();
            GrabCrouch();
        }

        private void FixedUpdate()
        {
            _currenIntensity = _intensities[Walk];

            if (_run.Running)
            {
                _currenIntensity = _intensities[Run];
            }
            else if (_crouch.Crouching)
            {
                if (_crouch.Sliding)
                {
                    _currenIntensity = _intensities[Slide];
                }
                else if (_physics.HorizontalSpeed > 0f)
                {
                    _currenIntensity = _intensities[Crawl];   
                }
                else
                {
                    _currenIntensity = _intensities[Crouch];
                }
            }

            _intensities.Values.ToList().ForEach(x =>
            {
                x.isActive = x == _currenIntensity;
                x.Update(Time.fixedDeltaTime);
            });
        }

        private void GrabMovement()
        {
            _walk = GetComponentInParent<CharacterWalk>();
            _run = GetComponentInParent<CharacterRun>();

            _intensities.Add(Walk, new Intensity(() => _walk.SpeedPercent));
            FPSCameraShaker.Shake walkShake = new FPSCameraShaker.Shake(walkAnim, true,
                () => _physics.HorizontalSpeed, () => _intensities[Walk].Value);
            _shaker.AddShake(walkShake);

            _intensities.Add(Run, new Intensity(() => _walk.SpeedPercent));
            FPSCameraShaker.Shake runShake = new FPSCameraShaker.Shake(runAnim, true,
                () => _physics.HorizontalSpeed, () => _intensities[Run].Value);
            _shaker.AddShake(runShake);
        }

        private void GrabJump()
        {
            _physics.EventGrounded += () =>
            {
                if (_canPlayGroundAnim)
                {
                    _canPlayGroundAnim = false;
                    float intensity = Mathf.Clamp01(_physics.VerticalSpeed / groundedTargetSpeed);
                    FPSCameraShaker.Shake groundShake = new FPSCameraShaker.Shake(landingAnim, false,
                        () => 1f, () => intensity);
                    _shaker.AddShake(groundShake);

                    UnlockGroundAnimAsync(groundShake.Animation.Duration, this.GetCancellationTokenOnDestroy()).Forget();
                }
            };
        }

        private async UniTaskVoid UnlockGroundAnimAsync(float delay, CancellationToken cancellationToken)
        {
            bool cancelled = await Tween.Delay(delay, false, cancellationToken).SuppressCancellationThrow();
            if (!cancelled)
            {
                _canPlayGroundAnim = true;
            }
        }

        private void GrabCrouch()
        {
            _crouch = GetComponentInParent<CharacterCrouch>();
            if (_crouch != null)
            {
                _intensities.Add(Crouch, new Intensity());
                FPSCameraShaker.Shake crouchShake = new FPSCameraShaker.Shake(crouchAnim, true,
                    () => 1f, () => _intensities[Crouch].Value);
                _shaker.AddShake(crouchShake);

                _intensities.Add(Crawl, new Intensity(() => _walk.SpeedPercent));
                FPSCameraShaker.Shake crawlingShake = new FPSCameraShaker.Shake(crawlingAnim, true,
                    () => _physics.HorizontalSpeed, () => _intensities[Crawl].Value);
                _shaker.AddShake(crawlingShake);

                _intensities.Add(Slide, new Intensity());
                FPSCameraShaker.Shake slidingShake = new FPSCameraShaker.Shake(slidingAnim, true,
                    () => _physics.HorizontalSpeed, () => _intensities[Slide].Value);
                _shaker.AddShake(slidingShake);
            }
        }

        private class Intensity
        {
            public bool isActive;

            private float _currentValue;
            private Func<float> _targetValue;

            public float Value
            {
                get => _currentValue;
            }

            public Intensity(Func<float> value = null)
            {
                _targetValue = value ?? (() => 1f);
            }

            public void Update(float deltaTime)
            {
                _currentValue = Mathf.Lerp(_currentValue, isActive ? _targetValue() : 0f, deltaTime * 5f);
            }
        }
    }
}
