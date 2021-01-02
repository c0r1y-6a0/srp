using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

namespace MySRP
{
    public class Lighting
    {
        private const int c_maxDirectinonalLightCount = 4;
        private const string c_BufferName = "Lighting";
        private static int s_dirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
		private static int s_dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
		private static int s_dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");
        private static int s_dirLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData");

        private static Vector4[] s_dirLightColors = new Vector4[c_maxDirectinonalLightCount];
		private static Vector4[] s_dirLightDirections = new Vector4[c_maxDirectinonalLightCount];
        private static Vector4[] s_dirLightShadowData = new Vector4[c_maxDirectinonalLightCount];

        private CommandBuffer m_buffer = new CommandBuffer { name = c_BufferName };
        private CullingResults m_culling = new CullingResults();

        private Shadows m_shadows = new Shadows();

        public void Setup(ScriptableRenderContext context, CullingResults culling, ShadowSettings settings)
        {
            m_culling = culling;
            m_buffer.BeginSample(c_BufferName);

            m_shadows.Setup(context, culling, settings);
            SetupLights();
            m_shadows.Render();
            m_buffer.EndSample(c_BufferName);

            context.ExecuteCommandBuffer(m_buffer);
            m_buffer.Clear();
        }

        private void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
        {
            s_dirLightColors[index] = visibleLight.finalColor;
            s_dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
            s_dirLightShadowData[index] = m_shadows.ReserveDirectionalShadow(visibleLight.light, index);
        }
        private void SetupLights()
        {
            NativeArray<VisibleLight> visibleLights = m_culling.visibleLights;

            int dirCount = 0;
            for (int i = 0; i < visibleLights.Length ; i++)
            {
                VisibleLight visibleLight = visibleLights[i];
                if (visibleLight.lightType == LightType.Directional) 
                {
                    dirCount += 1;
                    SetupDirectionalLight(i, ref visibleLight);
                    if(dirCount > c_maxDirectinonalLightCount)
                    {
                        break;
                    }
                }
            }

            m_buffer.SetGlobalInt(s_dirLightCountId, visibleLights.Length);
            m_buffer.SetGlobalVectorArray(s_dirLightColorsId, s_dirLightColors);
            m_buffer.SetGlobalVectorArray(s_dirLightDirectionsId, s_dirLightDirections);
            m_buffer.SetGlobalVectorArray(s_dirLightShadowDataId, s_dirLightShadowData);
        }

        public void CleanUp()
        {
            m_shadows.CleanUp();
        }
    }

}