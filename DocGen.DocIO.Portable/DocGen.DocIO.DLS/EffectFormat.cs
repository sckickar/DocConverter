using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DocGen.DocIO.DLS;

internal class EffectFormat
{
	private ShadowFormat m_shadowFormat;

	private GlowFormat m_glowFormat;

	private ThreeDFormat m_threeDFormat;

	private Reflection m_reflection;

	private float m_softEdgeRadius;

	private ShapeBase m_shape;

	private ChildShape m_childShape;

	private GroupShape m_groupShape;

	private byte m_flagA;

	internal Dictionary<string, Stream> m_docxProps;

	internal bool IsShadowEffect
	{
		get
		{
			return (m_flagA & 1) != 0;
		}
		set
		{
			m_flagA = (byte)((m_flagA & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool IsGlowEffect
	{
		get
		{
			return (m_flagA & 2) >> 1 != 0;
		}
		set
		{
			m_flagA = (byte)((m_flagA & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal bool IsReflection
	{
		get
		{
			return (m_flagA & 4) >> 2 != 0;
		}
		set
		{
			m_flagA = (byte)((m_flagA & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal bool IsSoftEdge
	{
		get
		{
			return (m_flagA & 8) >> 3 != 0;
		}
		set
		{
			m_flagA = (byte)((m_flagA & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal bool NoSoftEdges
	{
		get
		{
			return (m_flagA & 0x10) >> 4 != 0;
		}
		set
		{
			m_flagA = (byte)((m_flagA & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	internal bool IsSceneProperties
	{
		get
		{
			return (m_flagA & 0x20) >> 5 != 0;
		}
		set
		{
			m_flagA = (byte)((m_flagA & 0xDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	internal bool IsShapeProperties
	{
		get
		{
			return (m_flagA & 0x40) >> 6 != 0;
		}
		set
		{
			m_flagA = (byte)((m_flagA & 0xBFu) | ((value ? 1u : 0u) << 6));
		}
	}

	internal bool IsEffectListItem
	{
		get
		{
			return (m_flagA & 0x80) >> 7 != 0;
		}
		set
		{
			m_flagA = (byte)((m_flagA & 0x7Fu) | ((value ? 1u : 0u) << 7));
		}
	}

	internal ShadowFormat ShadowFormat
	{
		get
		{
			if (m_shadowFormat == null)
			{
				m_shadowFormat = new ShadowFormat(m_shape);
			}
			return m_shadowFormat;
		}
		set
		{
			m_shadowFormat = value;
		}
	}

	internal GlowFormat GlowFormat
	{
		get
		{
			if (m_glowFormat == null)
			{
				m_glowFormat = new GlowFormat(m_shape);
			}
			return m_glowFormat;
		}
		set
		{
			m_glowFormat = value;
		}
	}

	internal ThreeDFormat ThreeDFormat
	{
		get
		{
			if (m_threeDFormat == null)
			{
				m_threeDFormat = new ThreeDFormat(m_shape);
			}
			return m_threeDFormat;
		}
		set
		{
			m_threeDFormat = value;
		}
	}

	internal Reflection ReflectionFormat
	{
		get
		{
			if (m_reflection == null)
			{
				m_reflection = new Reflection(m_shape);
			}
			return m_reflection;
		}
		set
		{
			m_reflection = value;
		}
	}

	internal float SoftEdgeRadius
	{
		get
		{
			return m_softEdgeRadius;
		}
		set
		{
			if (value == 0f)
			{
				NoSoftEdges = true;
			}
			m_softEdgeRadius = value;
		}
	}

	internal Dictionary<string, Stream> DocxProps
	{
		get
		{
			if (m_docxProps == null)
			{
				m_docxProps = new Dictionary<string, Stream>();
			}
			return m_docxProps;
		}
	}

	internal EffectFormat(Shape shape)
	{
		m_shape = shape;
		m_docxProps = new Dictionary<string, Stream>();
	}

	internal EffectFormat(ChildShape shape)
	{
		m_childShape = shape;
		m_docxProps = new Dictionary<string, Stream>();
	}

	internal EffectFormat(GroupShape shape)
	{
		m_groupShape = shape;
		m_docxProps = new Dictionary<string, Stream>();
	}

	internal void Close()
	{
		if (m_shadowFormat != null)
		{
			m_shadowFormat.Close();
			m_shadowFormat = null;
		}
		if (m_glowFormat != null)
		{
			m_glowFormat.Close();
			m_glowFormat = null;
		}
		if (m_reflection != null)
		{
			m_reflection.Close();
			m_reflection = null;
		}
		if (m_threeDFormat != null)
		{
			m_threeDFormat.Close();
			m_threeDFormat = null;
		}
		if (m_docxProps.Count > 0)
		{
			m_docxProps.Clear();
			m_docxProps = null;
		}
	}

	internal bool Compare(EffectFormat effectFormat)
	{
		if (IsShadowEffect != effectFormat.IsShadowEffect || IsGlowEffect != effectFormat.IsGlowEffect || IsReflection != effectFormat.IsReflection || IsSoftEdge != effectFormat.IsSoftEdge || NoSoftEdges != effectFormat.NoSoftEdges || IsSceneProperties != effectFormat.IsSceneProperties || IsShapeProperties != effectFormat.IsShapeProperties || IsEffectListItem != effectFormat.IsEffectListItem || SoftEdgeRadius != effectFormat.SoftEdgeRadius)
		{
			return false;
		}
		if ((ShadowFormat == null && effectFormat.ShadowFormat != null) || (ShadowFormat != null && effectFormat.ShadowFormat == null) || (GlowFormat == null && effectFormat.GlowFormat != null) || (GlowFormat != null && effectFormat.GlowFormat == null) || (ThreeDFormat == null && effectFormat.ThreeDFormat != null) || (ThreeDFormat != null && effectFormat.ThreeDFormat == null) || (ReflectionFormat == null && effectFormat.ReflectionFormat != null) || (ReflectionFormat != null && effectFormat.ReflectionFormat == null))
		{
			return false;
		}
		if (ShadowFormat != null && !ShadowFormat.Compare(effectFormat.ShadowFormat))
		{
			return false;
		}
		if (GlowFormat != null && !GlowFormat.Compare(effectFormat.GlowFormat))
		{
			return false;
		}
		if (ThreeDFormat != null && !ThreeDFormat.Compare(effectFormat.ThreeDFormat))
		{
			return false;
		}
		if (ReflectionFormat != null && !ReflectionFormat.Compare(effectFormat.ReflectionFormat))
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(IsShadowEffect ? "1" : "0;");
		stringBuilder.Append(IsGlowEffect ? "1" : "0;");
		stringBuilder.Append(IsReflection ? "1" : "0;");
		stringBuilder.Append(IsSoftEdge ? "1" : "0;");
		stringBuilder.Append(NoSoftEdges ? "1" : "0;");
		stringBuilder.Append(IsSceneProperties ? "1" : "0;");
		stringBuilder.Append(IsShapeProperties ? "1" : "0;");
		stringBuilder.Append(IsEffectListItem ? "1" : "0;");
		stringBuilder.Append(SoftEdgeRadius + ";");
		if (ShadowFormat != null)
		{
			stringBuilder.Append(ShadowFormat.GetAsString());
		}
		if (GlowFormat != null)
		{
			stringBuilder.Append(GlowFormat.GetAsString());
		}
		if (ThreeDFormat != null)
		{
			stringBuilder.Append(ThreeDFormat.GetAsString());
		}
		if (ReflectionFormat != null)
		{
			stringBuilder.Append(ReflectionFormat.GetAsString());
		}
		return stringBuilder;
	}
}
