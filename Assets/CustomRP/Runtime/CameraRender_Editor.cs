using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public partial class CameraRender
{
    partial void DrawUnsupportedShaders();
    partial void DrawGizmos();
    partial void PrepareSceneMesh();
    partial void PrepareBuffer();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private static ShaderTagId s_unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    private static ShaderTagId[] s_legacyShaderTagIds = {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };
    private static Material s_errorMaterial;

    partial void PrepareBuffer()
    {
        m_buffer.name = m_camera.name;
    }

    partial void PrepareSceneMesh()
    {
        if(m_camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(m_camera);
        }
    }

    partial void DrawGizmos()
    {
        if(Handles.ShouldRenderGizmos())
        {
            m_context.DrawGizmos(m_camera, GizmoSubset.PreImageEffects);
            m_context.DrawGizmos(m_camera, GizmoSubset.PostImageEffects);
        }
    }

    partial void DrawUnsupportedShaders()
    {
        if(s_errorMaterial == null)
        {
            s_errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }
        var drawSettings = new DrawingSettings();
        drawSettings.sortingSettings = new SortingSettings(m_camera);
        for(int i = 0; i < s_legacyShaderTagIds.Length; i++)
        {
            drawSettings.SetShaderPassName(i, s_legacyShaderTagIds[i]);
        }
        drawSettings.overrideMaterial = s_errorMaterial;
        var filterSettings = FilteringSettings.defaultValue;

        m_context.DrawRenderers(m_cullingResults, ref drawSettings, ref filterSettings);
    }
#endif
}
