using Larje.Character.Abilities;
using UnityEngine;

namespace Larje.Character.AI
{
    public sealed class AIActionAttack : AIAction
    {
        [Header("Shooting")]
        [SerializeField] private float hitChance = 0.2f;
        [SerializeField] private float missRadius = 5f;
        [SerializeField] private float shootDelayMin = 0.5f;
        [SerializeField] private float shootDelayMax = 1.5f;

        [Header("Target")]
        [SerializeField] private TargetSource targetSource = TargetSource.Player;
        [SerializeField] private Character targetCharacter;

        private float _delay;
        private Vector3 _targetPosition;
        private CharacterItemUser _itemUser;
        private IPlayerProviderService _players;

#if LARJE_ZENJECT
        [Zenject.Inject]
#endif
        public void Construct(IPlayerProviderService players)
        {
            _players = players;
        }

        public override void PerformAction()
        {
            if (_delay > 0f)
            {
                _delay -= Time.deltaTime;
                return;
            }

            Character target = ResolveTarget();
            if (target != null && target.IsAlive && _itemUser != null)
            {
                UpdateTargetPosition(target);
                _itemUser.StartAction(0);
                _delay = Random.Range(shootDelayMin, shootDelayMax);
            }
        }

        protected override void OnInitialized()
        {
            if (_itemUser == null)
            {
                _itemUser = Brain.Owner.GetComponent<CharacterItemUser>();
            }
        }

        protected override void OnEnterState()
        {
            if (_itemUser != null)
            {
                _itemUser.TargetPosition.AddValue(GetTargetPosition, () => 10);
            }
        }

        protected override void OnExitState()
        {
            if (_itemUser != null)
            {
                _itemUser.TargetPosition.RemoveValue(GetTargetPosition);
            }
        }

        private Character ResolveTarget()
        {
            if (targetSource == TargetSource.Character)
            {
                return targetCharacter;
            }

            return _players != null && _players.TryGetPlayer(out Character player) ? player : null;
        }

        private void UpdateTargetPosition(Character target)
        {
            CharacterPhysics physics = target.GetComponent<CharacterPhysics>();
            Vector3 center = physics != null ? physics.transform.TransformPoint(physics.ColliderCenter) : target.transform.position;
            _targetPosition = Random.value < hitChance ? center : center + Random.insideUnitSphere * missRadius;
        }

        private Vector3 GetTargetPosition()
        {
            return _targetPosition;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_targetPosition, 0.25f);
        }

        private enum TargetSource
        {
            Player,
            Character
        }
    }
}
