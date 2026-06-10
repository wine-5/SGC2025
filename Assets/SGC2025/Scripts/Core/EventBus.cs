using System;
using System.Collections.Generic;
using UnityEngine;

namespace SGC2025.Core
{
    /// <summary>
    /// 型安全なイベントバスシステム
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> eventHandlers = new Dictionary<Type, List<Delegate>>();

        #region Subscribe

        /// <summary>
        /// イベントを購読する
        /// </summary>
        public static void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            if (handler == null) return;

            Type eventType = typeof(T);

            if (!eventHandlers.ContainsKey(eventType))
                eventHandlers[eventType] = new List<Delegate>();

            eventHandlers[eventType].Add(handler);
        }

        #endregion

        #region Unsubscribe

        /// <summary>
        /// イベントの購読を解除する
        /// </summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            if (handler == null) return;

            Type eventType = typeof(T);

            if (!eventHandlers.ContainsKey(eventType)) return;

            eventHandlers[eventType].Remove(handler);

            if (eventHandlers[eventType].Count == 0)
                eventHandlers.Remove(eventType);
        }

        #endregion

        #region Publish

        /// <summary>
        /// イベントを発行する
        /// </summary>
        public static void Publish<T>(T eventData) where T : IGameEvent
        {
            Type eventType = typeof(T);

            if (!eventHandlers.ContainsKey(eventType)) return;

            var handlers = new List<Delegate>(eventHandlers[eventType]);
            foreach (var handler in handlers)
            {
                try
                {
                    (handler as Action<T>)?.Invoke(eventData);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[EventBus] ハンドラーでエラーが発生しました: {eventType.Name}\n{ex}");
                }
            }
        }

        #endregion
    }
}
