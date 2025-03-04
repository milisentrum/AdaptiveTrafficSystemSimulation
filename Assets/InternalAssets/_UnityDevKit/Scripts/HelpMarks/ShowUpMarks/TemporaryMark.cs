using MyBox;
using UnityEngine;

namespace UnityDevKit.HelpMarks
{
    public class TemporaryMark : CustomMark
    {
        [SerializeField] [PositiveValueOnly] private float lifeTime = 3f;

        public override void Show()
        {
            base.Show();
            CloseAfterDuration(lifeTime);
        }
    }
}