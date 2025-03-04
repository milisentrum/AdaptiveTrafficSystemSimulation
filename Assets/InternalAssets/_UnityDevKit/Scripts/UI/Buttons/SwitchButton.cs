using UnityEngine;
using DG.Tweening;
using System;
using MyBox;

namespace UnityDevKit.UI_Handlers.Buttons
{
    public class SwitchButton : MonoBehaviour
    {
        [SerializeField] private GameObject switchBtn;
        [SerializeField] [PositiveValueOnly] private float shiftDuration = 0.2f;

        public void OnSwitchButtonClicked()
        {
            ShiftState();
            SwitchState = CalculateState();
        }

        public int SwitchState { get; private set; } = 1;

        public void SetSwitchState(int state)
        {
            if (SwitchState != state)
            {
                OnSwitchButtonClicked();
            }
        }

        private void ShiftState()
        {
            switchBtn.transform.DOLocalMoveX(-switchBtn.transform.localPosition.x, shiftDuration);
        }

        private int CalculateState() => Math.Sign(-switchBtn.transform.localPosition.x);
    }
}