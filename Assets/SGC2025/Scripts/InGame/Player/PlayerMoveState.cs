using UnityEngine;
using SGC2025.Manager;

namespace SGC2025.Player
{
    /// <summary>
    /// Playerの移動状態
    /// </summary>
    public class PlayerMoveState : PlayerFlyingState
    {
        private const float BOUNDARY_MARGIN = 0.5f;

        private Vector2 playerLimitPos;

        public PlayerMoveState(PlayerCharacter player, StateMachine stateMachine, string stateName) : base(player, stateMachine, stateName)
        {
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Exit()
        {
            base.Exit();
        }

        public override void Update()
        {
            base.Update();

            if (player.moveInput.x == 0 && player.moveInput.y == 0)
            {
                stateMachine.ChangeState(player.idleState);
                return;
            }
            
            if (GroundManager.I == null || GroundManager.I.MapData == null)
            {
                player.SetVelocity(player.moveInput.x, player.moveInput.y);
                return;
            }

            var mapData = GroundManager.I.MapData;
            Vector2 limitHigh = new Vector2(
                mapData.MapMaxWorldPosition.x - BOUNDARY_MARGIN,
                mapData.MapMaxWorldPosition.y - BOUNDARY_MARGIN
            );
            Vector2 limitLow = new Vector2(BOUNDARY_MARGIN, BOUNDARY_MARGIN);
            
            Vector3 currentPos = player.transform.position;
            bool isOutOfBounds = limitHigh.x < currentPos.x || limitHigh.y < currentPos.y ||
                                 limitLow.x > currentPos.x || limitLow.y > currentPos.y;
            
            if (isOutOfBounds)
            {
                playerLimitPos = currentPos;
                if (limitHigh.x < currentPos.x)
                    playerLimitPos = new Vector2(limitHigh.x, playerLimitPos.y);
                if (limitHigh.y < currentPos.y)
                    playerLimitPos = new Vector2(playerLimitPos.x, limitHigh.y);
                if (limitLow.x > currentPos.x)
                    playerLimitPos = new Vector2(limitLow.x, playerLimitPos.y);
                if (limitLow.y > currentPos.y)
                    playerLimitPos = new Vector2(playerLimitPos.x, limitLow.y);
                player.transform.position = playerLimitPos;
            }
            else
            {
                player.SetVelocity(player.moveInput.x, player.moveInput.y);
            }
        }
    }
}
