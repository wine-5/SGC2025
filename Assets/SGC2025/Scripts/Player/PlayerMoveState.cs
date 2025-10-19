using UnityEngine;

public class PlayerMoveState : PlayerFlyingState
{
    public PlayerMoveState(PlayerCharacter player, StateMachine stateMachine, string stateName) : base(player, stateMachine, stateName)
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
        
        //à⁄ìÆêßå¿
        if(player.positionLimitHigh.x < player.transform.position.x || player.positionLimitHigh.y < player.transform.position.y
            || player.positionLimitLow.x > player.transform.position.x || player.positionLimitLow.y > player.transform.position.y)
        {
            playerLimitPos = player.transform.position;
            if(player.positionLimitHigh.x < player.transform.position.x)
            {
                playerLimitPos = new Vector2(player.positionLimitHigh.x, playerLimitPos.y);
            }
            if(player.positionLimitHigh.y < player.transform.position.y)
            {
                playerLimitPos = new Vector2(playerLimitPos.x, player.positionLimitHigh.y);
            }
            if(player.positionLimitLow.x > player.transform.position.x)
            {
                playerLimitPos = new Vector2(player.positionLimitLow.x, playerLimitPos.y);
            }
            if(player.positionLimitLow.y > player.transform.position.y)
            {
                playerLimitPos = new Vector2(playerLimitPos.x, player.positionLimitLow.y);
            }

            player.transform.position = playerLimitPos;
        }
        else
        {
            player.SetVelocity(player.moveInput.x, player.moveInput.y);

        }





    }
}
