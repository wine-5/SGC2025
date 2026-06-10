using UnityEngine;
using SGC2025.Core;
using SGC2025.Player;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 敵の移動を管理するクラス（plain C#）
    /// EnemyControllerが所有し、Tick()で毎フレーム駆動される
    /// </summary>
    public class EnemyMovement
    {
        private const float DEFAULT_ARRIVE_THRESHOLD = 0.5f;
        private const float OVERSHOOT_MULTIPLIER = 2f;

        private readonly Transform _transform;
        private readonly EnemyController _controller;
        private IMovementStrategy _movementStrategy;
        private Vector3 _moveDirection = Vector3.down;
        private Vector3? _targetPosition = null;
        private readonly float _arriveThreshold = DEFAULT_ARRIVE_THRESHOLD;
        private Vector3 _lastPosition;

        private Transform _playerTransform;
        private bool _playerSearchAttempted = false;

        public EnemyMovement(Transform transform, EnemyController controller)
        {
            _transform = transform;
            _controller = controller;
            _lastPosition = transform.position;
        }

        /// <summary>
        /// 移動戦略を設定（追従型）
        /// </summary>
        public void SetMovementStrategy(IMovementStrategy strategy)
        {
            _movementStrategy = strategy;
            _targetPosition = null;
        }

        /// <summary>
        /// 目標位置を設定（固定方向移動型）
        /// </summary>
        public void SetTargetPosition(Vector3 target)
        {
            _targetPosition = target;
            _movementStrategy = null;
            Vector3 direction = target - _transform.position;
            direction.z = 0f;
            _moveDirection = direction.normalized;
        }

        /// <summary>
        /// 毎フレームの移動処理（EnemyController.Update()から呼ぶ）
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (!_controller.CanMove) return;

            float speed = _controller.MoveSpeed;

            if (_targetPosition.HasValue)
            {
                MoveToFixedTarget(speed, deltaTime);
            }
            else if (_movementStrategy != null)
            {
                Transform player = GetPlayerTransform();
                if (player != null)
                    _movementStrategy.Move(_transform, player, speed, deltaTime);
                else
                {
                    Vector3 movement = _moveDirection * speed * deltaTime;
                    movement.z = 0f;
                    _transform.Translate(movement);
                }
            }
            else
            {
                Vector3 movement = _moveDirection * speed * deltaTime;
                movement.z = 0f;
                _transform.Translate(movement);
            }
        }

        private Transform GetPlayerTransform()
        {
            if (_playerTransform != null) return _playerTransform;
            if (_playerSearchAttempted) return null;

            _playerSearchAttempted = true;
            if (PlayerDataProvider.I != null && PlayerDataProvider.I.IsPlayerRegistered)
            {
                _playerTransform = PlayerDataProvider.I.PlayerTransform;
                return _playerTransform;
            }
            GameObject playerObject = GameObject.FindWithTag(GameLayers.PlayerTag);
            if (playerObject != null)
            {
                _playerTransform = playerObject.transform;
                return _playerTransform;
            }
            return null;
        }

        private void MoveToFixedTarget(float speed, float deltaTime)
        {
            Vector3 movement = _moveDirection * speed * deltaTime;
            movement.z = 0f;

            _lastPosition = _transform.position;
            _transform.position += movement;

            Vector3 currentPos = _transform.position;
            Vector3 targetPos = _targetPosition.Value;
            currentPos.z = 0f;
            targetPos.z = 0f;

            float distanceToTarget = Vector3.Distance(currentPos, targetPos);

            Vector3 lastPos = _lastPosition;
            lastPos.z = 0f;
            float lastDistance = Vector3.Distance(lastPos, targetPos);
            bool overshot = distanceToTarget > lastDistance && lastDistance < _arriveThreshold * OVERSHOOT_MULTIPLIER;

            if (distanceToTarget < _arriveThreshold || overshot)
                _controller.ReturnToPool();
        }
    }
}