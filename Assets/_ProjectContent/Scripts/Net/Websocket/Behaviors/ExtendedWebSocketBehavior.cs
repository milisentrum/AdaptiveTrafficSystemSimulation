using System;
using AdaptiveTrafficSystem.Utils;
using UnityEngine;
using WebSocketSharp.Server;

namespace AdaptiveTrafficSystem.Net.Websocket.Behaviors
{
    public class ExtendedWebSocketBehavior : WebSocketBehavior
    {
        protected static void ExecuteOnMainTread(Action action)
        {
            UnityMainThread.ExecuteInUpdate(action);
        }
        
        protected static void ExecuteOnMainTread<T>(Action<T> action, T arg)
        {
            UnityMainThread.ExecuteInUpdate(action, arg);
        }

        protected void SendAsync(string data)
        {
            SendAsync(data, OnSendCompleted);
        }

        private static void OnSendCompleted(bool completed)
        {
            Debug.Log($"Send to client.\n" + $"Result {completed}");
        }
    }
}