using UnityEngine;

namespace SGC2025.Player
{
    /// <summary>
    /// Playerの飛行状態の基底クラス
    /// </summary>
    public class PlayerFlyingState : EntityState
    {
        public PlayerFlyingState(PlayerCharacter player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
        {
        }
    }
}
