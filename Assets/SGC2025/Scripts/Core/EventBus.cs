using System;
using System.Collections.Generic;
using Tyotyo.Core.Log;
using UnityEngine;

namespace Tyotyo.Core
{
    /// <summary>
    /// 型安全なイベントバスシステム
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> eventHandlers = new Dictionary<Type, List<Delegate>>();

        // Publish中の購読変更（Subscribe/Unsubscribe）は、反復中のリスト破壊を避けるため
        // ここへ退避し、最も外側のPublish完了後にまとめて適用する。
        private static int publishDepth;
        private static readonly List<PendingOperation> pendingOperations = new List<PendingOperation>();

        private readonly struct PendingOperation
        {
            public readonly Type EventType;
            public readonly Delegate Handler;
            public readonly bool IsSubscribe;

            public PendingOperation(Type eventType, Delegate handler, bool isSubscribe)
            {
                EventType = eventType;
                Handler = handler;
                IsSubscribe = isSubscribe;
            }
        }

        #region Subscribe

        /// <summary>
        /// イベントを購読する
        /// </summary>
        public static void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            if (handler == null) return;

            Type eventType = typeof(T);

            if (publishDepth > 0)
            {
                pendingOperations.Add(new PendingOperation(eventType, handler, true));
                return;
            }

            AddHandler(eventType, handler);
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

            if (publishDepth > 0)
            {
                pendingOperations.Add(new PendingOperation(eventType, handler, false));
                return;
            }

            RemoveHandler(eventType, handler);
        }

        #endregion

        #region Publish

        /// <summary>
        /// イベントを発行する
        /// </summary>
        public static void Publish<T>(T eventData) where T : IGameEvent
        {
            Type eventType = typeof(T);

            if (!eventHandlers.TryGetValue(eventType, out var handlers)) return;

            // 反復中はリストを変更しない（Subscribe/Unsubscribeはpendingへ退避される）ため、
            // コピーを作らずインデックス走査でゼロアロケーションに発行できる。
            publishDepth++;
            try
            {
                for (int i = 0; i < handlers.Count; i++)
                {
                    try
                    {
                        (handlers[i] as Action<T>)?.Invoke(eventData);
                    }
                    catch (Exception ex)
                    {
                        CusLog.Error("EventBus", $"ハンドラーでエラーが発生しました: {eventType.Name}\n{ex}");
                    }
                }
            }
            finally
            {
                publishDepth--;
                if (publishDepth == 0)
                    FlushPendingOperations();
            }
        }

        #endregion

        #region Internal

        private static void AddHandler(Type eventType, Delegate handler)
        {
            if (!eventHandlers.TryGetValue(eventType, out var list))
            {
                list = new List<Delegate>();
                eventHandlers[eventType] = list;
            }

            list.Add(handler);
        }

        private static void RemoveHandler(Type eventType, Delegate handler)
        {
            if (!eventHandlers.TryGetValue(eventType, out var list)) return;

            list.Remove(handler);

            if (list.Count == 0)
                eventHandlers.Remove(eventType);
        }

        private static void FlushPendingOperations()
        {
            if (pendingOperations.Count == 0) return;

            for (int i = 0; i < pendingOperations.Count; i++)
            {
                var op = pendingOperations[i];
                if (op.IsSubscribe)
                    AddHandler(op.EventType, op.Handler);
                else
                    RemoveHandler(op.EventType, op.Handler);
            }

            pendingOperations.Clear();
        }

        #endregion
    }
}
