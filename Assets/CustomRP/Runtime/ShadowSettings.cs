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

        public enum FilterMode
        {
            PCF2x2, PCF3x3, PCF5x5, PCF7x7
        }

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
            public TextureSize AtlasSize;
            public FilterMode Filter;

            [Range(1, 4)]
            public int CascadeCount;

            [Range(0f, 1f)]
            public float Cascade1Ratio;
            public float Cascade2Ratio;
            public float Cascade3Ratio;

            public Vector3 CascadeRatio => new Vector3(Cascade1Ratio, Cascade2Ratio, Cascade3Ratio);

            [Range(0.01f, 1f)]
            public float CascadeFade;
            public enum CascadeBlendMode
            {
                Hard, Soft, Dither
            }
            public CascadeBlendMode CascadeBlend;
        }

        public DirectionalInfo Directional = new DirectionalInfo { 
            AtlasSize = TextureSize._1024 ,
            Filter = FilterMode.PCF2x2,
            CascadeCount = 4,
            Cascade1Ratio = 0.1f,
            Cascade2Ratio = 0.25f,
            Cascade3Ratio = 0.5f,
            CascadeFade = 0.1f,
            CascadeBlend = DirectionalInfo.CascadeBlendMode.Hard
        };

    }

}
