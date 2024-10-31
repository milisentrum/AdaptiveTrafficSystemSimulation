using System;
using MyBox;
using UnityEngine;

namespace UnityDevKit.Optimization
{
    [Serializable]
    public class DynamicFrameFilter
    {
        [SerializeField] private bool enabled = true;
        [SerializeField] [ConditionalField(nameof(enabled))] private RangedInt frameFilterBounds = new RangedInt(1, 30);

        [ReadOnly] public int currentFrameFilter = 1;

        private const int DEFAULT_FRAME_STEP = 1;


        public bool IsFilteredFrame() => !enabled || Time.frameCount % currentFrameFilter == 0;

        public void Increase(int frameStep = DEFAULT_FRAME_STEP)
        {
            currentFrameFilter = Mathf.Min(currentFrameFilter + frameStep, frameFilterBounds.Max);
        }

        public void Decrease(int frameStep = DEFAULT_FRAME_STEP)
        {
            currentFrameFilter = Mathf.Max(currentFrameFilter - frameStep, frameFilterBounds.Min);
        }

        public void SetToMax()
        {
            currentFrameFilter = frameFilterBounds.Max;
        }

        public void SetToMin()
        {
            currentFrameFilter = frameFilterBounds.Min;
        }

        private enum StartFrameFilterMode // TODO
        {
            MIN,
            AVERAGE,
            MAX
        }
    }
}