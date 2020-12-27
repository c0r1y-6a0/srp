using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MySRP
{
    [DisallowMultipleComponent]
    public class PerObjectColor : MonoBehaviour
    {
        private static int s_baseColorId = Shader.PropertyToID("_BaseColor");
        private static int s_cutoffId = Shader.PropertyToID("_Cutoff");
        private static int s_metallicId = Shader.PropertyToID("_Metallic");
		private static int s_smoothnessId = Shader.PropertyToID("_Smoothness");
        private static MaterialPropertyBlock s_matBlock;


        [SerializeField]
        Color BaseColor = Color.white;
        [SerializeField, Range(0f, 1f)]
        float cutoff = 0.5f;
        [SerializeField, Range(0f, 1f)]
        float metallic= 0.5f;
        [SerializeField, Range(0f, 1f)]
        float smoothness = 0.5f;

        private void OnValidate()
        {
            if (s_matBlock == null)
            {
                s_matBlock = new MaterialPropertyBlock();
            }

            s_matBlock.SetColor(s_baseColorId, BaseColor);
            s_matBlock.SetFloat(s_cutoffId, cutoff);
            s_matBlock.SetFloat(s_metallicId, metallic);
            s_matBlock.SetFloat(s_smoothnessId, smoothness);
            GetComponent<Renderer>().SetPropertyBlock(s_matBlock);
        }

        private void Awake()
        {
            OnValidate();
        }

    }
}
