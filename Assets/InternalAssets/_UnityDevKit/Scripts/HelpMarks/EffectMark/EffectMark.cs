using UnityEngine;
using UnityDevKit.Effects;

namespace UnityDevKit.HelpMarks
{
    public abstract class EffectMark<TEffect> : MonoBehaviour, IHelpMark
    where TEffect : IEffect
    {
        protected TEffect Effect;

        private void Start()
        {
            Effect = GetComponent<TEffect>();
        }

        public void Show()
        {
            Effect.Apply();
        }

        public void Close()
        {
            Effect.Remove();
        }
    }
}