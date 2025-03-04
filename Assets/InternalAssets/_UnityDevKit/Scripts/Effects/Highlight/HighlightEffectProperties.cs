using UnityEngine;

namespace UnityDevKit.Effects
{
    [CreateAssetMenu(fileName = "HighlightEffectProperties", menuName = "Effects/HighlightEffect", order = 0)]
    public class HighlightEffectProperties : ScriptableObject
    {
        public float rimPower = 1;
        public float rimIntensity = 80;
        [ColorUsage(true, true)] public Color rimColor = Color.cyan;
    }
}