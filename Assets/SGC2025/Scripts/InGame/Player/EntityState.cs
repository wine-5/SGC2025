using UnityEngine;

namespace SGC2025.Player
{
    /// <summary>
    /// エンティティ状態のベースクラス
    /// </summary>
    public abstract class EntityState
    {
        protected PlayerCharacter player;
        protected StateMachine stateMachine;
        protected string animBoolName;

        protected Animator anim;

        protected EntityState(PlayerCharacter player, StateMachine stateMachine, string animBoolName)
        {
            this.player = player;
            this.stateMachine = stateMachine;
            this.animBoolName = animBoolName;

            anim = player.anim;
        }

        public virtual void Enter()
        {
            anim.SetBool(animBoolName, true);
        }

        public virtual void Update()
        {
        }

        public virtual void Exit()
        {
            anim.SetBool(animBoolName, false);
        }
    }
}
