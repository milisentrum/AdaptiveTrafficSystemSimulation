using System;
using UnityEngine;

namespace UnityDevKit.Effects
{
    public class HighlightEffect : BaseEffect
    {
        [SerializeField] protected HighlightEffectProperties properties;

        [SerializeField] MeshRenderer meshRenderer;

        private Material _highlightedMaterial;
        private Material _defaultMaterial;

        private static readonly int RimPowerID = Shader.PropertyToID("Vector1_51D2992B");
        private static readonly int RimIntensityID = Shader.PropertyToID("Vector1_9C013D6C");
        private static readonly int RimColorID = Shader.PropertyToID("Color_87739F");
        private static readonly int AlbedoID = Shader.PropertyToID("Texture2D_C389AB95");
        private static readonly int NormalId = Shader.PropertyToID("Texture2D_51375A76");
        private static readonly int MetallicID = Shader.PropertyToID("Texture2D_7C12B9C2");
        private static readonly int OcclusionID = Shader.PropertyToID("Texture2D_998385A3");

        private const float INTENSITY_MODIFIER = 1.20f;

        private void Awake()
        {
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }

            if (meshRenderer == null)
            {
                Debug.LogError("[HighlightEffect] There's no MeshRenderer component");
            }
        }

        protected virtual void Start()
        {
            Init();
            //Invoke(nameof(Init), 1f);
        }

        public override void Apply()
        {
            var newMaterials = new Material[meshRenderer.materials.Length];
            
            for (int i = 0; i < newMaterials.Length; i++)
            {
                newMaterials[i] = _highlightedMaterial;
            }

            meshRenderer.materials = newMaterials;
            base.Apply();
            //_meshRenderer.material = _highlightedMaterial;
        }

        public override void Remove()
        {
            var newMaterials = new Material[meshRenderer.materials.Length];
            
            for (int i = 0; i < newMaterials.Length; i++)
            {
                newMaterials[i] = _defaultMaterial;
            }

            meshRenderer.materials = newMaterials;
            base.Remove();
            //_meshRenderer.material = _defaultMaterial;
        }
        
        public void IncreaseEffectPower(float modifier = INTENSITY_MODIFIER)
        {
            _highlightedMaterial.SetFloat(RimIntensityID, properties.rimIntensity * modifier);
        }

        public void DecreaseEffectPower(float modifier = INTENSITY_MODIFIER)
        {
            _highlightedMaterial.SetFloat(RimIntensityID, properties.rimIntensity / modifier);
        }

        public void SetEffectPower(float power)
        {
            _highlightedMaterial.SetFloat(RimIntensityID, power);
        }
        
        public float GetEffectPower() => _highlightedMaterial.GetFloat(RimIntensityID);

        protected void Init()
        {
            _defaultMaterial = meshRenderer.material;

            HighlightedMaterialInit();
            
            //ApplyEffect();
        }
        

        private void HighlightedMaterialInit()
        {
            _highlightedMaterial = new Material(Shader.Find("Shader Graphs/Highlight"))
            {
                name = $"Highlighted {name}"
            };

            _highlightedMaterial.SetFloat(RimPowerID, properties.rimPower);
            _highlightedMaterial.SetFloat(RimIntensityID, properties.rimIntensity);
            _highlightedMaterial.SetColor(RimColorID, properties.rimColor);

            _highlightedMaterial.SetTexture(AlbedoID, _defaultMaterial.GetTexture("_BaseColorMap"));
            _highlightedMaterial.SetTexture(NormalId, _defaultMaterial.GetTexture("_NormalMap"));
            //_highlightedMaterial.SetTexture(MetallicID, _defaultMaterial.GetTexture("_Metallic"));
            //_highlightedMaterial.SetTexture(OcclusionID, _defaultMaterial.GetTexture("_BaseColor"));
        }

        public HighlightEffectProperties Properties => properties;
    }
}