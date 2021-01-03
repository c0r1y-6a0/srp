using UnityEngine;

namespace MySRP
{
    [System.Serializable]
    public class ShadowSettings
    {
        [Min(0.01f)]
        public float MaxDistance = 100f;

        [Range(0.01f, 1f)]
        public float DistanceFade = 0.1f;

        public enum TextureSize
        {
            _256 = 256,
            _512 = 512,
            _1024 = 1024,
            _2048 = 2048,
            _4096 = 4096,
            _8192 = 8192
        }

        [System.Serializable]
        public struct DirectionalInfo
        {
            public TextureSize atlasSize;

            [Range(1, 4)]
            public int cascadeCount;

            [Range(0f, 1f)]
            public float cascade1Ratio, cascade2Ratio, cascade3Ratio;

            public Vector3 CascadeRatio => new Vector3(cascade1Ratio, cascade2Ratio, cascade3Ratio);
        }

        public DirectionalInfo Directional = new DirectionalInfo { 
            atlasSize = TextureSize._1024 ,
            cascadeCount = 4,
            cascade1Ratio = 0.1f,
            cascade2Ratio = 0.25f,
            cascade3Ratio = 0.5f
            };

    }

}
