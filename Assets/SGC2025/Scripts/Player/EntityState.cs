using Unity.VisualScripting;
using UnityEngine;

public abstract class EntityState
{
    protected SGC2025.PlayerCharacter player;
    protected StateMachine stateMachine;
    protected string animBoolName;

    protected Animator anim;
    protected Rigidbody rb;
    protected PlayerInputSet input;

    public EntityState(SGC2025.PlayerCharacter player, StateMachine stateMachine, string animBoolName)
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
        //�X�e�[�g�Ɉڍs�����ہA���߂Ɏ��s����鏈��

        //Animator��bool�l��ύX����
        anim.SetBool(animBoolName, true);



        //Debug.Log("Enter " + animBoolName);
    }

    public virtual void Update()
    {
        //�X�e�[�g�Ɉڍs�����ہA���s���ꑱ����


        //Debug.Log("Update " + animBoolName);
    }

    public virtual void Exit()
    {
        //�X�e�[�g���痣���ۂɎ��s����鏈��

        anim.SetBool(animBoolName, false);

        //Debug.Log("Exit " + animBoolName);
    }
}
