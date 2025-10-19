using Unity.VisualScripting;
using UnityEngine;

public abstract class EntityState
{
    protected PlayerCharacter player;
    protected StateMachine stateMachine;
    protected string animBoolName;

    protected Animator anim;
    protected Rigidbody rb;
    protected PlayerInputSet input;

    public EntityState(PlayerCharacter player, StateMachine stateMachine, string animBoolName)
    {
        this.player = player;
        this.stateMachine = stateMachine;
        this.animBoolName = animBoolName;

        anim = player.anim;
        rb = player.rb;
        input = player.input;
    }

    public virtual void Enter()
    {
        //ステートに移行した際、初めに実行される処理

        //Animatorのbool値を変更する
        anim.SetBool(animBoolName, true);



        //Debug.Log("Enter " + animBoolName);
    }

    public virtual void Update()
    {
        //ステートに移行した際、実行され続ける


        //Debug.Log("Update " + animBoolName);
    }

    public virtual void Exit()
    {
        //ステートから離れる際に実行される処理

        anim.SetBool(animBoolName, false);

        //Debug.Log("Exit " + animBoolName);
    }
}
