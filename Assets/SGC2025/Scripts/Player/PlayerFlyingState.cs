using System;
using UnityEngine;

public class PlayerFlyingState : EntityState
{
    public PlayerFlyingState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
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
        Debug.Log("Shot");
    }
}
