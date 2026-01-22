using UnityEngine;

public class PlayerMoveState : PlayerFlyingState
{
    public PlayerMoveState(SGC2025.PlayerCharacter player, StateMachine stateMachine, string stateName) : base(player, stateMachine, stateName)
    {
    }

    private Vector2 playerLimitPos;

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
        }
        
        // 移動制限（Inspector設定値を使用、GroundManagerがあればそちらを優先）
        Vector2 limitHigh = player.positionLimitHigh;
        Vector2 limitLow = player.positionLimitLow;
        
        // GroundManagerが初期化されていれば、マップサイズから移動範囲を計算
        if (SGC2025.GroundManager.I != null && SGC2025.GroundManager.I.MapData != null)
        {
            var mapData = SGC2025.GroundManager.I.MapData;
            // マップのワールド座標から少し余白を持たせて移動範囲を計算
            float margin = 0.5f;
            limitHigh = new Vector2(
                mapData.MapMaxWorldPosition.x - margin,
                mapData.MapMaxWorldPosition.y - margin
            );
            limitLow = new Vector2(margin, margin);
        }
        
        if(limitHigh.x < player.transform.position.x || limitHigh.y < player.transform.position.y
            || limitLow.x > player.transform.position.x || limitLow.y > player.transform.position.y)
        {
            playerLimitPos = player.transform.position;
            if(limitHigh.x < player.transform.position.x)
            {
                playerLimitPos = new Vector2(limitHigh.x, playerLimitPos.y);
            }
            if(limitHigh.y < player.transform.position.y)
            {
                playerLimitPos = new Vector2(playerLimitPos.x, limitHigh.y);
            }
            if(limitLow.x > player.transform.position.x)
            {
                playerLimitPos = new Vector2(limitLow.x, playerLimitPos.y);
            }
            if(limitLow.y > player.transform.position.y)
            {
                playerLimitPos = new Vector2(playerLimitPos.x, limitLow.y);
            }

            player.transform.position = playerLimitPos;
        }
        else
        {
            player.SetVelocity(player.moveInput.x, player.moveInput.y);

        }





    }
}
