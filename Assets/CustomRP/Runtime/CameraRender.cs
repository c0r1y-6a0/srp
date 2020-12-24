using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRender 
{
    private ScriptableRenderContext m_context;
    private Camera m_camera;

    private const string BUFFERNAME = "Render Camera";
    private CommandBuffer m_buffer = new CommandBuffer { name = BUFFERNAME };

    private CullingResults m_cullingResults;


    public void Render(ScriptableRenderContext context, Camera camera)
    {
        m_context = context;
        m_camera = camera;

        PrepareBuffer();
        PrepareSceneMesh();

        if (!Cull())
        {
            return;
        }

        Setup();
        DrawVisibleGeometry();
        DrawUnsupportedShaders();
        DrawGizmos();
        Submit();
    }

    private void Setup()
    {
        m_context.SetupCameraProperties(m_camera);
        m_buffer.ClearRenderTarget(
            m_camera.clearFlags <= CameraClearFlags.Depth, 
            m_camera.clearFlags == CameraClearFlags.Color, 
            m_camera.clearFlags == CameraClearFlags.Color ? m_camera.backgroundColor.linear : Color.clear);
        m_buffer.BeginSample(BUFFERNAME);
        ExecuteBuffer();
    }

    private void DrawVisibleGeometry()
    {
        var filterSettings = new FilteringSettings(RenderQueueRange.opaque);
        var sortingSettings = new SortingSettings(m_camera) { criteria = SortingCriteria.CommonOpaque };
        var drawSettings = new DrawingSettings(s_unlitShaderTagId, sortingSettings);

        m_context.DrawRenderers(m_cullingResults, ref drawSettings, ref filterSettings);

        m_context.DrawSkybox(m_camera);

        filterSettings.renderQueueRange = RenderQueueRange.transparent;
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawSettings.sortingSettings = sortingSettings;
        m_context.DrawRenderers(m_cullingResults, ref drawSettings, ref filterSettings);
    }

    private void Submit()
    {
        m_buffer.EndSample(BUFFERNAME);
        ExecuteBuffer();
        m_context.Submit();
    }

    private void ExecuteBuffer()
    {
        m_context.ExecuteCommandBuffer(m_buffer);
        m_buffer.Clear();
    }

    bool Cull()
    {
        if(m_camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            m_cullingResults = m_context.Cull(ref p);
            return true;
        }
        return false;
    }

}
