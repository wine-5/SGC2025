using UnityEngine;

public class PlayerFlyingState : EntityState
{
    public PlayerFlyingState(SGC2025.PlayerCharacter player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
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




        //�e����
        if (input.Player.Shot.WasPerformedThisFrame())
        {

            //�e�̔���
            PlayerShot();

        }
    }

    private void PlayerShot()
    {
    }
}
