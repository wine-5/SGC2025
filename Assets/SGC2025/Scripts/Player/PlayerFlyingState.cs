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




        //’e”­ŽË
        if (input.Player.Shot.WasPerformedThisFrame())
        {

            //’e‚Ì”­ŽË
            PlayerShot();

        }
    }

    private void PlayerShot()
    {
        Debug.Log("Shot");
    }
}
