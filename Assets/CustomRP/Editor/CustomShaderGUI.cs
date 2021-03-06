using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

public class CustomShaderGUI : ShaderGUI
{
	enum ShadowMode
	{
		On, Clip, Dither, Off
	}

	ShadowMode Shadows
	{
		set
		{
			if (SetProperty("_Shadows", (float)value))
			{
				SetKeyword("_SHADOWS_CLIP", value == ShadowMode.Clip);
				SetKeyword("_SHADOWS_DITHER", value == ShadowMode.Dither);
			}
		}
	}

	bool Clipping {
		set => SetProperty("_Clipping", "_CLIPPING", value);
	}

	bool PremultiplyAlpha {
		set => SetProperty("_PremulAlpha", "_PREMULTIPLY_ALPHA", value);
	}

	BlendMode SrcBlend {
		set => SetProperty("_SrcBlend", (float)value);
	}

	BlendMode DstBlend {
		set => SetProperty("_DstBlend", (float)value);
	}

	bool ZWrite {
		set => SetProperty("_ZWrite", value ? 1f : 0f);
	}

    bool HasProperty (string name) =>
		FindProperty(name, m_properties, false) != null;

    bool HasPremultiplyAlpha => HasProperty("_PremulAlpha");

    RenderQueue RenderQueue {
		set {
			foreach (Material m in m_materials) {
				m.renderQueue = (int)value;
			}
		}
	}

    MaterialEditor m_editor;
    Object[] m_materials; 
    MaterialProperty[] m_properties;
    bool showPresets;
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
		EditorGUI.BeginChangeCheck();

        base.OnGUI(materialEditor, properties);
        m_editor = materialEditor;
        m_materials = materialEditor.targets;
        m_properties = properties;

		EditorGUILayout.Space();
		showPresets = EditorGUILayout.Foldout(showPresets, "Presets", true);
		if (showPresets) 
        {
            OpaquePreset();
            ClipPreset();
            FadePreset();
            TransparentPreset();
        }

		if(EditorGUI.EndChangeCheck())
        {
			SetShadowCasterPass();
        }
    }

    private bool SetProperty(string name, float value)
    {
        MaterialProperty property = FindProperty(name, m_properties, false);
		if (property != null) 
        {
			property.floatValue = value;
			return true;
		}
		return false;
    }

    private void SetKeyword(string name, bool enable)
    {
        foreach(Material m in m_materials)
        {
            if(enable)
            {
                m.EnableKeyword(name);
            }
            else
            {
                m.DisableKeyword(name);
            }
        }
    }

    private void SetProperty(string name, string keyword, bool value)
    {
        if(SetProperty(name, value ? 1.0f:0.0f))
        {
            SetKeyword(keyword, value);
        }
    }

    bool PresetButton (string name) {
		if (GUILayout.Button(name)) {
			m_editor.RegisterPropertyChangeUndo(name);
			return true;
		}
		return false;
	}

    void OpaquePreset () {
		if (PresetButton("Opaque")) {
			Clipping = false;
			PremultiplyAlpha = false;
			SrcBlend = BlendMode.One;
			DstBlend = BlendMode.Zero;
			ZWrite = true;
			RenderQueue = RenderQueue.Geometry;
		}
	}

    void ClipPreset () {
		if (PresetButton("Clip")) {
			Clipping = true;
			PremultiplyAlpha = false;
			SrcBlend = BlendMode.One;
			DstBlend = BlendMode.Zero;
			ZWrite = true;
			RenderQueue = RenderQueue.AlphaTest;
		}
	}

	void FadePreset () {
		if (PresetButton("Fade")) {
			Clipping = false;
			PremultiplyAlpha = false;
			SrcBlend = BlendMode.SrcAlpha;
			DstBlend = BlendMode.OneMinusSrcAlpha;
			ZWrite = false;
			RenderQueue = RenderQueue.Transparent;
		}
	}

    void TransparentPreset () {
		if (HasPremultiplyAlpha && PresetButton("Transparent")) {
			Clipping = false;
			PremultiplyAlpha = true;
			SrcBlend = BlendMode.One;
			DstBlend = BlendMode.OneMinusSrcAlpha;
			ZWrite = false;
			RenderQueue = RenderQueue.Transparent;
		}
	}

	void SetShadowCasterPass()
	{
		MaterialProperty shadows = FindProperty("_Shadows", m_properties, false);
		if (shadows == null || shadows.hasMixedValue)
		{
			return;
		}
		bool enabled = shadows.floatValue < (float)ShadowMode.Off;
		foreach (Material m in m_materials)
		{
			m.SetShaderPassEnabled("ShadowCaster", enabled);
		}
	}

}