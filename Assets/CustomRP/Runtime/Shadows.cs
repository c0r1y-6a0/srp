using UnityEngine;
using UnityEngine.Rendering;

namespace MySRP
{
    public class Shadows
    {
        struct ShadowedDirectionalLight
        {
            public int VisibleLightIndex;
            public float SlopeScaleBias;

            public float NearPlaneOffset;
        }

        const int c_MaxShadowedDirectionalLightCount = 4;
        const int c_MaxCascades = 4;
        const string c_BufferName = "Shadows";

        static int s_DirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");
        static int s_DirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices");
        static int s_CascadeCountId = Shader.PropertyToID("_CascadeCount");
		static int s_CascadeCullingSpheresId = Shader.PropertyToID("_CascadeCullingSpheres");
        static int s_CascadeDataId = Shader.PropertyToID("_CascadeData");
        static int s_ShadowDistanceFadeId = Shader.PropertyToID("_ShadowDistanceFade");
        static int s_ShadowAtlasSizeId = Shader.PropertyToID("_ShadowAtlasSize");


        static Matrix4x4[] s_DirShadowMatrices = new Matrix4x4[c_MaxShadowedDirectionalLightCount * c_MaxCascades];
        static Vector4[] s_CascadeCullingSpheres = new Vector4[c_MaxCascades];
        static Vector4[] s_CascadeData = new Vector4[c_MaxCascades];

        static string[] s_DirectionalFilterKeywords = { "_DIRECTIONAL_PCF3", "_DIRECTIONAL_PCF5", "_DIRECTIONAL_PCF7", };
        static string[] s_CascadeBlendKeywords = { "_CASCADE_BLEND_SOFT", "_CASCADE_BLEND_DITHER" };


        CommandBuffer m_buffer = new CommandBuffer { name = c_BufferName };
        ScriptableRenderContext m_context;
        CullingResults m_cullingResult;
        ShadowSettings m_shadowSettings;
        int m_shadowedDirectionalLightCount;

        ShadowedDirectionalLight[] m_shadowedDirectionalLightInfo = new ShadowedDirectionalLight[c_MaxShadowedDirectionalLightCount];

        public void Setup(ScriptableRenderContext context, CullingResults results, ShadowSettings settings)
        {
            m_context = context;
            m_cullingResult = results;
            m_shadowSettings = settings;

            m_shadowedDirectionalLightCount = 0;
        }

        public void ExecuteBuffer()
        {
            m_context.ExecuteCommandBuffer(m_buffer);
            m_buffer.Clear();
        }

        public Vector3 ReserveDirectionalShadow(Light light, int visibleLightIndex)
        {
            if(m_shadowedDirectionalLightCount < c_MaxShadowedDirectionalLightCount
                && light.shadows != LightShadows.None && light.intensity > 0f
                && m_cullingResult.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
            {
                m_shadowedDirectionalLightInfo[m_shadowedDirectionalLightCount] = new ShadowedDirectionalLight { VisibleLightIndex = visibleLightIndex , SlopeScaleBias = light.shadowBias, NearPlaneOffset = light.shadowNearPlane};
                return new Vector3(light.shadowStrength, m_shadowSettings.Directional.CascadeCount * m_shadowedDirectionalLightCount++, light.shadowNormalBias);
            }
            return Vector3.zero;
        }

        public void Render()
        {
            if(m_shadowedDirectionalLightCount > 0)
            {
                RenderDirectionalShadows();
            }
            else
            {
                m_buffer.GetTemporaryRT(s_DirShadowAtlasId, 1, 1, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap );
            }
        }

        private void RenderDirectionalShadows()
        {
            int atlasSize = (int)m_shadowSettings.Directional.AtlasSize;
            m_buffer.GetTemporaryRT(s_DirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
            m_buffer.SetRenderTarget(s_DirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            m_buffer.ClearRenderTarget(true, true, Color.clear);
            m_buffer.BeginSample(c_BufferName);
            ExecuteBuffer();

            int tiles = m_shadowedDirectionalLightCount * m_shadowSettings.Directional.CascadeCount;
            int split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;
            int tileSize = atlasSize / split;

            for(int i = 0; i < m_shadowedDirectionalLightCount; i++)
            {
                RenderDirectionalShadow(i, split, tileSize);
            }

            m_buffer.SetGlobalInt(s_CascadeCountId, m_shadowSettings.Directional.CascadeCount);
            float f = 1f - m_shadowSettings.Directional.CascadeFade;
            m_buffer.SetGlobalVector(s_ShadowDistanceFadeId, new Vector4(1 / m_shadowSettings.MaxDistance, 1 / m_shadowSettings.DistanceFade, 1f / (1f - f*f)));
            m_buffer.SetGlobalVectorArray(s_CascadeCullingSpheresId, s_CascadeCullingSpheres);
            m_buffer.SetGlobalVectorArray(s_CascadeDataId, s_CascadeData);
            m_buffer.SetGlobalMatrixArray(s_DirShadowMatricesId, s_DirShadowMatrices);
            SetKeywords(s_DirectionalFilterKeywords, (int)m_shadowSettings.Directional.Filter - 1);
            SetKeywords(s_CascadeBlendKeywords, (int)m_shadowSettings.Directional.CascadeBlend - 1);
            m_buffer.SetGlobalVector(s_ShadowAtlasSizeId, new Vector4(atlasSize, 1f/atlasSize));
            m_buffer.EndSample(c_BufferName);
            ExecuteBuffer();
        }

        Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split)
        {
            if (SystemInfo.usesReversedZBuffer)
            {
                m.m20 = -m.m20;
                m.m21 = -m.m21;
                m.m22 = -m.m22;
                m.m23 = -m.m23;
            }

            float scale = 1f / split;
            m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
            m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
            m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
            m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
            m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
            m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
            m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
            m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
            m.m20 = 0.5f * (m.m20 + m.m30);
            m.m21 = 0.5f * (m.m21 + m.m31);
            m.m22 = 0.5f * (m.m22 + m.m32);
            m.m23 = 0.5f * (m.m23 + m.m33);

            return m;
        }

        Vector2 SetViewPort(int index, int split, float tileSize)
        {
            Vector2 offset = new Vector2(index % split, index / split);
            m_buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
            return offset;
        }

        private void RenderDirectionalShadow(int index, int split, int tileSize)
        {
            var shadowdDirLightInfo = m_shadowedDirectionalLightInfo[index];
            var shadowSettings = new ShadowDrawingSettings(m_cullingResult,shadowdDirLightInfo.VisibleLightIndex);

            int cascadeCount = m_shadowSettings.Directional.CascadeCount;
            int tileOffset = index * cascadeCount;
            Vector3 ratios = m_shadowSettings.Directional.CascadeRatio;

            float cullingFactor = Mathf.Max(0f, 0.8f - m_shadowSettings.Directional.CascadeFade);

            for(int i = 0 ; i < cascadeCount ; i++)
            {
                m_cullingResult.ComputeDirectionalShadowMatricesAndCullingPrimitives(shadowdDirLightInfo.VisibleLightIndex, i, cascadeCount,
                    ratios, tileSize, shadowdDirLightInfo.NearPlaneOffset,
                    out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix, out ShadowSplitData shadowSplitData);
                shadowSplitData.shadowCascadeBlendCullingFactor = cullingFactor;
                shadowSettings.splitData = shadowSplitData;
                if(index == 0) // TODO:这里不能移到循环外面吗？
                {
                    SetCascadeData(i, shadowSplitData.cullingSphere, tileSize);
                }
                int tileIndex = tileOffset + i;
                s_DirShadowMatrices[tileIndex] = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, SetViewPort(tileIndex, split, tileSize), split);
                m_buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
                m_buffer.SetGlobalDepthBias(0, shadowdDirLightInfo.SlopeScaleBias);
                ExecuteBuffer();
                m_context.DrawShadows(ref shadowSettings);
                m_buffer.SetGlobalDepthBias(0f, 0f);
            }
        }

        void SetCascadeData(int index, Vector4 cullingSphere, float tileSize)
        {
            float texelSize = 2f * cullingSphere.w / tileSize;
            float filterSize = texelSize * ((float)m_shadowSettings.Directional.Filter + 1f);
            s_CascadeData[index] = new Vector4(1f / cullingSphere.w, texelSize * filterSize * 1.4142136f);
            cullingSphere.w -= filterSize;
            cullingSphere.w *= cullingSphere.w;
            s_CascadeCullingSpheres[index] = cullingSphere;
        }

        public void CleanUp()
        {
            m_buffer.ReleaseTemporaryRT(s_DirShadowAtlasId);
            ExecuteBuffer();
        }

        void SetKeywords(string[] keywords, int enabledIndex)
        {
            for (int i = 0; i < keywords.Length; i++)
            {
                if (i == enabledIndex)
                {
                    m_buffer.EnableShaderKeyword(keywords[i]);
                }
                else
                {
                    m_buffer.DisableShaderKeyword(keywords[i]);
                }
            }
        }
    }

}