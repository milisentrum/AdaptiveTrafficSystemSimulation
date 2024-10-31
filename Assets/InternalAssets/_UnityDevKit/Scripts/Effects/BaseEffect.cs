using UnityEngine;

namespace UnityDevKit.Effects
{
    public abstract class BaseEffect : MonoBehaviour, IEffect
    {
        [SerializeField] protected BaseEffect[] childrenEffects;

        public virtual void Apply()
        {
            foreach (var child in childrenEffects)
            {
                child.Apply();
            }
        }

        public virtual void Remove()
        {
            foreach (var child in childrenEffects)
            {
                child.Remove();
            }
        }
    }
}
