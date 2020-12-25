using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MySRP
{
    [DisallowMultipleComponent]
    public class PerObjectColor : MonoBehaviour
    {
        private static int baseColorId = Shader.PropertyToID("_BaseColor");
        static int cutoffId = Shader.PropertyToID("_Cutoff");
        private static MaterialPropertyBlock matBlock;


        [SerializeField]
        Color BaseColor = Color.white;
        [SerializeField, Range(0f, 1f)]
        float cutoff = 0.5f;

        private void OnValidate()
        {
            if (matBlock == null)
            {
                matBlock = new MaterialPropertyBlock();
            }

            matBlock.SetColor(baseColorId, BaseColor);
            matBlock.SetFloat(cutoffId, cutoff);
            GetComponent<Renderer>().SetPropertyBlock(matBlock);
        }

        private void Awake()
        {
            OnValidate();
        }

    }
}
