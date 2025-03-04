using MyBox;
using UnityEngine;

namespace AdaptiveTrafficSystem.RoadMarkings
{
    public class RoadMarkingCreator : MonoBehaviour
    {
        [SerializeField] private Transform holder;
        [SerializeField] private GameObject prototype;
        [SerializeField] private Vector3 offset = new Vector3() {z = -3};
        [SerializeField] [PositiveValueOnly] private int count = 10;
        
#if UNITY_EDITOR
        [ButtonMethod]
        public string Generate()
        {
            if (holder.childCount > 0)
            {
                Delete();
            }
            
            for (var i = 0; i < count; i++)
            {
                var segment = Instantiate(prototype, holder);
                segment.transform.localPosition = offset * i;
            }
            return $"Generate road markings with {prototype.name} and {count} count";
        }
        
        [ButtonMethod]
        public string Delete()
        {
            for (var i = holder.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(holder.GetChild(i).gameObject);
            }
            return $"Delete road markings";
        }
#endif
    }
}