using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityDevKit.Optimization
{
    public static class AssistanceFunctions
    {
        // ----- API -----
        public static Transform[] CacheTransforms<T>(T[] array)
        {
            if (!typeof(T).IsSubclassOf(typeof(Component)))
            {
                Debug.LogError("[AssistanceFunction] Invalid types of input collection. " +
                               "Party (" + typeof(T).FullName + ") is not a subclass of (Component).");
                return null;
            }

            var len = array.Length;
            var transforms = new Transform[len];
            for (var i = 0; i < len; i++)
            {
                transforms[i] = (array[i] as Component)?.transform;
            }

            return transforms;
        }

        public static Transform[] CacheTransforms<T>(List<T> list)
        {
            if (!typeof(T).IsSubclassOf(typeof(Component)))
            {
                Debug.LogError("[AssistanceFunction] Invalid type of input collection. " +
                               "Party (" + typeof(T).FullName + ") is not a subclass of (Component).");
                return null;
            }

            var len = list.Count;
            var transforms = new Transform[len];
            for (var i = 0; i < len; i++)
            {
                transforms[i] = (list[i] as Component)?.transform;
            }

            return transforms;
        }

        public static T GetEnumByName<T>(string eName)
        {
            return (T) Enum.Parse(typeof(T), eName.ToUpper());
        }

        public static T GetEnumByNameUsual<T>(string eName)
        {
            return (T) Enum.Parse(typeof(T), eName);
        }

        public static GameObject GetRoot(GameObject target)
        {
            var tmpObjectTransform = target.transform;
            while (tmpObjectTransform.parent != null)
            {
                tmpObjectTransform = tmpObjectTransform.parent;
            }

            return tmpObjectTransform.gameObject;
        }
    }
}