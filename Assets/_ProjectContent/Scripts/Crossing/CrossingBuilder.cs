using System;
using AdaptiveTrafficSystem.Paths;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace AdaptiveTrafficSystem.Crossing
{
    public class CrossingBuilder : MonoBehaviour
    {
        [SerializeField] private Transform holder;
        
        [Header("Lines settings")] 
        [SerializeField] private GameObject line;
        [SerializeField] private Material whiteDecalMaterial;
        [SerializeField] private Material yellowDecalMaterial;
        [SerializeField] private CrossingLinesParameters parameters;
        
        public CrossingLinesParameters Parameters => parameters;
        
        public enum ColorScheme
        {
            WHITE,
            YELLOW,
            WHITE_YELLOW
        }

        public void Build(RoadData data, ColorScheme colorScheme = ColorScheme.WHITE_YELLOW)
        {
            var stepsCount = Mathf.Ceil(data.Width / parameters.Step);

            for (var i = holder.childCount - 1; i >= 0; i--)
            {
                var child = holder.GetChild(i);
#if UNITY_EDITOR
                DestroyImmediate(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
            }

            for (var i = 0; i < stepsCount; i++)
            {
                var newLine = Instantiate(line, holder);

                newLine.transform.Translate(newLine.transform.forward * parameters.Step * i, Space.Self);

                var decalMaterial = colorScheme switch
                {
                    ColorScheme.WHITE => whiteDecalMaterial,
                    ColorScheme.YELLOW => yellowDecalMaterial,
                    ColorScheme.WHITE_YELLOW => i % 2 == 0 ? whiteDecalMaterial : yellowDecalMaterial,
                    _ => throw new ArgumentOutOfRangeException(nameof(colorScheme), colorScheme, null)
                };

                newLine.GetComponent<DecalProjector>().material = decalMaterial;
            }
        }
    }
}