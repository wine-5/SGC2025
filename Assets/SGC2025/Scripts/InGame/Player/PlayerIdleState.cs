using UnityEngine;

namespace SGC2025.Player
{
    public class PlayerIdleState : PlayerFlyingState
    {
        public PlayerIdleState(PlayerCharacter player, StateMachine stateMachine, string stateName) : base(player, stateMachine, stateName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        player.SetVelocity(0, 0);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();


        if (player.moveInput.x != 0 || player.moveInput.y != 0)
        {
            stateMachine.ChangeState(player.moveState);
        }

        
    }
    }
}
