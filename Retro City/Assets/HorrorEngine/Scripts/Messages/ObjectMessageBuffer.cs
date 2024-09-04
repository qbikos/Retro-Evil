using System;
using System.Collections.Generic;
using UnityEngine;

namespace HorrorEngine
{
    public class ObjectMessageBuffer : MonoBehaviour
    {
        private Dictionary<Type, List<Delegate>> mListeners = new Dictionary<Type, List<Delegate>>();

        // --------------------------------------------------------------------

        public void Dispatch<T>(T message) where T : BaseMessage, new()
        {
            List<Delegate> callbacks = null;
            mListeners.TryGetValue(typeof(T), out callbacks);
            if (callbacks == null)
                return;

            for (int i = 0; i < callbacks.Count; ++i)
            {
                MessageBuffer<T>.MessageCallback callback = (MessageBuffer<T>.MessageCallback)callbacks[i];
                callback(message);
            }
        }

        // --------------------------------------------------------------------

        public void Subscribe<T>(MessageBuffer<T>.MessageCallback callback) where T : BaseMessage, new()
        {
            if (!mListeners.ContainsKey(typeof(T)))
            {
                mListeners.Add(typeof(T), new List<Delegate>());
            }

            mListeners[typeof(T)].Add(callback);
        }

        // --------------------------------------------------------------------

        public void Unsubscribe<T>(MessageBuffer<T>.MessageCallback callback) where T : BaseMessage, new()
        {
            if (mListeners.ContainsKey(typeof(T)))
                mListeners[typeof(T)].Remove(callback);
        }
    }
}