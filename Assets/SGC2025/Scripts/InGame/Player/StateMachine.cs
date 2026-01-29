using UnityEngine;

namespace SGC2025.Player
{
    /// <summary>
    /// エンティティの状態遷移管理
    /// </summary>
    public class StateMachine
    {
        public EntityState CurrentState { get; private set; }

        /// <summary>
        /// 初期状態を設定
        /// </summary>
        public void Initialize(EntityState startState)
        {
            CurrentState = startState;
            CurrentState.Enter();
        }

        /// <summary>
        /// 状態を変更
        /// </summary>
        public void ChangeState(EntityState newState)
        {
            CurrentState.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }

        /// <summary>
        /// アクティブ状態を更新
        /// </summary>
        public void UpdateActiveState()
        {
            CurrentState.Update();
        }
    }
}
