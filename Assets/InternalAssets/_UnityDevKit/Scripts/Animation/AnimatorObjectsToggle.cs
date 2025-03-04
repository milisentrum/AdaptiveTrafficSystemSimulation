using System.Collections.Generic;
using UnityEngine;

namespace UnityDevKit.Animation
{
    public class AnimatorObjectsToggle : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        [SerializeField] private List<GameObject> gameObjects;
        
        public void TurnOnAfterAnim(int clipNumber)
        {
            var clipLength = animator.GetCurrentAnimatorClipInfo(0)[clipNumber].clip.length;
            TurnObjectsOn(clipLength);
        }

        public void TurnOffAfterAnim(int clipNumber)
        {
            var clipLength = animator.GetCurrentAnimatorClipInfo(0)[clipNumber].clip.length;
            TurnObjectsOff(clipLength);
        }

        private void TurnObjectsOff(float seconds)
        {
            Invoke(nameof(TurnObjectsOff), seconds);
        }

        private void TurnObjectsOn(float seconds)
        {
            Invoke(nameof(TurnObjectsOn), seconds);
        }

        private void TurnObjectsOn()
        {
            foreach (var go in gameObjects)
            {
                go.SetActive(true);
            }
        }

        private void TurnObjectsOff()
        {
            foreach (var go in gameObjects)
            {
                go.SetActive(false);
            }
        }
    }
}