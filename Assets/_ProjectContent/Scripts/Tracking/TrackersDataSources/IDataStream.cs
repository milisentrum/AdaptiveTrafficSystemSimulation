using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AdaptiveTrafficSystem.Tracking
{
    public interface IDataStream
    {
        IEnumerable<UnityEvent<GameObject>> GetDataInputEvent();
        
        UnityEvent<GameObject> GetDataOutputEvent();
    }
}