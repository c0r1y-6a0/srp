using UnityEngine;
using UnityEngine.Rendering;

namespace MySRP
{
    public class Shadows
    {
        struct ShadowedDirectionalLight
        {
            public int VisibleLightIndex;
        }

        const int c_MaxShadowedDirectionalLightCount = 4;
        const string c_BufferName = "Shadows";

        static int s_DirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");
        static int s_DirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices");
        static Matrix4x4[] s_DirShadowMatrices = new Matrix4x4[c_MaxShadowedDirectionalLightCount];

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

        public void ReserveDirectionalShadow(Light light, int visibleLightIndex)
        {
            if(m_shadowedDirectionalLightCount < c_MaxShadowedDirectionalLightCount
                && light.shadows != LightShadows.None && light.intensity > 0f
                && m_cullingResult.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
            {
                m_shadowedDirectionalLightInfo[m_shadowedDirectionalLightCount++] = new ShadowedDirectionalLight { VisibleLightIndex = visibleLightIndex };
            }
        }

        public void Render()
        {
            if(m_shadowedDirectionalLightCount > 0)
            {
                RenderDirectionalShadow();
            }
            else
            {
                m_buffer.GetTemporaryRT(s_DirShadowAtlasId, 1, 1, 16, FilterMode.Bilinear, RenderTextureFormat.Shadowmap );
            }
        }

        private void RenderDirectionalShadow()
        {
            int atlasSize = (int)m_shadowSettings.Directional.atlasSize;
            m_buffer.GetTemporaryRT(s_DirShadowAtlasId, atlasSize, atlasSize, 16, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
            m_buffer.SetRenderTarget(s_DirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            m_buffer.ClearRenderTarget(true, true, Color.clear);
            m_buffer.BeginSample(c_BufferName);
            ExecuteBuffer();

            int split = m_shadowedDirectionalLightCount <= 1 ? 1 : 2;
            int tileSize = atlasSize / split;

            for(int i = 0; i < m_shadowedDirectionalLightCount; i++)
            {
                RenderDirectionalShadow(i, split, tileSize);
            }

            m_buffer.SetGlobalMatrixArray(s_DirShadowMatricesId, s_DirShadowMatrices);
            m_buffer.EndSample(c_BufferName);
            ExecuteBuffer();
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

            m_cullingResult.ComputeDirectionalShadowMatricesAndCullingPrimitives(index, 0, 1, Vector3.zero, tileSize, 0f, out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix, out ShadowSplitData shadowSplitData);
            shadowSettings.splitData = shadowSplitData;
            SetViewPort(index, split, tileSize);
            m_buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            ExecuteBuffer();
            m_context.DrawShadows(ref shadowSettings);
        }

        public void CleanUp()
        {
            m_buffer.ReleaseTemporaryRT(s_DirShadowAtlasId);
            ExecuteBuffer();
        }
    }

}