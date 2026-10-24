using Larje.Character.Abilities;
using Larje.Character;
using Larje.Core.Audio;
using UnityEngine;

namespace Larje.Character.Sounds
{
    public sealed class CharacterStepSound : MonoBehaviour
    {
        [SerializeField] private float stepDistance = 0.25f;
        [SerializeField] private SoundId stepSound;

        private float _distance;
        private Vector3 _lastPosition;
        private Character _character;
        private CharacterPhysics _physics;
        private CharacterWalk _walk;
        private ISoundService _sounds;

#if LARJE_ZENJECT
        [Zenject.Inject]
#endif
        public void Construct(ISoundService sounds)
        {
            _sounds = sounds;
        }

        private void Start()
        {
            _lastPosition = transform.position;
            _character = GetComponentInParent<Character>();
            _physics = GetComponentInParent<CharacterPhysics>();
            _walk = _character != null ? _character.FindAbility<CharacterWalk>() : null;
        }

        private void Update()
        {
            if (_character == null || !_character.IsActive || !_character.IsAlive || !_physics.Grounded)
            {
                return;
            }

            _distance += Vector3.Distance(transform.position, _lastPosition);
            _lastPosition = transform.position;
            if (_distance >= stepDistance)
            {
                _distance = 0f;
                PlayStep();
            }
        }

        private void PlayStep()
        {
            if (_sounds == null || stepSound == null)
            {
                return;
            }

            Sound sound = _sounds.Play(stepSound);
            if (sound != null)
            {
                sound.SetVolume(_walk != null ? _walk.SpeedPercent : 1f).SetPosition(transform.position);
            }
        }
    }
}
