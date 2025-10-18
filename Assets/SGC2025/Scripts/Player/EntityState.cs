using Unity.VisualScripting;
using UnityEngine;

public abstract class EntityState
{
    protected Player player;
    protected StateMachine stateMachine;
    protected string animBoolName;

    protected Animator anim;
    protected Rigidbody rb;
    protected PlayerInputSet input;

    public EntityState(Player player, StateMachine stateMachine, string animBoolName)
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
        //animator‚ð‚Â‚¯‚Ä‚©‚ç—LŒø‰»‚·‚é
        anim.SetBool(animBoolName, true);



        //Debug.Log("Enter " + animBoolName);
    }

    public virtual void Update()
    {
        //Debug.Log("Update " + animBoolName);
    }

    public virtual void Exit()
    {
        anim.SetBool(animBoolName, false);



        //Debug.Log("Exit " + animBoolName);
    }
}
