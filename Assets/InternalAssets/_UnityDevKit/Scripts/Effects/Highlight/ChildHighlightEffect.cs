namespace UnityDevKit.Effects
{
    public class ChildHighlightEffect : HighlightEffect
    {
        protected override void Start()
        {
        }

        public void Setup(HighlightEffectProperties highlightEffectProperties)
        {
            properties = highlightEffectProperties;
            Init();
        }
    }
}